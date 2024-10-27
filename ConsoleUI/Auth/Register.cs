namespace ConsoleUI.Auth
{
    /// <summary>
    /// Model for handle the user registration
    /// </summary>
    public class Register
    {
        /// <summary>
        /// New user name
        /// </summary>
        public string? UserName  { get; set; }

        /// <summary>
        /// New user first name
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// New user email address
        /// </summary>
        public string? Email     { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string? Password  { get; set; }

        /// <summary>
        /// User role
        /// </summary>
        public string? Role      { get; set; }
    }
}
