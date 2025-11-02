using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ArtisanHubs.API.Hubs
{
    /// <summary>
    /// SignalR Hub ?? x? lý real-time updates cho Forum
    /// </summary>
    [Authorize]
    public class ForumHub : Hub
    {
        /// <summary>
        /// G?i khi client k?t n?i
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var accountId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            
            Console.WriteLine($"User {username} (ID: {accountId}) connected with ConnectionId: {Context.ConnectionId}");
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// G?i khi client ng?t k?t n?i
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            Console.WriteLine($"User {username} disconnected: {Context.ConnectionId}");
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join vào m?t topic c? th? ?? nh?n updates
        /// </summary>
        /// <param name="topicId">ID c?a forum topic</param>
        public async Task JoinTopic(int topicId)
        {
            var groupName = $"Topic_{topicId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            Console.WriteLine($"User {username} joined topic {topicId}");
            
            // Thông báo cho ng??i dùng khác trong group
            await Clients.OthersInGroup(groupName).SendAsync("UserJoinedTopic", new
            {
                Username = username,
                TopicId = topicId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Leave kh?i m?t topic
        /// </summary>
        /// <param name="topicId">ID c?a forum topic</param>
        public async Task LeaveTopic(int topicId)
        {
            var groupName = $"Topic_{topicId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            Console.WriteLine($"User {username} left topic {topicId}");
            
            // Thông báo cho ng??i dùng khác trong group
            await Clients.OthersInGroup(groupName).SendAsync("UserLeftTopic", new
            {
                Username = username,
                TopicId = topicId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Join vào m?t thread c? th? ?? nh?n updates v? comments
        /// </summary>
        /// <param name="threadId">ID c?a forum thread</param>
        public async Task JoinThread(int threadId)
        {
            var groupName = $"Thread_{threadId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            Console.WriteLine($"User {username} joined thread {threadId}");
        }

        /// <summary>
        /// Leave kh?i m?t thread
        /// </summary>
        /// <param name="threadId">ID c?a forum thread</param>
        public async Task LeaveThread(int threadId)
        {
            var groupName = $"Thread_{threadId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            Console.WriteLine($"User {username} left thread {threadId}");
        }

        /// <summary>
        /// G?i thông báo khi user ?ang typing comment
        /// </summary>
        /// <param name="threadId">ID c?a thread</param>
        public async Task UserTyping(int threadId)
        {
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            var groupName = $"Thread_{threadId}";
            
            await Clients.OthersInGroup(groupName).SendAsync("UserIsTyping", new
            {
                Username = username,
                ThreadId = threadId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// G?i thông báo khi user ng?ng typing
        /// </summary>
        /// <param name="threadId">ID c?a thread</param>
        public async Task UserStoppedTyping(int threadId)
        {
            var username = Context.User?.Identity?.Name ?? "Anonymous";
            var groupName = $"Thread_{threadId}";
            
            await Clients.OthersInGroup(groupName).SendAsync("UserStoppedTyping", new
            {
                Username = username,
                ThreadId = threadId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
