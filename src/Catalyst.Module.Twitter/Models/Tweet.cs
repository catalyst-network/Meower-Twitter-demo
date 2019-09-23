using System;
using System.Collections.Generic;
using Catalyst.Module.TwitterLikes.Models;

namespace Catalyst.Module.Twitter.Models
{
    public class Tweet
    {
        public string TweetHash { set; get; }
        public string PublicKey { set; get; }
        public string Message { set; get; }
        public DateTime Date { set; get; }
        public List<TweetLike> Likes { set; get; }

        public Tweet()
        {
            Likes = new List<TweetLike>();
        }
    }
}