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

        public async Task<List<Post>> GetFeedAsync(List<string> followingIds, int page, int pageSize)
        {
            if (followingIds == null || followingIds.Count == 0)
                return new List<Post>();

            return await _postsCollection
                .Find(p => followingIds.Contains(p.UserId!))
                .SortByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(string userId, int page, int pageSize)
        {
            return await _postsCollection
                .Find(p => p.UserId == userId)
                .SortByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(string id)
        {
            return await _postsCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, Post updatedPost)
        {
            var result = await _postsCollection.ReplaceOneAsync(p => p.Id == id, updatedPost);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _postsCollection.DeleteOneAsync(p => p.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> ToggleLikeAsync(string postId, string userId)
        {
            var post = await GetPostByIdAsync(postId);
            if (post == null) return false;

            if (post.Likes.Contains(userId))
                post.Likes.Remove(userId);
            else
                post.Likes.Add(userId);
            

            var result = await _postsCollection.ReplaceOneAsync(p => p.Id == postId, post);
            return result.IsAcknowledged && result.ModifiedCount > 0;
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

        public async Task<bool> RemoveCommentAsync(string postId, string commentId)
        {
            var post = await GetPostByIdAsync(postId);
            if (post == null) return false;

            var comment = post.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null) return false;

            post.Comments.Remove(comment);
            var result = await _postsCollection.ReplaceOneAsync(p => p.Id == postId, post);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task UpdateUserInfoInPostsAsync(string userId, string newName, string newImgUrl)
        {
            var filter = Builders<Post>.Filter.Eq(p => p.UserId, userId);
            var update = Builders<Post>.Update
                .Set(p => p.UserName, newName)
                .Set(p => p.UserImgUrl, newImgUrl);
            await _postsCollection.UpdateManyAsync(filter, update);
        }
    }
}
