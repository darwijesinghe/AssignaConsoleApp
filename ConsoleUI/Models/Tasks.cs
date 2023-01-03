using System;

namespace ConsoleUI.Models
{
    public class Tasks
    {
        public int tsk_id { get; set; }
        public string? tsk_title { get; set; } 
        public DateTime deadline { get; set; }
        public string? tsk_note { get; set; }
        public string? cat_name { get; set; }
        public string? first_name { get; set; }
        public bool pending { get; set; }
        public bool complete { get; set; }
        public bool pri_high { get; set; }
        public bool pri_medium { get; set; }
        public bool pri_low { get; set; }
        public string? user_note { get; set; }
        public int cat_id { get; set; }
        public int user_id { get; set; }
    }
}
