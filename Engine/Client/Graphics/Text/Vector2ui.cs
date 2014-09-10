namespace Engine.Client.Graphics.Text
{
    public class Vector2ui
    {
        protected bool Equals(Vector2ui other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Vector2ui) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) X * 397) ^ (int) Y;
            }
        }

        public uint X;
        public uint Y;

        public Vector2ui(uint x, uint y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Vector2ui left, Vector2ui right)
        {
            if ((object)left == null)
                return false;
            if ((object)right == null)
                return false;

            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2ui left, Vector2ui right)
        {
            return !(left == right);
        }
    }
}