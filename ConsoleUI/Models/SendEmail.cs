namespace ConsoleUI.Models
{
    /// <summary>
    /// Model for send an email
    /// </summary>
    public class SendEmail
    {
        /// <summary>
        /// Task ID
        /// </summary>
        public int TaskId     { get; set; }

        /// <summary>
        /// Mail message
        /// </summary>
        public string Message { get; set; }
    }
}
