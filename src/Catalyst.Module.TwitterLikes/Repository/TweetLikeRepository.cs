using Catalyst.Core.Lib.Repository;
using Catalyst.Module.TwitterLikes.Interfaces;
using Catalyst.Module.TwitterLikes.Models;
using SharpRepository.Repository;

namespace Catalyst.Module.TwitterLikes.Repository
{
    public class TweetLikeRepository : RepositoryWrapper<TweetLike>, ITweetLikeRepository
    {
        public TweetLikeRepository(IRepository<TweetLike, string> repository) : base(repository)
        {
        }
    }
}