using System.Collections.Generic;
using Catalyst.Module.TwitterLikes.Models;

namespace Catalyst.Module.TwitterLikes.Interfaces
{
    public interface ITweetLikeService
    {
        Dictionary<string, List<TweetLike>> GetLikes(List<string> tweetIds);
        bool Like(TweetLikeData tweetLikeData);
        bool Unlike(TweetLikeData tweetLikeData);
    }
}