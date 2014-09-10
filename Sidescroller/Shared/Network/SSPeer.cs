using Engine.Shared.Network;
using Lidgren.Network;

namespace Sidescroller.Shared.Network
{
    public abstract class SSPeer : NetworkPeer
    {
        protected SSPeer(bool isServer, NetPeerConfiguration configuration) : base(isServer, configuration)
        {

        }
    }
}