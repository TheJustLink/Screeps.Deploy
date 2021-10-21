using Newtonsoft.Json;

using Screeps.Network.API;

namespace Screeps.Deploy
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