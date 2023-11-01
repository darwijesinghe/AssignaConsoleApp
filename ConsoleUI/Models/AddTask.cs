namespace ConsoleUI.Models
{

    public class AddTask
    {
        // add a new task
        public string? TskTitle { get; set; }
        public int TskCategory { get; set; }
        public string? Deadline { get; set; }
        public string? Priority { get; set; }
        public int Member { get; set; }
        public string? TskNote { get; set; }
    }
}
