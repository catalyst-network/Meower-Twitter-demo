using System;
using System.Collections.Generic;
using System.Linq;
using Catalyst.Module.TwitterLikes.Interfaces;
using Catalyst.Module.TwitterLikes.Models;
using Microsoft.AspNetCore.Mvc;

namespace Catalyst.Module.TwitterLikes.Services
{
    public class TweetLikeService : ITweetLikeService
    {
        private readonly ITweetLikeRepository _tweetLikeRepository;
        public TweetLikeService(ITweetLikeRepository tweetLikeRepository)
        {
            _tweetLikeRepository = tweetLikeRepository;
        }

        public Dictionary<string, List<TweetLike>> GetLikes(List<string> tweetIds)
        {
            var likes = _tweetLikeRepository.FindAll(x => x.Value && tweetIds.Contains(x.TweetId))
                .GroupBy(k => k.TweetId).ToDictionary(x => x.Key, x => x.ToList());
            return likes;
        }

        private TweetLike MapTweetLikeToTweetLikeData(TweetLikeData tweetLikeData)
        {
            var tweetLike = new TweetLike
            {
                TweetId = tweetLikeData.TweetId,
                PublicKey = tweetLikeData.PublicKey,
                Signature = tweetLikeData.Signature,
                PostedDate = tweetLikeData.PostedDate
            };
            return tweetLike;
        }

        private bool UpdateTweetLikeIfExists(TweetLike tweetLike)
        {
            var savedTweetLike =
                _tweetLikeRepository.Find(x => x.PublicKey == tweetLike.PublicKey && x.TweetId == tweetLike.TweetId);
            if (savedTweetLike != null)
            {
                if (tweetLike.PostedDate.ToUniversalTime() > savedTweetLike.PostedDate.ToUniversalTime())
                {
                    savedTweetLike.Value = tweetLike.Value;
                    savedTweetLike.PostedDate = tweetLike.PostedDate;
                    savedTweetLike.Signature = tweetLike.Signature;
                    _tweetLikeRepository.Update(savedTweetLike);
                    return true;
                }
            }

            return false;
        }

        private bool UpdateOrCreateTweetLike(TweetLikeData tweetLikeData, bool value)
        {
            var tweetLike = MapTweetLikeToTweetLikeData(tweetLikeData);
            tweetLike.Value = value;

            if (UpdateTweetLikeIfExists(tweetLike))
            {
                return true;
            }

            tweetLike.DocumentId = Guid.NewGuid().ToString();
            _tweetLikeRepository.Add(tweetLike);
            return true;
        }

        public bool Like(TweetLikeData tweetLikeData)
        {
            return UpdateOrCreateTweetLike(tweetLikeData, true);
        }

        public bool Unlike(TweetLikeData tweetLikeData)
        {
            return UpdateOrCreateTweetLike(tweetLikeData, false);
        }
    }
}
