namespace api.DTOs.PostDTOs
{
    public class CreatePostDto
    {
        public string Content { get; set; } = null!;
        public string? PostImg { get; set; }
    }
}
