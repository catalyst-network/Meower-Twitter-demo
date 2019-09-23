using Catalyst.Abstractions.Repository;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SharpRepository.Repository;
using System;

namespace Catalyst.Module.TwitterLikes.Models
{
    public class TweetLike : IDocument
    {
        [JsonIgnore]
        [RepositoryPrimaryKey(Order = 1)]
        [JsonProperty("id")]
        [BsonId]
        public string DocumentId { set; get; }

        public string TweetId { set; get; }
        public bool Value { set; get; }
        public string PublicKey { set; get; }
        public string Signature { set; get; }
        public DateTime PostedDate { set; get; }
    }
}