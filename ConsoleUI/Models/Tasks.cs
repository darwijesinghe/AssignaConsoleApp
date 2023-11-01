using System;

namespace ConsoleUI.Models
{
    public class Tasks
    {
        // task
        public int TskId { get; set; }
        public string? TskTitle { get; set; } 
        public DateTime Deadline { get; set; }
        public string? TskNote { get; set; }
        public string? CatName { get; set; }
        public string? FirstName { get; set; }
        public bool Pending { get; set; }
        public bool Complete { get; set; }
        public bool PriHigh { get; set; }
        public bool PriMedium { get; set; }
        public bool PriLow { get; set; }
        public string? UserNote { get; set; }
        public int CatId { get; set; }
        public int UserId { get; set; }
    }
}
