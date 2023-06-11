using Screeps.Network.API;

namespace Screeps.Deploy;

class Config
{
    public string Protocol;
    public string Host;
    public ServerType ServerType;

    public string TokenPath;

    public DeployConfig[] Deploys;
}