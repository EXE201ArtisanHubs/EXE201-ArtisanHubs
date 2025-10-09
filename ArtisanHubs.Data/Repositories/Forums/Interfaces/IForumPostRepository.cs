using ArtisanHubs.Data.Basic;
using ArtisanHubs.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtisanHubs.Data.Repositories.Forums.Interfaces
{
    public interface IForumPostRepository : IGenericRepository<ForumPost>
    {
        Task<bool> CheckIfThreadExistsAsync(int threadId);
        Task<ForumPost?> GetPostWithAuthorAsync(int postId);

    }
}
