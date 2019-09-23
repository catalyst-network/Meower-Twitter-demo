using Catalyst.Abstractions.Repository;
using Catalyst.Module.TwitterLikes.Models;

namespace Catalyst.Module.TwitterLikes.Interfaces
{
    public interface ITweetLikeRepository : IRepositoryWrapper<TweetLike>
    {
    }
}