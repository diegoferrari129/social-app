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
        public async Task UpdateAsync(string id, User updatedUser) => await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);
    }
}
