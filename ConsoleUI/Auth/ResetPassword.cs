namespace ConsoleUI.Auth
{
    /// <summary>
    /// Model for handle the reset password
    /// </summary>
    public class ResetPassword
    {
        /// <summary>
        /// Password
        /// </summary>
        public string? Password    { get; set; }

        /// <summary>
        /// Confirm password
        /// </summary>
        public string? ConPassword { get; set; }

        /// <summary>
        /// Password reset token
        /// </summary>
        public string? ResetToken  { get; set; }
    }
}
