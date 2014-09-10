using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Shared.Network;
using ProtoBuf;

namespace Engine.Client.Network.Messages
{
    [ProtoContract]
    public class HailRequest : NetworkMessage
    {
        [ProtoMember(1)] public string Name;

        public HailRequest() {} // Used by ProtoBuf.
        public HailRequest(byte[] bytes) : base(bytes) { } // Used by networking.
        public HailRequest(string name)
        {
            Name = name;
        }


        public override void Deserialize(byte[] bytes)
        {
            var data = NetworkMessage.Deserialize<HailRequest>(bytes);
            Name = data.Name;
        }
    }
}