using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catalyst.Abstractions.Cryptography;
using Catalyst.Abstractions.IO.Events;
using Catalyst.Abstractions.KeySigner;
using Catalyst.Abstractions.Mempool.Repositories;
using Catalyst.Abstractions.Types;
using Catalyst.Core.Lib.DAO;
using Catalyst.Core.Lib.Mempool.Documents;
using Catalyst.Core.Modules.Mempool.Repositories;
using Catalyst.Module.Twitter.Helpers;
using Catalyst.Module.Twitter.Interfaces;
using Catalyst.Module.Twitter.Models;
using Catalyst.Module.TwitterLikes.Models;
using Catalyst.Protocol.Cryptography;
using Catalyst.Protocol.Network;
using Catalyst.Protocol.Transaction;
using Catalyst.Protocol.Wire;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using SharpRepository.Repository.Queries;
using SimpleBase;

namespace Catalyst.Module.Twitter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TweetController : Controller
    {
        private readonly SigningContext _signingContext;
        private readonly IPrivateKey _privateKey;
        private readonly IPublicKey _publicKey;
        private readonly IWrapper _cryptoWrapper;
        private readonly byte[] _tweetHeader = { 0x74, 0x77, 0x3a };
        private readonly MempoolDocumentRepository _mempoolRepository;
        private readonly ITransactionReceivedEvent _transactionReceivedEvent;
        private readonly ITwitterModuleConfig _twitterModuleConfig;

        public TweetController(IKeySigner keySigner,
            IWrapper cryptoWrapper,
            IMempoolRepository<TransactionBroadcastDao> mempoolRepository,
            ITransactionReceivedEvent transactionReceivedEvent,
            ITwitterModuleConfig twitterModuleConfig)
        {
            _signingContext = new SigningContext
            {
                NetworkType = NetworkType.Devnet,
                SignatureType = SignatureType.TransactionPublic
            };

            _privateKey = keySigner.KeyStore.KeyStoreDecrypt(KeyRegistryTypes.DefaultKey);
            _publicKey = keySigner.CryptoContext.GetPublicKey(_privateKey);

            _cryptoWrapper = cryptoWrapper;
            _mempoolRepository = (MempoolDocumentRepository) mempoolRepository;
            _transactionReceivedEvent = transactionReceivedEvent;
            _twitterModuleConfig = twitterModuleConfig;
        }

        private IEnumerable<TransactionBroadcastDao> GetPageDesc(int page, int count)
        {
            var pagingOptions =
                new PagingOptions<TransactionBroadcastDao, DateTime>(page, count, x => x.TimeStamp, true);
            return _mempoolRepository.GetAll(x => x, pagingOptions).ToList();
        }

        [HttpGet("Me")]
        public object Me()
        {
            return new { PublicKey = Base58.Bitcoin.Encode(_publicKey.Bytes) };
        }

        [HttpGet]
        public IEnumerable<Tweet> Get(int page = 1, int count = 30)
        {
            var tweets = GetPageDesc(page, count).Select(x => new Tweet
            {
                Date = x.TimeStamp,
                TweetHash = Base58.Bitcoin.Encode(Base32.Crockford.Decode(x.Signature.RawBytes)),
                PublicKey = Base58.Bitcoin.Encode(Base32.Crockford.Decode(x.PublicEntries.First().Base.SenderPublicKey)),
                Message = Encoding.UTF8.GetString(Convert.FromBase64String(x.ContractEntries.First().Data)).Substring(_tweetHeader.Length)
            }).ToList();

            var client = new RestClient(_twitterModuleConfig.TwitterLikesApiUrl);
            var request = new RestRequest("/api/TweetLike/GetLikes");

            var twitterIds = tweets.Select(x => x.TweetHash).ToList();
            request.AddJsonBody(twitterIds);


            var tweetLikesResponse = client.Post<Dictionary<string, List<TweetLike>>>(request);
            if (tweetLikesResponse.Data == null)
            {
                return tweets;
            }

            tweets.ForEach(x =>
            {
                if (tweetLikesResponse.Data.ContainsKey(x.TweetHash))
                {
                    x.Likes.AddRange(tweetLikesResponse.Data[x.TweetHash]);
                }
            });

            return tweets;
        }

        private IActionResult GenerateTweet(IPrivateKey privateKey, IPublicKey publicKey, string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                return BadRequest("Messages must contain text");
            }

            if (message.Length > 240)
            {
                return BadRequest("Messages have a maximum length of 240 characters.");
            }

            var publicKeyByteString = ByteString.CopyFrom(publicKey.Bytes);

            var transaction = new TransactionBroadcast();
            var publicEntry = new PublicEntry();
            publicEntry.Amount = ByteString.CopyFrom(0);

            var contractEntry = new ContractEntry();
            contractEntry.Amount = ByteString.CopyFrom(0);
            contractEntry.Data = ByteString.CopyFromUtf8($"tw:{message}");

            var contractEntryBase = new BaseEntry();
            contractEntryBase.SenderPublicKey = publicKeyByteString;
            contractEntryBase.TransactionFees = ByteString.CopyFrom(0);

            contractEntry.Base = contractEntryBase;
            publicEntry.Base = contractEntryBase;

            transaction.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            transaction.ContractEntries.Add(contractEntry);
            transaction.PublicEntries.Add(publicEntry);

            transaction.Signature = SignatureHelper.GenerateSignature(_cryptoWrapper, privateKey, transaction, _signingContext);

            _transactionReceivedEvent.OnTransactionReceived(transaction);

            return Ok();
        }

        [HttpPost]
        public IActionResult Post(TweetData tweetData)
        {
            return GenerateTweet(_privateKey, _publicKey, tweetData.Message);
        }

        [HttpPost("Like/{tweetId}")]
        public IActionResult Like(string tweetId)
        {
            return LikeOrUnlikeTweet(tweetId, true);
        }

        [HttpPost("Unlike/{tweetId}")]
        public IActionResult Unlike(string tweetId)
        {
            return LikeOrUnlikeTweet(tweetId, false);
        }

        private IActionResult LikeOrUnlikeTweet(string tweetId, bool value)
        {
            var tweetLike = new TweetLikeData
            {
                TweetId = tweetId,
                PublicKey = Base58.Bitcoin.Encode(_publicKey.Bytes),
                PostedDate = DateTime.UtcNow
            };

            var signatureBytes = _cryptoWrapper.StdSign(_privateKey,
                Encoding.UTF8.GetBytes($"{tweetLike.TweetId}{tweetLike.PublicKey}{value}{tweetLike.PostedDate}"),
                _signingContext.ToByteArray()).SignatureBytes;

            tweetLike.Signature = Base58.Bitcoin.Encode(signatureBytes);

            var client = new RestClient(_twitterModuleConfig.TwitterLikesApiUrl);
            var endpoint = value ? "Like" : "Unlike";
            var request = new RestRequest($"/api/TweetLike/{endpoint}");
            request.AddJsonBody(tweetLike);
            var response = client.Post(request);
            if (!response.IsSuccessful)
            {
                return BadRequest(response.ErrorMessage);
            }

            return Ok();
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Catalyst.Abstractions.Cryptography;
//using Catalyst.Abstractions.IO.Events;
//using Catalyst.Abstractions.KeySigner;
//using Catalyst.Abstractions.Mempool.Repositories;
//using Catalyst.Abstractions.Types;
//using Catalyst.Core.Lib.Mempool.Documents;
//using Catalyst.Core.Modules.Mempool.Repositories;
//using Catalyst.Module.Twitter.Helpers;
//using Catalyst.Module.Twitter.Interfaces;
//using Catalyst.Module.Twitter.Models;
//using Catalyst.Module.TwitterLikes.Models;
//using Catalyst.Protocol.Cryptography;
//using Catalyst.Protocol.Network;
//using Catalyst.Protocol.Transaction;
//using Catalyst.Protocol.Wire;
//using Google.Protobuf;
//using Google.Protobuf.WellKnownTypes;
//using Microsoft.AspNetCore.Mvc;
//using RestSharp;
//using SimpleBase;

//namespace Catalyst.Module.Twitter.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class TweetController : Controller
//    {
//        private readonly SigningContext _signingContext;
//        private readonly IPrivateKey _privateKey;
//        private readonly IPublicKey _publicKey;
//        private readonly IWrapper _cryptoWrapper;
//        private readonly byte[] _tweetHeader = {0x74, 0x77, 0x3a};
//        private readonly MempoolDocumentRepository _mempoolRepository;
//        private readonly ITransactionReceivedEvent _transactionReceivedEvent;
//        private readonly ITwitterModuleConfig _twitterModuleConfig;

//        public TweetController(IKeySigner keySigner,
//            IWrapper cryptoWrapper,
//            IMempoolRepository<MempoolDocument> mempoolRepository,
//            ITransactionReceivedEvent transactionReceivedEvent,
//            ITwitterModuleConfig twitterModuleConfig)
//        {
//            _signingContext = new SigningContext
//            {
//                NetworkType = NetworkType.Devnet,
//                SignatureType = SignatureType.TransactionPublic
//            };

//            _privateKey = keySigner.KeyStore.KeyStoreDecrypt(KeyRegistryTypes.DefaultKey);
//            _publicKey = keySigner.CryptoContext.GetPublicKey(_privateKey);

//            _cryptoWrapper = cryptoWrapper;
//            _mempoolRepository = (MempoolDocumentRepository) mempoolRepository;
//            _transactionReceivedEvent = transactionReceivedEvent;
//            _twitterModuleConfig = twitterModuleConfig;
//        }

//        private IEnumerable<TransactionBroadcast> GetPageDesc(int page = 0, int count = 30)
//        {
//            //var pagingOptions =
//            //    new PagingOptions<MempoolDocument, Timestamp>(page, count, x => x.Transaction.TimeStamp, true);
//            //return _mempoolRepository
//            //    .FindAll(x => x.Transaction.Data.Length > 3 && x.Transaction.Data.StartsWith(_tweetHeader),
//            //        pagingOptions).Select(x => x.Transaction).ToList();

//            var allItems = _mempoolRepository.GetAll().OrderByDescending(x => x.Timestamp.ToDateTime()).ToList();
//            return allItems;
//        }

//        [HttpGet("Me")]
//        public object Me()
//        {
//            return new {PublicKey = Base58.Bitcoin.Encode(_publicKey.Bytes)};
//        }

//        [HttpGet]
//        public IEnumerable<Tweet> Get(int page = 0, int count = 30)
//        {
//            var tweets = GetPageDesc(page, count).Select(x => new Tweet
//            {
//                Date = x.Timestamp.ToDateTime(),
//                TweetHash = Base58.Bitcoin.Encode(x.Signature.ToByteArray()),
//                PublicKey = Base58.Bitcoin.Encode(x.PublicEntries.First().Base.SenderPublicKey.ToByteArray()),
//                Message = x.ContractEntries.First().Data.ToStringUtf8().Substring(_tweetHeader.Length)
//            }).ToList();

//            var client = new RestClient(_twitterModuleConfig.TwitterLikesApiUrl);
//            var request = new RestRequest("/api/TweetLike/GetLikes");

//            var twitterIds = tweets.Select(x => x.TweetHash).ToList();
//            request.AddJsonBody(twitterIds);


//            var tweetLikesResponse = client.Post<Dictionary<string, List<TweetLike>>>(request);
//            if (tweetLikesResponse.Data == null)
//            {
//                return tweets;
//            }

//            tweets.ForEach(x =>
//            {
//                if (tweetLikesResponse.Data.ContainsKey(x.TweetHash))
//                {
//                    x.Likes.AddRange(tweetLikesResponse.Data[x.TweetHash]);
//                }
//            });

//            return tweets;
//        }

//        private IActionResult GenerateMeow(IPrivateKey privateKey, IPublicKey publicKey, string message)
//        {
//            if (message.Length > 240)
//            {
//                return BadRequest("Messages have a maximum length of 240 characters.");
//            }

//            var publicKeyByteString = ByteString.CopyFrom(publicKey.Bytes);

//            var transaction = new TransactionBroadcast();
//            var publicEntry = new PublicEntry();
//            publicEntry.Amount = ByteString.CopyFrom(0);

//            var contractEntry = new ContractEntry();
//            contractEntry.Amount = ByteString.CopyFrom(0);
//            contractEntry.Data = ByteString.CopyFromUtf8($"tw:{message}");

//            var contractEntryBase = new BaseEntry();
//            contractEntryBase.SenderPublicKey = publicKeyByteString;
//            contractEntryBase.TransactionFees = ByteString.CopyFrom(0);

//            contractEntry.Base = contractEntryBase;
//            publicEntry.Base = contractEntryBase;

//            transaction.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
//            transaction.ContractEntries.Add(contractEntry);
//            transaction.PublicEntries.Add(publicEntry);

//            transaction.Signature =
//                SignatureHelper.GenerateSignature(_cryptoWrapper, privateKey, transaction, _signingContext);

//            _transactionReceivedEvent.OnTransactionReceived(transaction);

//            return Ok();
//        }

//        [HttpPost]
//        public IActionResult Post(TweetData tweetData)
//        {
//            return GenerateMeow(_privateKey, _publicKey, tweetData.Message);
//        }

//        [HttpPost("RandomMessage")]
//        public IActionResult RandomMessage(TweetData tweetData)
//        {
//            var privateKey = _cryptoWrapper.GeneratePrivateKey();
//            return GenerateMeow(privateKey, privateKey.GetPublicKey(), tweetData.Message);
//        }

//        [HttpPost("Like/{tweetId}")]
//        public IActionResult Like(string tweetId)
//        {
//            return LikeOrUnlikeTweet(tweetId, true);
//        }

//        [HttpPost("Unlike/{tweetId}")]
//        public IActionResult Unlike(string tweetId)
//        {
//            return LikeOrUnlikeTweet(tweetId, false);
//        }

//        private IActionResult LikeOrUnlikeTweet(string tweetId, bool value)
//        {
//            var tweetLike = new TweetLikeData
//            {
//                TweetId = tweetId,
//                PublicKey = Base58.Bitcoin.Encode(_publicKey.Bytes),
//                PostedDate = DateTime.UtcNow
//            };

//            //var signatureBytes = _cryptoWrapper.StdSign(_privateKey,
//            //    Encoding.UTF8.GetBytes($"{tweetLike.TweetId}{tweetLike.PublicKey}{value}"),
//            //    _signingContext.ToByteArray()).SignatureBytes;

//            //tweetLike.Signature = Base58.Bitcoin.Encode(signatureBytes);

//            var client = new RestClient(_twitterModuleConfig.TwitterLikesApiUrl);
//            var endpoint = value ? "Like" : "Unlike";
//            var request = new RestRequest($"/api/TweetLike/{endpoint}");
//            request.AddJsonBody(tweetLike);
//            var response = client.Post(request);
//            if (!response.IsSuccessful)
//            {
//                return BadRequest(response.ErrorMessage);
//            }

//            return Ok();
//        }
//    }
//}