using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Engine.Server.Network.Messages;
using Engine.Shared.Network.Messages;
using Lidgren.Network;
using Microsoft.Xna.Framework.Net;

namespace Engine.Shared.Network
{
    public class NetworkPeer
    {
        protected NetPeer netPeer;
        private readonly Dictionary<Intent, Action<NetConnection, NetworkMessage>> messageHandlers;
        private readonly Dictionary<Intent, Type> messageTypes;

        public NetworkPeer(bool isServer, NetPeerConfiguration configuration)
        {
            netPeer = isServer ? (NetPeer)new NetServer(configuration) : (NetPeer)new NetClient(configuration);
            messageHandlers = new Dictionary<Intent, Action<NetConnection, NetworkMessage>>();
            messageTypes = new Dictionary<Intent, Type>();
        }

        public void AddIncomingMessageHandler<T>(Intent intent, Action<NetConnection, T> handler) where T : NetworkMessage
        {
            if (typeof (T) == typeof (HailResponse) && netPeer is NetClient) // Invalid use. The HailResponse is received with the ConnectionStatusChanged message on client connect.
            {
                throw new InvalidOperationException("Invalid use. The HailResponse is received with the ConnectionStatusChanged message on client connect.");
            }

            if (messageHandlers.ContainsKey(intent))
                throw new InvalidOperationException("There's already a registered action for this intent. (" + intent + ")");

            messageHandlers.Add(intent, (connection, message) => handler(connection, (T)message));
            messageTypes.Add(intent, typeof(T));
        }

        public void Poll()
        {
            NetIncomingMessage message;
            while ((message = netPeer.ReadMessage()) != null)
            {
                try
                {
                    Intent intent;

                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.ConnectionApproval:
                            intent = Intent.Hail;
                            break;
                        case NetIncomingMessageType.Data:
                            intent = (Intent)message.ReadByte();

                            if (intent == Intent.Hail || intent == Intent.StatusChanged)
                            {
                                if (netPeer is NetServer)
                                {
                                    message.SenderConnection.Disconnect("InvalidData");
                                    break; // Stop reading messages.
                                }
                                else
                                {
                                    Console.Error.WriteLine("[CLIENT] Received invalid intent from server?? (" + intent + ")");
                                }

                                continue;
                            }

                            break;
                        case NetIncomingMessageType.StatusChanged:
                            intent = Intent.StatusChanged;
                            break;
                        default:
                            Console.WriteLine("Unhandled message: " + message.MessageType);
                            continue;
                    }

                    if (messageHandlers.ContainsKey(intent))
                    {
                        if (message.MessageType == NetIncomingMessageType.ConnectionApproval)
                        {
                            var bytes = message.SenderConnection.RemoteHailMessage.ReadBytes(message.SenderConnection.RemoteHailMessage.LengthBytes);

                            if (messageTypes.ContainsKey(intent))
                            {
                                var typeofIntent = messageTypes[intent];
                                var networkMessage = CreateIntentInstance(typeofIntent, bytes);
                                messageHandlers[intent](message.SenderConnection, networkMessage);
                            }
                            else
                            {
                                throw new NotImplementedException("ConnectionApproval not handled!");
                            }
                        }
                        else if (message.MessageType == NetIncomingMessageType.StatusChanged)
                        {
                            var status = (NetConnectionStatus)message.ReadByte();
                            var arg = new ConnectionStatusChanged(status, message.ReadString());
                            if (status == NetConnectionStatus.Connected && netPeer is NetClient)
                            {
                                arg.HailResponse = new HailResponse();
                                arg.HailResponse.Deserialize(message.SenderConnection.RemoteHailMessage.ReadBytes(message.SenderConnection.RemoteHailMessage.LengthBytes));
                            }

                            messageHandlers[intent](message.SenderConnection, arg);
                        }
                        else
                        {
                            var bytes = message.ReadBytes(message.LengthBytes - message.PositionInBytes);
                            var typeofIntent = messageTypes[intent];
                            var msg = CreateIntentInstance(typeofIntent, bytes);
                            messageHandlers[intent](message.SenderConnection, msg);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException("Received message with unhandled intent \"" + intent + "\".");
                    }
                }
                catch (Exception exception)
                {
                    HandleInvalidMessage(message, exception);
                }
            }
        }

        private static NetworkMessage CreateIntentInstance(Type typeofIntent, byte[] bytes)
        {
            return (NetworkMessage)Activator.CreateInstance(typeofIntent, bytes);
        }

        private void HandleInvalidMessage(NetIncomingMessage message, Exception exception)
        {
            if (netPeer is NetServer)
            {
                message.SenderConnection.Disconnect("InvalidData");
            }
            else
            {
                string fileName = (DateTime.Now.ToString("yyyy-MMM-dd_HH-mm-ss", CultureInfo.InvariantCulture) +
                                   " (" + exception.GetType().Name + "= " + (exception.InnerException != null ? exception.InnerException.Message : exception.Message) +
                                   ").bin").RemoveInvalidPathChars();
                message.Position = 0;
                var bytes = new byte[message.LengthBytes];
                message.ReadBytes(bytes, 0, message.LengthBytes);

                if (!Directory.Exists("logs/crashes"))
                    Directory.CreateDirectory("logs/crashes");

                using (var filestream = new FileStream("logs/crashes/" + fileName, FileMode.Create, FileAccess.Write))
                {
                    filestream.Write(bytes, 0, bytes.Length);
                }

                Console.Error.WriteLine("The server sent a message the client couldn't handle, binary data has been saved to \"logs/crashes/" + fileName + "\".");
                throw new NetworkException("The server sent a message the client couldn't handle.");
            }
        }
        
        /// <summary>Sends the message to the recipient.</summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="recipient">The connection to send the data to.</param>
        /// <param name="intent">Signifies what this message contains.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="netDeliveryMethod">See Lidgren documentation.</param>
        /// <param name="sequenceChannel">See Lidgren documentation.</param>
        public void SendMessage<T>(NetConnection recipient, Intent intent, T data, NetDeliveryMethod netDeliveryMethod, int sequenceChannel) where T : NetworkMessage
        {
            if (netPeer.Status != NetPeerStatus.Running)
                throw new InvalidOperationException("Tried to send a message when the NetPeer wasn't running. (" + typeof(T).FullName + ")");

            if (netPeer is NetClient && ((NetClient)netPeer).ConnectionStatus != NetConnectionStatus.Connected)
                throw new InvalidOperationException("Tried to send a message without being connected to a server. (" + typeof(T).FullName + ")");

            var message = CreateSerializeMessage(intent, data);
            netPeer.SendMessage(message, recipient, netDeliveryMethod, sequenceChannel);
        }

        protected NetOutgoingMessage CreateSerializeMessage<T>(Intent intent, T data) where T : NetworkMessage
        {
            var message = netPeer.CreateMessage();
            message.Write((byte)intent);
            message.Write(data.Serialize());
            return message;
        }

        /// <summary>Sends the message to the recipient on sequence channel 0.</summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="recipient">The connection to send the data to.</param>
        /// <param name="intent">Signifies what this message contains.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="netDeliveryMethod">See Lidgren documentation.</param>
        public void SendMessage<T>(NetConnection recipient, Intent intent, T data, NetDeliveryMethod netDeliveryMethod) where T : NetworkMessage
        {
            SendMessage<T>(recipient, intent, data, netDeliveryMethod, 0);
        }

        /// <summary>Sends the message to the recipient reliably and ordered, on sequence channel 0.</summary>
        //// <typeparam name="T">The type of the data.</typeparam>
        /// <param name="recipient">The connection to send the data to.</param>
        /// <param name="intent">Signifies what this message contains.</param>
        /// <param name="data">The data to send.</param>
        public void SendMessage<T>(NetConnection recipient, Intent intent, T data) where T : NetworkMessage
        {
            SendMessage<T>(recipient, intent, data, NetDeliveryMethod.ReliableOrdered);
        }
    }
}