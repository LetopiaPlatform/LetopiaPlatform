namespace LetopiaPlatform.API.AppMetaData;

public static class Router
{
    public const string Root = "/api";
    public const string Version = "v1";
    public const string Rule = $"{Root}/{Version}";

    public static class Authentication
    {
        public const string Prefix = $"{Rule}/auth";
        public const string SignUp = $"{Prefix}/signup";
        public const string Login = $"{Prefix}/login";
    }

    public static class Users
    {
        public const string Prefix = $"{Rule}/users";
        public const string Me = $"{Prefix}/me";
        public const string Update = $"{Prefix}/me";

        // File operations scoped to the current user
        public const string UploadFile = $"{Prefix}/me/files";
        public const string DeleteFile = $"{Prefix}/me/files";
    }

    public static class Communities
    {
        public const string Prefix = $"{Rule}/communities";
        public const string Create = Prefix;
        public const string List = Prefix;
        public const string GetBySlug = $"{Prefix}/{{slug}}";
        public const string Update = $"{Prefix}/{{id}}";
        public const string Join = $"{Prefix}/{{id}}/join";
        public const string Leave = $"{Prefix}/{{id}}/leave";
        public const string Members = $"{Prefix}/{{id}}/members";
        public const string ChangeRole = $"{Prefix}/{{id}}/members/{{userId}}/role";
    }
    
    public static class Categories
    {
        public const string Prefix = $"{Rule}/categories";
        public const string GetByType = $"{Prefix}";
        public const string GetBySlug = $"{Prefix}/{{slug}}";
        public const string Update = $"{Prefix}/{{id}}";
        public const string Delete = $"{Prefix}/{{id}}";
    }
}