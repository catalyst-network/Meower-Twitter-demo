using Catalyst.Module.Twitter.Interfaces;

namespace Catalyst.Module.Twitter.Config
{
    public class TwitterModuleConfig : ITwitterModuleConfig
    {
        public string TwitterLikesApiUrl { set; get; }
    }
}