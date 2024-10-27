namespace ConsoleUI.Auth
{
    /// <summary>
    /// Represents a model for handling a forgot password request
    /// </summary>
    public class ForgotPassword
    {
        /// <summary>
        /// Email address associated with the account for password recovery
        /// </summary>
        public string? Email { get; set; }
    }

}
