namespace TodoAPI.Models
{
    public class Todo
    {
        public int Id { get; set; }
        // Legacy todos have no known owner and remain inaccessible through the API.
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsCompleted { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdateDate { get; set; }
    }
}
