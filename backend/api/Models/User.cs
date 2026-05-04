using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? Bio { get; set; }
        public string? ImgUrl { get; set; }

        public List<string> Followers { get; set; } = new();
        public List<string> Following { get; set; } = new();
    }
}
