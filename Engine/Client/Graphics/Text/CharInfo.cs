namespace Engine.Client.Graphics.Text
{
    public struct CharInfo
    {
        public const int SizeOf = 4 + 2 + 2 + 2 + 2 + 2 + 2 + 2 + 1 + 1;

        public uint Id;
        public ushort X;
        public ushort Y;
        public ushort Width;
        public ushort Height;
        public short XOffset;
        public short YOffset;
        public short XAdvance;
        public byte Page;
        public byte Channel;
    }
}