using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api.Services
{
    public class PostService
    {
        private readonly IMongoCollection<Post> _postsCollection;

        public PostService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _postsCollection = database.GetCollection<Post>(settings.Value.PostsCollection);
        }

        public async Task CreateAsync(Post post)
        {
            post.CreatedAt = DateTime.UtcNow;
            await _postsCollection.InsertOneAsync(post);
        }
    }
}
