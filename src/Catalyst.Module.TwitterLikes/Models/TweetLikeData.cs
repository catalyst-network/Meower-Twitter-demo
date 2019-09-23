using System;

namespace Catalyst.Module.TwitterLikes.Models
{
    public class TweetLikeData
    {
        public string TweetId { set; get; }
        public string PublicKey { set; get; }
        public string Signature { set; get; }
        public DateTime PostedDate { set; get; }
    }
}