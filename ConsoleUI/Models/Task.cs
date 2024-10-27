using System;

namespace ConsoleUI.Models
{
    /// <summary>
    /// Model for task
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Task ID
        /// </summary>
        public int TaskId          { get; set; }

        /// <summary>
        /// Task title
        /// </summary>
        public string TaskTitle    { get; set; }

        /// <summary>
        /// Task deadline
        /// </summary>
        public DateTime Deadline   { get; set; }

        /// <summary>
        /// Task note
        /// </summary>
        public string TaskNote     { get; set; }

        /// <summary>
        /// Pending task
        /// </summary>
        public bool Pending        { get; set; }

        /// <summary>
        /// Completed task
        /// </summary>
        public bool Complete       { get; set; }

        /// <summary>
        /// High priority task
        /// </summary>
        public bool HighPriority   { get; set; }

        /// <summary>
        /// Medium priority task
        /// </summary>
        public bool MediumPriority { get; set; }

        /// <summary>
        /// Low priority task
        /// </summary>
        public bool LowPriority    { get; set; }

        /// <summary>
        /// User note for the task
        /// </summary>
        public string UserNote    { get; set; }

        /// <summary>
        /// Category ID
        /// </summary>
        public int CatId           { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        public string CatName      { get; set; }

        /// <summary>
        /// Task asignee ID
        /// </summary>
        public int UserId          { get; set; }

        /// <summary>
        /// Task asignee first name
        /// </summary>
        public string FirstName    { get; set; }
    }
}
