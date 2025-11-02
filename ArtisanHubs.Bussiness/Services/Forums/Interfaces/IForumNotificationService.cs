using ArtisanHubs.DTOs.DTO.Reponse.Forums;

namespace ArtisanHubs.Bussiness.Services.Forums.Interfaces
{
    /// <summary>
    /// Interface cho Forum Notification Service
    /// </summary>
    public interface IForumNotificationService
    {
        Task NotifyNewThread(int topicId, ForumThreadResponse thread);
        Task NotifyThreadUpdated(int topicId, ForumThreadResponse thread);
        Task NotifyThreadDeleted(int topicId, int threadId);
        Task NotifyNewPost(int threadId, ForumPostResponse post);
        Task NotifyPostUpdated(int threadId, ForumPostResponse post);
        Task NotifyPostDeleted(int threadId, int postId);
        Task NotifyUser(int accountId, string message, object data);
    }
}
