namespace Blog
{
    public class Configuration
    {
        public static string JwtKey { get; set; } = "Y2LwFwCNf0i2OsBN7Zkk9w==";

        public static string ApiKeyName { get; set; } = "api_key";

        public static string ApiKey { get; set;} = "key";

        public static SmtpConfiguration Smtp = new();

        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; } = 25;
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
