using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Shared.Network.Messages;

namespace Engine.Shared.Network
{
    public abstract class NetworkMessage
    {
        public NetworkMessage(byte[] bytes = null)
        {
            if (bytes != null)
                Deserialize(bytes);
        }

        public byte[] Serialize()
        {
            byte[] bytes;

            using (var memstream = new MemoryStream())
            {
                ProtoBuf.Serialize(this, memstream);

                memstream.Seek(0, SeekOrigin.Begin);
                bytes = new byte[memstream.Length];
                memstream.Read(bytes, 0, bytes.Length);
            }

            return bytes;
        }

        protected static T Deserialize<T>(byte[] bytes)
        {
            using (var memstream = new MemoryStream(bytes))
            {
                return ProtoBuf.Deserialize<T>(memstream);
            }
        }

        public abstract void Deserialize(byte[] bytes);
    }
}