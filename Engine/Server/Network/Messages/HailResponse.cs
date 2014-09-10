using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Shared.Network;
using ProtoBuf;

namespace Engine.Server.Network.Messages
{
    [ProtoContract]
    public class HailResponse : NetworkMessage
    {
        [ProtoMember(1)] public bool CanJoin;
        [ProtoMember(2)] public string Name;

        public HailResponse() {} // Used by ProtoBuf.
        public HailResponse(byte[] bytes) : base(bytes) {} // Used by networking.
        public HailResponse(bool canJoin, string name)
        {
            CanJoin = canJoin;
            Name = name;
        }

        public override void Deserialize(byte[] bytes)
        {
            var data = NetworkMessage.Deserialize<HailResponse>(bytes);
            CanJoin = data.CanJoin;
            Name = data.Name;
        }
    }
}