namespace ConsoleUI.Models
{
    /// <summary>
    /// Model for user
    /// </summary>
    public class User
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId       { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string UserName  { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string UserMail  { get; set; }

        /// <summary>
        /// Indicates whether the user has administrative privileges
        /// </summary>
        public bool IsAdmin     { get; set; }
    }
}
