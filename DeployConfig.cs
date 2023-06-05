using System.Collections.Generic;

using Screeps.Network.API;

namespace Screeps.Deploy
{
    class DeployConfig
    {
        public string Protocol;
        public string Host;
        public ServerType ServerType;

        public string Branch;
        public Dictionary<string, string> Modules;
    }
}