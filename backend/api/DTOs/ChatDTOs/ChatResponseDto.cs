namespace api.DTOs.ChatDTOs
{
    public class ChatResponseDto
    {
        public string Id { get; set; } = null!;
        public string OtherUserName { get; set; } = null!;
        public string OtherUserId { get; set; } = null!;
        public string OtherUserImgUrl { get; set; } = null!;
        public string? LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}
