using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using api.Models;
using BCrypt.Net;
using MongoDB.Bson;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Post> _postsCollection;
        private readonly IMongoCollection<Notification> _notificationsCollection;
        private readonly IMongoCollection<Chat> _conversationsCollection;
        private readonly IMongoCollection<Message> _messagesCollection;
        private readonly Random _random = new Random();

        public SeedController(IMongoDatabase database)
        {
            _database = database;
            _usersCollection = database.GetCollection<User>("Users");
            _postsCollection = database.GetCollection<Post>("Posts");
            _notificationsCollection = database.GetCollection<Notification>("Notifications");
            _conversationsCollection = database.GetCollection<Chat>("Chats");
            _messagesCollection = database.GetCollection<Message>("Messages");
        }

        [HttpPost("populate")]
        public async Task<IActionResult> PopulateDatabase()
        {
            await ClearAllCollections();

            var users = GenerateRandomUsers(20);
            await _usersCollection.InsertManyAsync(users);

            var posts = GenerateRandomPosts(users);
            await _postsCollection.InsertManyAsync(posts);

            await AddRandomLikes(posts, users);

            await AddRandomComments(posts, users);

            await AddRandomFollows(users);

            await AddRandomChats(users);

            return Ok(new { message = "Database populated with random data" });
        }

        private async Task ClearAllCollections()
        {
            await _usersCollection.DeleteManyAsync(FilterDefinition<User>.Empty);
            await _postsCollection.DeleteManyAsync(FilterDefinition<Post>.Empty);
            await _notificationsCollection.DeleteManyAsync(FilterDefinition<Notification>.Empty);
            await _conversationsCollection.DeleteManyAsync(FilterDefinition<Chat>.Empty);
            await _messagesCollection.DeleteManyAsync(FilterDefinition<Message>.Empty);
        }

        private List<User> GenerateRandomUsers(int count)
        {
            var users = new List<User>();
            for (int i = 1; i <= count; i++)
            {
                string name = $"User_{i}";
                string gender = i % 2 == 0 ? "women" : "men";
                users.Add(new User
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = name,
                    Email = $"{name.ToLower()}@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Bio = RandomString(50),
                    ImgUrl = $"https://randomuser.me/api/portraits/{gender}/{i % 70}.jpg",
                    Followers = new List<string>(),
                    Following = new List<string>()
                });
            }
            return users;
        }

        private List<Post> GenerateRandomPosts(List<User> users)
        {
            var posts = new List<Post>();
            foreach (var user in users)
            {
                int postCount = _random.Next(3, 6);
                for (int i = 0; i < postCount; i++)
                {
                    posts.Add(new Post
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        Content = RandomString(_random.Next(80, 200)),
                        PostImg = _random.NextDouble() > 0.7 ? $"https://picsum.photos/id/{_random.Next(1, 100)}/200/150" : null,
                        UserId = user.Id,
                        UserName = user.Name,
                        UserImgUrl = user.ImgUrl,
                        Likes = new List<string>(),
                        Comments = new List<Comment>(),
                        CreatedAt = DateTime.UtcNow.AddDays(-_random.Next(0, 30))
                    });
                }
            }
            return posts;
        }

        private async Task AddRandomLikes(List<Post> posts, List<User> users)
        {
            foreach (var post in posts)
            {
                int likeCount = _random.Next(5, 16);
                var likedBy = users.OrderBy(x => _random.Next()).Take(likeCount).Select(u => u.Id).ToList();
                post.Likes = likedBy;
                await _postsCollection.ReplaceOneAsync(p => p.Id == post.Id, post);
            }
        }

        private async Task AddRandomComments(List<Post> posts, List<User> users)
        {
            foreach (var post in posts)
            {
                int commentCount = _random.Next(1, 4);
                for (int i = 0; i < commentCount; i++)
                {
                    var user = users[_random.Next(users.Count)];
                    var comment = new Comment
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        UserId = user.Id,
                        UserName = user.Name,
                        Text = RandomString(_random.Next(20, 80)),
                        CreatedAt = DateTime.UtcNow.AddMinutes(-_random.Next(0, 10080))
                    };
                    post.Comments.Add(comment);
                }
                await _postsCollection.ReplaceOneAsync(p => p.Id == post.Id, post);
            }
        }

        private async Task AddRandomFollows(List<User> users)
        {
            foreach (var user in users)
            {
                int followCount = _random.Next(5, 16);
                var following = users.Where(u => u.Id != user.Id).OrderBy(x => _random.Next()).Take(followCount).Select(u => u.Id).ToList();
                user.Following = following;
                await _usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

                foreach (var followedId in following)
                {
                    var followed = users.First(u => u.Id == followedId);
                    if (followed.Followers == null) followed.Followers = new List<string>();
                    if (!followed.Followers.Contains(user.Id))
                        followed.Followers.Add(user.Id);
                    await _usersCollection.ReplaceOneAsync(u => u.Id == followed.Id, followed);
                }
            }
        }

        private async Task AddRandomChats(List<User> users)
        {
            var pairs = new HashSet<(string, string)>();
            int chatCount = Math.Min(20, users.Count * (users.Count - 1) / 2);
            for (int i = 0; i < chatCount; i++)
            {
                var user1 = users[_random.Next(users.Count)];
                var user2 = users[_random.Next(users.Count)];
                if (user1.Id == user2.Id) continue;
                var pair = (user1.Id, user2.Id);
                var reversed = (user2.Id, user1.Id);
                if (pairs.Contains(pair) || pairs.Contains(reversed)) continue;
                pairs.Add(pair);

                var chat = new Chat
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Participants = new List<string> { user1.Id, user2.Id },
                    CreatedAt = DateTime.UtcNow,
                    LastMessage = "",
                    LastMessageTime = DateTime.UtcNow
                };
                await _conversationsCollection.InsertOneAsync(chat);

                int msgCount = _random.Next(1, 6);
                for (int j = 0; j < msgCount; j++)
                {
                    var sender = _random.Next(2) == 0 ? user1 : user2;
                    var message = new Message
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        ChatId = chat.Id,
                        SenderId = sender.Id,
                        Text = RandomString(_random.Next(20, 100)),
                        SentAt = DateTime.UtcNow.AddMinutes(-_random.Next(0, 10080)),
                        IsRead = _random.Next(2) == 0
                    };
                    await _messagesCollection.InsertOneAsync(message);
                }

                var lastMsg = await _messagesCollection.Find(m => m.ChatId == chat.Id).SortByDescending(m => m.SentAt).FirstOrDefaultAsync();
                if (lastMsg != null)
                {
                    chat.LastMessage = lastMsg.Text;
                    chat.LastMessageTime = lastMsg.SentAt;
                    await _conversationsCollection.ReplaceOneAsync(c => c.Id == chat.Id, chat);
                }
            }
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}