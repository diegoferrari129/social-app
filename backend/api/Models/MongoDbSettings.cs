namespace api.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollection { get; set; } = null!;
        public string PostsCollection { get; set; } = null!;
        public string ChatsCollection { get; set; } = null!;
        public string MessagesCollection { get; set; } = null!;
    }
}
