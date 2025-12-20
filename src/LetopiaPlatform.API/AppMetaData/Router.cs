namespace LetopiaPlatform.API.AppMetaData
{
    public static class Router
    {

        public const string SingleRoute = "/{id}";

        public const string Root = "/api";
        public const string Version = "v1";
        public const string Rule = $"{Root}/{Version}";

        public static class Authentication
        {

            public const string Prefix = $"{Rule}/auth";
            public const string SignUp = $"{Prefix}/signup";
            public const string Login =  $"{Prefix}/login";
        }
        public static class User
        {
            public const string Prefix = $"{Rule}/user";
            public const string Me = $"{Prefix}/me";
            public const string Update = $"{Prefix}/update";

        }
        public static class File
        {
            public const string Prefix = $"{Rule}/file";
            public const string Upload = $"{Prefix}/upload";
            public const string UploadMultiple = $"{Prefix}/upload-multiple";
            public const string Replace = $"{Prefix}/replace";
            public const string Delete = $"{Prefix}/delete";

        }
    }
}
