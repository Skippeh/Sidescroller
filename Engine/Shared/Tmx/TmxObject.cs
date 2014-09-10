namespace Engine.Shared.Tmx
{
    public class TmxObject
    {
        public string Name { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public TmxProperties Properties { get; internal set; }
        public string Type { get; internal set; }
        public bool Visible { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }

        internal TmxObject()
        {
            
        }

        public override string ToString()
        {
            return "TmxObject: " + Name;
        }
    }
}