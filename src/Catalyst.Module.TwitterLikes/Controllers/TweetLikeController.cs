using System.Collections.Generic;
using Catalyst.Module.TwitterLikes.Interfaces;
using Catalyst.Module.TwitterLikes.Models;
using Microsoft.AspNetCore.Mvc;

namespace Catalyst.Module.TwitterLikes.Controllers
{
    [ApiController]
    [Route("api/TweetLike")]
    public class TweetLikeController : Controller
    {
        private readonly ITweetLikeService _tweetLikeService;

        public TweetLikeController(ITweetLikeService tweetLikeService)
        {
            _tweetLikeService = tweetLikeService;
        }

        [HttpPost("GetLikes")]
        public Dictionary<string, List<TweetLike>> GetLikes([FromBody] List<string> tweetIds)
        {
            return _tweetLikeService.GetLikes(tweetIds);
        }

        [HttpPost("Like")]
        public IActionResult Like(TweetLikeData tweetLikeData)
        {
            if (_tweetLikeService.Like(tweetLikeData))
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost("Unlike")]
        public IActionResult Unlike(TweetLikeData tweetLikeData)
        {
            if (_tweetLikeService.Unlike(tweetLikeData))
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}