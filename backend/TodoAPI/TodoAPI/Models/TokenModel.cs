namespace TodoAPI.Models
{
    public class TokenModel
    {
        public string TokenValue { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
