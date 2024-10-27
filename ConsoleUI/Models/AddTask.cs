namespace ConsoleUI.Models
{
    /// <summary>
    /// Model for add a new task
    /// </summary>
    public class AddTask
    {
        /// <summary>
        /// Task title
        /// </summary>
        public string TaskTitle { get; set; }

        /// <summary>
        /// Task category
        /// </summary>
        public int TaskCategory { get; set; }

        /// <summary>
        /// Task deadline
        /// </summary>
        public string Deadline  { get; set; }

        /// <summary>
        /// Task priority
        /// </summary>
        public string Priority  { get; set; }

        /// <summary>
        /// Task assignee
        /// </summary>
        public int Member       { get; set; }

        /// <summary>
        /// Task note
        /// </summary>
        public string TaskNote  { get; set; }
    }
}
