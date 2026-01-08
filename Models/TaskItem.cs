using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public TaskItemStatus Status { get; set; } = TaskItemStatus.TODO;

        public TaskPriority Priority { get; set; } = TaskPriority.MEDIUM;

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? AssignedToId { get; set; }

        public User? AssignedTo { get; set; }
    }
}