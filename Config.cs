using Newtonsoft.Json;

namespace ScreepsDeploy
{
    class Config
    {
        [JsonRequired] public string Token;
        [JsonRequired] public DeployConfig[] Deploys;
    }
}