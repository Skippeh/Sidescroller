using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Engine.Shared.Network;
using Lidgren.Network;

namespace Engine.Server.Network
{
    public class NetworkServer : NetworkPeer
    {
        public NetServer netServer { get { return (NetServer)netPeer; } }

        public NetworkServer(int port) : base(true, new NetPeerConfiguration("Sc") {Port = port})
        {
            netServer.Configuration.SetMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval, true);
        }

        public void Start()
        {
            netServer.Start();

            while (netServer.Status != NetPeerStatus.Running) // Wait until server has started.
                Thread.Sleep(1);
        }

        public void Stop()
        {
            netServer.Shutdown("ServerShutdown");
        }

        public void SendMessageToAll<T>(Intent intent, T data, NetConnection except, NetDeliveryMethod deliveryMethod, int sequenceChannel) where T : NetworkMessage
        {
            if (netPeer.Status != NetPeerStatus.Running)
                throw new InvalidOperationException("Tried to send a message when the NetServer wasn't running. (" + typeof(T).FullName + ")");

            var message = CreateSerializeMessage(intent, data);
            netServer.SendToAll(message, except, deliveryMethod, sequenceChannel);
        }

        public void SendMessageToAll<T>(Intent intent, T data, NetConnection except, NetDeliveryMethod deliveryMethod) where T : NetworkMessage
        {
            SendMessageToAll<T>(intent, data, except, deliveryMethod, 0);
        }

        public void SendMessageToAll<T>(Intent intent, T data, NetConnection except) where T : NetworkMessage
        {
            SendMessageToAll(intent, data, except, NetDeliveryMethod.ReliableOrdered);
        }
    }
}