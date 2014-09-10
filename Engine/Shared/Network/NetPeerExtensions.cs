using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Server.Network.Messages;
using Lidgren.Network;

namespace Engine.Shared.Network
{
    public static class NetPeerExtensions
    {
        public static void Approve(this NetConnection connection, HailResponse response)
        {
            var message = connection.Peer.CreateMessage();
            message.Write(response.Serialize());
            connection.Approve(message);
        }
    }
}