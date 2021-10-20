using ScreepsNetworkAPI.API;
using Newtonsoft.Json;

namespace ScreepsDeploy
{
    class DeployConfig
    {
        public string Protocol;
        public string Host;
        public ServerType ServerType;

        [JsonRequired] public string Branch;
        [JsonRequired] public string[] Modules;
    }
}