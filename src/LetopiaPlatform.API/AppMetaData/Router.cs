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
        public const string GoogleLogin = $"{Prefix}/google";
        public const string RefreshToken = $"{Prefix}/RefreshToken";

        public const string SendCode = $"{Prefix}/send-code";
        public const string VerifyEmail = $"{Prefix}/verify-email";
        public const string ResetPassword = $"{Prefix}/reset-password";
        public const string ForgotPassword = $"{Prefix}/forgot-password";
    }

    public static class Users
    {
        public const string Prefix = $"{Rule}/users";
        public const string Me = $"{Prefix}/me";
        public const string Update = $"{Prefix}/me";
        public const string Avatar = $"{Prefix}/me/avatar";
    }


    public static class Communities
    {
        public const string Prefix = $"{Rule}/communities";
        public const string MyCommunities = $"{Prefix}/me";
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

    public static class ProjectCategories
    {
        public const string Prefix = $"{Rule}/ProjectCategories";
        public const string GetCategories = $"{Prefix}";
        public const string Create = $"{Prefix}/Create";
        public const string Update = $"{Prefix}/Update/{{id:guid}}";
        public const string GetCategoryBySlug = $"{Prefix}/{{slug}}";
        public const string GetCategoryStats = $"{Prefix}/stats";
        public const string DeleteCategory = $"{Prefix}/{{id:guid}}";
    }

    public static class Projects
    {
        public const string Prefix = $"{Rule}/projects";

        // Project Operations
        public const string Create = $"{Prefix}/Create";
        public const string Discover = $"{Prefix}/discover";
        public const string GetDetailsById = $"{Prefix}/{{id}}";
        public const string Update = $"{Prefix}/Update/{{id}}";
        public const string Delete = $"{Prefix}/Delete/{{id}}";
    }

    public static class ProjectMembers
    {
        public const string Prefix = $"{Rule}/project-members";

        public const string Join = $"{Prefix}/join/{{projectTitle}}";
        public const string Leave = $"{Prefix}/leave/{{projectTitle}}";

        public const string GetMembers = $"{Prefix}/members/{{projectId}}";
        public const string MyProjects = $"{Prefix}/my-projects";
    }

    public static class Comments
    {
        public const string Prefix = $"{Rule}/comments";

        // Base actions
        public const string Update = $"{Prefix}/{{commentId:guid}}";
        public const string Delete = $"{Prefix}/{{commentId:guid}}";

        // Interactions
        public const string React = $"{Prefix}/{{commentId:guid}}/react";

        // Replies
        public const string GetReplies = $"{Prefix}/{{commentId:guid}}/replies";
    }

    public static class Posts
    {
        public const string Prefix = $"{Rule}/posts";

        // Scoped to Community/Channel
        public const string Base = $"{Rule}/communities/{{communityId:guid}}";
        public const string Create = $"{Base}/channels/{{channelId:guid}}/posts";
        public const string List = $"{Base}/channels/{{channelId:guid}}/posts";
        public const string Pinned = $"{Base}/channels/{{channelId:guid}}/posts/pinned";

        // Resource Specific
        public const string GetById = $"{Prefix}/{{postId:guid}}";
        public const string Update = $"{Prefix}/{{postId:guid}}";
        public const string Delete = $"{Prefix}/{{postId:guid}}";
        public const string TogglePin = $"{Prefix}/{{postId:guid}}/pin";

        // Nested Resources
        public const string Comments = $"{Prefix}/{{postId:guid}}/comments";
        public const string React = $"{Prefix}/{{postId:guid}}/react";
    }

    public static class CommunityTaskCategory
    {
        public const string Prefix = $"{Rule}/CommunityTaskCategory";

        public const string GetAll = $"{Prefix}/{{communityId:guid}}/GetAll";
        public const string Create = $"{Prefix}/{{communityId:guid}}/Create";

        public const string GetCategoryById = $"{Prefix}/{{communityId:guid}}/{{categoryid:guid}}";

        public const string Update = $"{Prefix}/Update/{{id:guid}}";
        public const string Delete = $"{Prefix}/Delete/{{id:guid}}";
    }

    public static class CommunityTask
    {
        public const string Prefix = $"{Rule}/CommunityTask";

        public const string GetAll = $"{Prefix}/{{communityId:guid}}/GetAll";
        public const string Create = $"{Prefix}/{{communityId:guid}}/Create";
        public const string GetTodayProgress = $"{Prefix}/{{communityId:guid}}/GetTodayProgress";

        public const string Update = $"{Prefix}/Update/{{communityTaskid:guid}}";
        public const string Delete = $"{Prefix}/Delete/{{communityTaskid:guid}}";
        public const string Toggle = $"{Prefix}/Toggle/{{taskid:guid}}";
    }
}
