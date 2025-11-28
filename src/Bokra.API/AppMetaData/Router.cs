namespace Bokra.API.AppMetaData
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
    }
}
