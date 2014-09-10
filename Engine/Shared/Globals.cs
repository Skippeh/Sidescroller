namespace Engine.Shared
{
    public static class Globals
    {
        public static bool Server;
        public static bool ListenServer;

        /// <summary>Returns true if this is a client or listen server.</summary>
        public static bool Client { get { return !Server || (Server && ListenServer); } }
    }
}