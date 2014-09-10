using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Engine.Client.Network;
using Engine.Client.Network.Messages;
using Engine.Server.Network;
using Engine.Server.Network.Messages;
using Engine.Shared;
using Engine.Shared.Network;
using Engine.Shared.Network.Messages;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Sidescroller.Client;

namespace Sidescroller
{
    class Program
    {
        static void Main(string[] args)
        {
            Globals.Server = true;
            Globals.ListenServer = true;
            const int serverPort = 51010;

            if (Globals.Server && !Globals.ListenServer) // Launch dedicated server
            {
                throw new NotImplementedException("Dedicated server mode not supported yet.");
            }
            //else if (Globals.Server && Globals.ListenServer) // Launch listen server
            //{
            //    using (var game = new SSGame(true))
            //    {
            //        game.Run();
            //    }
            //}
            else // Launch client
            {
                using (var game = new SSGame())
                {
                    game.Run();
                }
            }

            //var server = new NetworkServer(serverPort);
            //
            //server.AddIncomingMessageHandler<HailRequest>(Intent.Hail, (connection, hailRequest) =>
            //                                                           {
            //                                                               Console.WriteLine("[SERVER] " + hailRequest.Name + " approved to join.");
            //                                                               connection.Approve(new HailResponse(true, hailRequest.Name));
            //                                                           });
            //server.AddIncomingMessageHandler<ConnectionStatusChanged>(Intent.StatusChanged, (connection, message) =>
            //                                                            {
            //                                                                Console.WriteLine("[SERVER] New status for client: " + message.NewStatus + " (" + message.Reason + ")");
            //                                                            });
            //
            //server.AddIncomingMessageHandler<TestMessage>(Intent.TestMessage, (connection, message) =>
            //                                                                  {
            //                                                                      TestMessageHandler(connection, message);
            //                                                                      server.SendMessage(connection, Intent.TestMessage, new TestMessage(10, false, 'f', "char is f", new Vector2(20.50235f, 30.4568f), 5030, "a", "bcd"));
            //                                                                  });
            //
            //server.Start();
            //
            //new Thread(() =>
            //{
            //    while (server.netServer.Status == NetPeerStatus.Running)
            //    {
            //        server.Poll();
            //        Thread.Sleep(33);
            //    }
            //}).Start();
            //
            //var client = new NetworkClient();
            //
            //client.AddIncomingMessageHandler<ConnectionStatusChanged>(Intent.StatusChanged, (connection, message) =>
            //                                                            {
            //                                                                Console.WriteLine("[CLIENT] New status: " + message.NewStatus);
            //                                                                if (message.NewStatus == NetConnectionStatus.Connected)
            //                                                                {
            //                                                                    Console.WriteLine("[CLIENT] HailResponse received: CanJoin = " + message.HailResponse.CanJoin);
            //                                                                    if (message.HailResponse.CanJoin)
            //                                                                        Console.WriteLine("\t[CLIENT] My name is " + message.HailResponse.Name + ".");
            //
            //                                                                    client.SendMessage(Intent.TestMessage, new TestMessage(100, true, 'G', "The char is G", Vector2.One, long.MaxValue, "hello", "world"));
            //                                                                }
            //                                                            });
            //client.AddIncomingMessageHandler<TestMessage>(Intent.TestMessage, TestMessageHandler);
            //
            //client.Connect("localhost", 51010, new HailRequest("Skippy"));
            //
            //new Thread(() =>
            //{
            //    while (client.netClient.Status == NetPeerStatus.Running)
            //    {
            //        client.Poll();
            //        Thread.Sleep(1);
            //    }
            //}).Start();
            //
            //while (true)
            //{
            //    Thread.Sleep(100);
            //}
        }

        //private static void TestMessageHandler(NetConnection connection, TestMessage message)
        //{
        //    string prefix = connection.Peer is NetServer ? "[SERVER] " : "[CLIENT] ";
        //    Console.WriteLine(prefix + "Received test message: " + message);
        //}
    }
}