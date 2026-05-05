namespace api.DTOs.ChatDTOs
{
    public class SendMessageDto
    {
        public string ToUserId { get; set; } = null!;
        public string Text { get; set; } = null!;
    }
}
