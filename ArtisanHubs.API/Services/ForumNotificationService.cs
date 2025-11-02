using ArtisanHubs.API.Hubs;
using ArtisanHubs.Bussiness.Services.Forums.Interfaces;
using ArtisanHubs.DTOs.DTO.Reponse.Forums;
using Microsoft.AspNetCore.SignalR;

namespace ArtisanHubs.API.Services
{
    /// <summary>
    /// Service ?? g?i real-time notifications cho Forum
    /// </summary>
    public class ForumNotificationService : IForumNotificationService
    {
        private readonly IHubContext<ForumHub> _hubContext;

        public ForumNotificationService(IHubContext<ForumHub> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// Broadcast thread m?i ??n t?t c? users trong topic
        /// </summary>
        public async Task NotifyNewThread(int topicId, ForumThreadResponse thread)
        {
            var groupName = $"Topic_{topicId}";
            await _hubContext.Clients.Group(groupName).SendAsync("NewThreadCreated", new
            {
                TopicId = topicId,
                Thread = thread,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Broadcast thread ?ã ???c update
        /// </summary>
        public async Task NotifyThreadUpdated(int topicId, ForumThreadResponse thread)
        {
            var groupName = $"Topic_{topicId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ThreadUpdated", new
            {
                TopicId = topicId,
                Thread = thread,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Broadcast thread ?ã ???c xóa
        /// </summary>
        public async Task NotifyThreadDeleted(int topicId, int threadId)
        {
            var groupName = $"Topic_{topicId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ThreadDeleted", new
            {
                TopicId = topicId,
                ThreadId = threadId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Broadcast comment m?i ??n t?t c? users trong thread
        /// </summary>
        public async Task NotifyNewPost(int threadId, ForumPostResponse post)
        {
            var groupName = $"Thread_{threadId}";
            await _hubContext.Clients.Group(groupName).SendAsync("NewPostCreated", new
            {
                ThreadId = threadId,
                Post = post,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Broadcast comment ?ã ???c update
        /// </summary>
        public async Task NotifyPostUpdated(int threadId, ForumPostResponse post)
        {
            var groupName = $"Thread_{threadId}";
            await _hubContext.Clients.Group(groupName).SendAsync("PostUpdated", new
            {
                ThreadId = threadId,
                Post = post,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Broadcast comment ?ã ???c xóa
        /// </summary>
        public async Task NotifyPostDeleted(int threadId, int postId)
        {
            var groupName = $"Thread_{threadId}";
            await _hubContext.Clients.Group(groupName).SendAsync("PostDeleted", new
            {
                ThreadId = threadId,
                PostId = postId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// G?i thông báo ??n m?t user c? th? (ví d?: mention, reply)
        /// </summary>
        public async Task NotifyUser(int accountId, string message, object data)
        {
            await _hubContext.Clients.User(accountId.ToString()).SendAsync("Notification", new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
