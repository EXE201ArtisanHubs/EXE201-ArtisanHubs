using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.Forums.Interfaces
{
    public interface IForumThreadRepository : IGenericRepository<ForumThread>
    {
        Task<ForumThread?> GetThreadWithDetailsAsync(int threadId);
        Task<IEnumerable<ForumThread>> GetThreadsByTopicAsync(int topicId);
        Task<bool> HasThreadsInTopicAsync(int topicId);
    }
}
