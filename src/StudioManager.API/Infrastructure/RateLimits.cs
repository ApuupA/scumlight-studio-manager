namespace StudioManager.API.Infrastructure
{
    /// <summary>
    /// Names of the rate limiting policies (registered in Program.cs, used with [EnableRateLimiting(...)]).
    /// </summary>
    public static class RateLimits
    {
        /// <summary>
        /// Login: at most 10 attempts per 15 minutes per IP address, so passwords can not be guessed by trying.
        /// </summary>
        public const string Login = "login";

        /// <summary>
        /// Public pages without login (band gear link): at most 60 requests per minute per IP address.
        /// </summary>
        public const string Public = "public";
    }
}
