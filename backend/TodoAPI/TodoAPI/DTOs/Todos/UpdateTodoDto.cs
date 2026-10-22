using System.ComponentModel.DataAnnotations;

namespace TodoAPI.DTOs.Todos
{
    public class UpdateTodoDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool? IsCompleted { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateDate { get; set; }
    }
}
