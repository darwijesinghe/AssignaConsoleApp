namespace ConsoleUI.Models
{
    public class EditTask
    {
        // edit a task
        public int tsk_id { get; set; }
        public string? tsk_title { get; set; }
        public int tsk_category { get; set; }
        public string? deadline { get; set; }
        public string? priority { get; set; }
        public int member { get; set; }
        public string? tsk_note { get; set; }
    }
}