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
        public const string UploadMultipleFiles = $"{Prefix}/me/files/batch";
        public const string ReplaceFile = $"{Prefix}/me/files";
        public const string DeleteFile = $"{Prefix}/me/files";
    }
}
