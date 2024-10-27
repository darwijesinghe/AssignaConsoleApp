namespace ConsoleUI.Models
{
    /// <summary>
    /// Model to add a note for task
    /// </summary>
    public class AddNote
    {
        /// <summary>
        /// Task ID
        /// </summary>
        public int TaskId       { get; set; }

        /// <summary>
        /// Task note
        /// </summary>
        public string? UserNote { get; set; }
    }
}
