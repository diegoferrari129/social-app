using api.Models;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using MongoDB.Driver;

namespace api.Services
{
    public class ChatService
    {
        private readonly IMongoCollection<Chat> _conversationsCollection;
        private readonly IMongoCollection<Message> _messagesCollection;

        public ChatService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _conversationsCollection = database.GetCollection<Chat>(settings.Value.ChatsCollection);
            _messagesCollection = database.GetCollection<Message>(settings.Value.MessagesCollection);
        }

        public async Task<string> GetOrCreateChatAsync(string userId1, string userId2)
        {
            var existing = await _conversationsCollection
                    .Find(c => c.Participants.Contains(userId1) && c.Participants.Contains(userId2))
                    .FirstOrDefaultAsync();

            if (existing != null)
                return existing.Id!;

            var newConversation = new Chat
            {
                Participants = new List<string> { userId1, userId2 },
                CreatedAt = DateTime.UtcNow,
                LastMessage = "",
                LastMessageTime = DateTime.UtcNow
            };
            await _conversationsCollection.InsertOneAsync(newConversation);

            return newConversation.Id!;
        }

        public async Task<Message> SendMessageAsync(string chatId, string senderId, string text)
        {
            var message = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                Text = text,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _messagesCollection.InsertOneAsync(message);

            var chat = await _conversationsCollection.Find(c => c.Id == chatId).FirstOrDefaultAsync();
            if (chat != null)
            {
                chat.LastMessage = text;
                chat.LastMessageTime = DateTime.UtcNow;
                await _conversationsCollection.ReplaceOneAsync(c => c.Id == chatId, chat);
            }
            return message;
        }

        public async Task<List<Message>> GetMessagesAsync(string chatId, string currentUserId)
        {
            var messages = await _messagesCollection
                .Find(m => m.ChatId == chatId)
                .SortBy(m => m.SentAt)
                .ToListAsync();

            foreach (var msg in messages.Where(m => m.SenderId != currentUserId && !m.IsRead))
            {
                msg.IsRead = true;
                await _messagesCollection.ReplaceOneAsync(m => m.Id == msg.Id, msg);
            }

            return messages;
        }

        public async Task<List<Chat>> GetUserChatsAsync(string userId)
        {
            return await _conversationsCollection.Find(c => c.Participants.Contains(userId))
                .SortByDescending(c => c.LastMessageTime)
                .ToListAsync();
        }

        public async Task MarkMessagesAsReadAsync(string conversationId, string userId)
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.ChatId, conversationId),
                Builders<Message>.Filter.Ne(m => m.SenderId, userId),
                Builders<Message>.Filter.Eq(m => m.IsRead, false)
            );
            var update = Builders<Message>.Update.Set(m => m.IsRead, true);
            await _messagesCollection.UpdateManyAsync(filter, update);
        }
    }
}
