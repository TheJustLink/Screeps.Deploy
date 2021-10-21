using Newtonsoft.Json;

namespace Screeps.Deploy
{
    class Config
    {
        [JsonRequired] public string Token;
        [JsonRequired] public DeployConfig[] Deploys;
    }
}