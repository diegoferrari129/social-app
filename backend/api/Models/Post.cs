using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? PostImg { get; set; }
        public string? Content { get; set; } = null!;
        public string? UserId { get; set; } = null!;
        public string? UserName { get; set; } = null!;
        public string UserImgUrl { get; set; } = null!;

        public List<string> Likes { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
