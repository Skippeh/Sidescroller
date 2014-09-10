using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Server.Network.Messages;
using Lidgren.Network;
using ProtoBuf;

namespace Engine.Shared.Network.Messages
{
    [ProtoContract]
    public class ConnectionStatusChanged : NetworkMessage
    {
        [ProtoMember(1)] public NetConnectionStatus NewStatus;
        [ProtoMember(2)] public string Reason;

        public HailResponse HailResponse; // This is only set when a client has connected to a server.

        public ConnectionStatusChanged() {} // Used by ProtoBuf.
        public ConnectionStatusChanged(byte[] bytes) : base(bytes) {} // Used by networking.
        public ConnectionStatusChanged(NetConnectionStatus newStatus, string reason)
        {
            NewStatus = newStatus;
            Reason = reason;
        }

        public override void Deserialize(byte[] bytes)
        {
            var data = NetworkMessage.Deserialize<ConnectionStatusChanged>(bytes);
            NewStatus = data.NewStatus;
            Reason = data.Reason;
        }
    }
}