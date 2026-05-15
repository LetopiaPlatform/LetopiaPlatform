using System.Text;
using Amazon.S3;
using LetopiaPlatform.Core.AppSettings;
using LetopiaPlatform.Core.Entities.Identity;
using LetopiaPlatform.Core.Interfaces;
using LetopiaPlatform.Core.Interfaces.Repositories;
using LetopiaPlatform.Core.Services.Interfaces;
using LetopiaPlatform.Infrastructure.Data;
using LetopiaPlatform.Infrastructure.Identity;
using LetopiaPlatform.Infrastructure.Repositories;
using LetopiaPlatform.Infrastructure.Services;
using LetopiaPlatform.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace LetopiaPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDatabase(configuration, environment);
        services.AddIdentitySystem();
        services.AddJwtAuthentication(configuration, environment);
        services.AddAppServices(configuration);
        services.AddHealthCheckServices(configuration);
        services.AddScoped<IProjectCategoryRepository, ProjectCategoryRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        services.AddScoped<ICommunityTaskCategoryRepository, CommunityTaskCategoryRepository>();
        services.AddScoped<ICommunityTaskRepository, CommunityTaskRepository>();
        services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
        return services;
    }

    // -----------------------------------------------------------
    // Database
    // -----------------------------------------------------------
    //private static IServiceCollection AddDatabase(
    //    this IServiceCollection services,
    //    IConfiguration configuration)
    //{
    //    services.AddDbContext<ApplicationDbContext>(options =>
    //        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    //    return services;
    //}
    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            // Testing environment overrides DbContext, skip PostgreSQL setup
            return services;
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 1. إعداد الـ DataSourceBuilder لتفعيل الـ JSON الديناميكي
        var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson(); // السطر ده هو "كلمة السر" لحل مشكلة الـ Milestones
        var dataSource = dataSourceBuilder.Build();

        // 2. تسجيل الـ DbContext باستخدام الـ dataSource الجديد
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dataSource));

        return services;
    }
    // -----------------------------------------------------------
    // Identity
    // -----------------------------------------------------------
    private static IServiceCollection AddIdentitySystem(
        this IServiceCollection services)
    {
        services.AddIdentity<User, Role>(options =>
        {
            // Password policy
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 4;

            // Lockout policy
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddAuthorization();
        return services;
    }

    // -----------------------------------------------------------
    // JWT
    // -----------------------------------------------------------
    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings section missing.");

        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = !environment.IsDevelopment(); // true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    // -----------------------------------------------------------
    // App Services
    // -----------------------------------------------------------
    private static IServiceCollection AddAppServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IUnitOfWork<ApplicationDbContext>>());
        services.AddHttpClient();
        services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddFileStorage(configuration);
        services.AddScoped<ICommunityRepository, CommunityRepository>();
        services.AddScoped<ICommunityService, CommunityService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddEmailService(configuration);

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IReactionRepository, ReactionRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IPostAuthorizationService, PostAuthorizationService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IReactionService, ReactionService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProjectCategoryService, ProjectCategoryService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectMemberService, ProjectMemberService>();
        services.AddScoped<ICommunityTaskCategoryService, CommunityTaskCategoryService>();
        services.AddScoped<IRoadmapRepository, RoadmapRepository>();
        services.AddScoped<IRoadmapService, RoadmapService>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<ICommunityTaskService, CommunityTaskService>();

        return services;
    }

    private static IServiceCollection AddHealthCheckServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string missing.");

        services.AddHealthChecks()
            .AddNpgSql(
                connectionString,
                name: "postgresql",
                tags: ["db", "ready"]);

        return services;
    }

    private static IServiceCollection AddFileStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(FileStorageSettings.SectionName)
        .Get<FileStorageSettings>() ?? new FileStorageSettings();

        services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));

        if (settings.Provider.Equals("R2", StringComparison.OrdinalIgnoreCase))
        {
            var r2 = settings.R2;

            services.AddSingleton<IAmazonS3>(_ =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = $"https://{r2.AccountId}.r2.cloudflarestorage.com",
                    ForcePathStyle = true
                };

                return new AmazonS3Client(r2.AccessKeyId, r2.SecretAccessKey, config);
            });

            services.AddScoped<IFileStorageService, R2FileStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        return services;
    }

    private static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.AddSingleton<SmtpEmailService>();
        services.AddSingleton<EmailBackgroundQueue>();
        services.AddSingleton<IEmailService>(sp => sp.GetRequiredService<EmailBackgroundQueue>());
        services.AddHostedService(sp => sp.GetRequiredService<EmailBackgroundQueue>());

        return services;
    }
}
