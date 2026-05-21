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
            var bioSamples = new[]
            {
                "Software developer passionate about coding and open source.",
                "Loves hiking, photography, and good coffee.",
                "Tech enthusiast and lifelong learner.",
                "Digital artist exploring new creative horizons.",
                "Foodie and travel addict. Always on the go.",
                "Fitness lover and wellness coach.",
                "Music producer and sound designer.",
                "Bookworm and fantasy novel lover.",
                "Yoga teacher, mindfulness advocate.",
                "Startup founder and entrepreneur.",
                "Gamer and tech reviewer.",
                "Nature lover and environmental activist.",
                "History buff and museum enthusiast.",
                "Science nerd, astronomy fan.",
                "Pet lover with two cats",
                "\"On the other hand, we denounce with righteous indignation and dislike men who are so beguiled and demoralized by the charms of pleasure of the moment, so blinded by desire, that they cannot foresee the pain and trouble that are bound to ensue; and equal blame belongs to those who fail in their duty through weakness of will, which is the same as saying through shrinking from toil and pain. These cases are perfectly simple and easy to distinguish. In a free hour, when our power of choice is untrammelled and when nothing prevents our being able to do what we like best, every pleasure is to be welcomed and every pain avoided. But in certain circumstances and owing to the claims of duty or the obligations of business it will frequently occur that pleasures have to be repudiated and annoyances accepted. The wise man therefore always holds in these matters to this principle of selection: he rejects pleasures to secure other greater pleasures, or else he endures pains to avoid worse pains.\""
            };

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
                    Bio = bioSamples[_random.Next(bioSamples.Length)],
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
            var sampleTexts = new[]
            {
                "What a fantastic day!",
                "I'm learning Angular and I love it.",
                "Anyone want to hang out tonight?",
                "My new project is almost finished!",
                "What do you think about the latest update?",
                "Programming is my passion.",
                "Just watched a beautiful sunset.",
                "Any recommendations for a vacation?",
                "Coffee in the morning is my fuel.",
                "Reading an interesting book right now.",
                "Who won the game yesterday?",
                "Finally, the weekend!",
                "My favorite song is...",
                "Trying out a new recipe today.",
                "Happy to be part of this community.",
                "\"On the other hand, we denounce with righteous indignation and dislike men who are so beguiled and demoralized by the charms of pleasure of the moment, so blinded by desire, that they cannot foresee the pain and trouble that are bound to ensue; and equal blame belongs to those who fail in their duty through weakness of will, which is the same as saying through shrinking from toil and pain. These cases are perfectly simple and easy to distinguish. In a free hour, when our power of choice is untrammelled and when nothing prevents our being able to do what we like best, every pleasure is to be welcomed and every pain avoided. But in certain circumstances and owing to the claims of duty or the obligations of business it will frequently occur that pleasures have to be repudiated and annoyances accepted. The wise man therefore always holds in these matters to this principle of selection: he rejects pleasures to secure other greater pleasures, or else he endures pains to avoid worse pains.\""
            };
            var random = new Random();

            foreach (var user in users)
            {
                int postCount = random.Next(3, 6);
                for (int i = 0; i < postCount; i++)
                {
                    posts.Add(new Post
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        Content = sampleTexts[random.Next(sampleTexts.Length)],
                        PostImg = random.NextDouble() > 0.7 ? $"https://picsum.photos/id/{random.Next(1, 100)}/200/150" : null,
                        UserId = user.Id,
                        UserName = user.Name,
                        UserImgUrl = user.ImgUrl,
                        Likes = new List<string>(),
                        Comments = new List<Comment>(),
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
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
            var commentTexts = new[]
            {
                "Nice!", "Agreed!", "Interesting", "Thanks for sharing",
                "I disagree", "Great!", "Awesome", "Congrats",
                "I really like this", "What do you think about...", "This is useful",
                "\"On the other hand, we denounce with righteous indignation and dislike men who are so beguiled and demoralized by the charms of pleasure of the moment, so blinded by desire, that they cannot foresee the pain and trouble that are bound to ensue; and equal blame belongs to those who fail in their duty through weakness of will, which is the same as saying through shrinking from toil and pain. These cases are perfectly simple and easy to distinguish. In a free hour, when our power of choice is untrammelled and when nothing prevents our being able to do what we like best, every pleasure is to be welcomed and every pain avoided. But in certain circumstances and owing to the claims of duty or the obligations of business it will frequently occur that pleasures have to be repudiated and annoyances accepted. The wise man therefore always holds in these matters to this principle of selection: he rejects pleasures to secure other greater pleasures, or else he endures pains to avoid worse pains.\""
            };
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
                        Text = commentTexts[_random.Next(commentTexts.Length)],
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
                    var messageTexts = new[]
                   {
                        "Hey, how are you?",
                        "What's up?",
                        "See you later",
                        "Thanks!",
                        "Let's meet soon",
                        "I agree",
                        "No problem",
                        "Great talking to you",
                        "Take care",
                        "See you tomorrow",
                        "\"On the other hand, we denounce with righteous indignation and dislike men who are so beguiled and demoralized by the charms of pleasure of the moment, so blinded by desire, that they cannot foresee the pain and trouble that are bound to ensue; and equal blame belongs to those who fail in their duty through weakness of will, which is the same as saying through shrinking from toil and pain. These cases are perfectly simple and easy to distinguish. In a free hour, when our power of choice is untrammelled and when nothing prevents our being able to do what we like best, every pleasure is to be welcomed and every pain avoided. But in certain circumstances and owing to the claims of duty or the obligations of business it will frequently occur that pleasures have to be repudiated and annoyances accepted. The wise man therefore always holds in these matters to this principle of selection: he rejects pleasures to secure other greater pleasures, or else he endures pains to avoid worse pains.\""
                    };
                    var message = new Message
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        ChatId = chat.Id,
                        SenderId = sender.Id,
                        Text = messageTexts[_random.Next(messageTexts.Length)],
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

        [HttpPost("clear")]
        public async Task<IActionResult> ClearAll()
        {
            await _usersCollection.DeleteManyAsync(FilterDefinition<User>.Empty);
            await _postsCollection.DeleteManyAsync(FilterDefinition<Post>.Empty);
            await _notificationsCollection.DeleteManyAsync(FilterDefinition<Notification>.Empty);
            await _conversationsCollection.DeleteManyAsync(FilterDefinition<Chat>.Empty);
            await _messagesCollection.DeleteManyAsync(FilterDefinition<Message>.Empty);

            return Ok(new { message = "All collections cleared successfully." });
        }
    }
}