using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Engine.Client.Network.Messages;
using Engine.Shared.Network;
using Lidgren.Network;

namespace Engine.Client.Network
{
    public class NetworkClient : NetworkPeer
    {
        public NetClient netClient { get { return (NetClient)netPeer; } }

        public NetworkClient() : base(false, new NetPeerConfiguration("Sc"))
        {
            
        }

        public void Connect(string ip, int port, HailRequest hail)
        {
            if (netClient.Status == NetPeerStatus.NotRunning)
                netClient.Start();

            while (netClient.Status != NetPeerStatus.Running) // Wait until client has started.
                Thread.Sleep(1);

            var hailMessage = netClient.CreateMessage();
            hailMessage.Write(hail.Serialize());

            netClient.Connect(ip, port, hailMessage);
        }

        /// <summary>Sends a message to the server.</summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="intent">Signifies what this message contains.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="deliveryMethod">See lidgren documentation.</param>
        /// <param name="sequenceChannel">See lidgren documentation.</param>
        public void SendMessage<T>(Intent intent, T data, NetDeliveryMethod deliveryMethod, int sequenceChannel) where T : NetworkMessage
        {
            SendMessage(netClient.ServerConnection, intent, data, deliveryMethod, sequenceChannel);
        }

        /// <summary>Sends a message to the server.</summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="intent">Signifies what this message contains.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="deliveryMethod">See lidgren documentation.</param>
        public void SendMessage<T>(Intent intent, T data, NetDeliveryMethod deliveryMethod) where T : NetworkMessage
        {
            SendMessage(intent, data, deliveryMethod, 0);
        }

        /// <summary>Sends a message to the server.</summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="intent">Signifies what this message contains.</param>
        /// <param name="data">The data to send.</param>
        public void SendMessage<T>(Intent intent, T data) where T : NetworkMessage
        {
            SendMessage(intent, data, NetDeliveryMethod.ReliableOrdered);
        }
    }
}