namespace Engine.Client.Graphics.Text
{
    public struct KerningPair
    {
        public const int SizeOf = 4 + 4 + 2;

        public uint First;
        public uint Second;
        public short Amount;
    }
}