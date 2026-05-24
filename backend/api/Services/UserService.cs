using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _usersCollection = database.GetCollection<User>(settings.Value.UsersCollection);
        }

        public async Task<List<User>> GetAllAsync() => await _usersCollection.Find(_ => true).ToListAsync();
        public async Task<User?> GetByIdAsync(string id) => await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        public async Task CreateAsync(User user) => await _usersCollection.InsertOneAsync(user);
        public async Task<User?> GetByEmailAsync(string email) => await _usersCollection.Find(x => x.Email == email).FirstOrDefaultAsync();
        public async Task<bool> UpdateAsync(string id, User updatedUser)
        {
            var result = await _usersCollection.ReplaceOneAsync(u => u.Id == id, updatedUser);
            if (result.IsAcknowledged && result.ModifiedCount > 0)
                return true;   
            else
                return false;
        }

        public async Task<List<User>> GetSuggestedUsersAsync(string currentUserId, int limit = 10)
        {
            var currentUser = await GetByIdAsync(currentUserId);
            if (currentUser == null)
                return new List<User>();

            var excludedIds = new List<string> { currentUserId };
            excludedIds.AddRange(currentUser.Following ?? new List<string>());

            var filter = Builders<User>.Filter.Where(u => !excludedIds.Contains(u.Id!));

            return await _usersCollection.Find(filter).Limit(limit).ToListAsync();
        }

        public async Task<bool> DeleteAsync(string userId)
        {
            var deleteResult = await _usersCollection.DeleteOneAsync(u => u.Id == userId);
            if (!deleteResult.IsAcknowledged || deleteResult.DeletedCount == 0)
                return false;

            var updateFollowingFilter = Builders<User>.Filter.AnyIn(u => u.Following, new[] { userId });
            var updateFollowing = Builders<User>.Update.Pull(u => u.Following, userId);
            await _usersCollection.UpdateManyAsync(updateFollowingFilter, updateFollowing);

            var updateFollowersFilter = Builders<User>.Filter.AnyIn(u => u.Followers, new[] { userId });
            var updateFollowers = Builders<User>.Update.Pull(u => u.Followers, userId);
            await _usersCollection.UpdateManyAsync(updateFollowersFilter, updateFollowers);

            return true;
        }

        public async Task<List<User>> SearchByNameAsync(string query)
        {
            var filter = Builders<User>.Filter.Regex(u => u.Name, new MongoDB.Bson.BsonRegularExpression(query, "i"));
            return await _usersCollection.Find(filter).Limit(10).ToListAsync();
        }
    }
}
