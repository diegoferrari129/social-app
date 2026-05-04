namespace api.DTOs
{
    public class CreatePostDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? PostImg { get; set; }
    }
}
