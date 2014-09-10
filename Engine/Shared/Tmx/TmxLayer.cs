namespace Engine.Shared.Tmx
{
    public class TmxLayer
    {
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int XOffset { get; internal set; }
        public int YOffset { get; internal set; }
        public float Opacity { get; internal set; }

        public string Name { get; internal set; }
        public TmxLayerType Type { get; internal set; }
        public bool Visible { get; internal set; }
        public TmxProperties Properties { get; internal set; }

        /// <summary>This is only valid if the layer's type is tile.</summary>
        public uint[] Data { get; internal set; }

        /// <summary>This is only valid if the layer's type is object.</summary>
        public TmxObjectCollection Objects { get; internal set; }

        internal TmxLayer()
        {
            Objects = new TmxObjectCollection();
            Data = new uint[0];
        }

        public override string ToString()
        {
            return "TmxLayer: " + Name;
        }
    }
}