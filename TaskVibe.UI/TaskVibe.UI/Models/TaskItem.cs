using System;

namespace TaskVibe.UI.Models
{
    public class TaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "In Process";
        public int? AssignedToUserId { get; set; }
    }
}