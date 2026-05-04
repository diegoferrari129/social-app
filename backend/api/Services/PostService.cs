using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

        public async Task<List<Post>> GetPostsByUserIdAsync(string userId, int page = 1, int pageSize = 10)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.UserId, userId);
            return await _postsCollection.Find(filter)
                .SortByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(string id)
        {
            return await _postsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> AddCommentAsync(string postId, Comment comment)
        {
            comment.Id = ObjectId.GenerateNewId().ToString();
            comment.CreatedAt = DateTime.UtcNow;
            var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
            var update = Builders<Post>.Update.Push(p => p.Comments, comment);
            var result = await _postsCollection.UpdateOneAsync(filter, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
