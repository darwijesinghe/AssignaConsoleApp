namespace ConsoleUI.Auth
{
    /// <summary>
    /// Model for handle the JWT tokens
    /// </summary>
    public class Keys
    {
        /// <summary>
        /// Access token
        /// </summary>
        public static string? AccessToken  { get; set; }

        /// <summary>
        /// Refresh token
        /// </summary>
        public static string? RefreshToken { get; set; }

        /// <summary>
        /// Password reset token
        /// </summary>
        public static string? ResetToken   { get; set; }

        /// <summary>
        /// User role
        /// </summary>
        public static string? Role         { get; set; }
    }
}
