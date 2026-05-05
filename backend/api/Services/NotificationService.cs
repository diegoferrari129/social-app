using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api.Services
{
    public class NotificationService
    {
        private readonly IMongoCollection<Notification> _notificationsCollection;

        public NotificationService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _notificationsCollection = database.GetCollection<Notification>(settings.Value.NotificationsCollection);
        }

        public async Task CreateAsync(Notification notification)
        {
            await _notificationsCollection.InsertOneAsync(notification);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _notificationsCollection
                .Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(string notificationId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            await _notificationsCollection.UpdateOneAsync(filter, update);
        }
    }
}
