using Microsoft.Xna.Framework;

namespace Engine.Shared.Tmx
{
    public class TmxTilesetTile
    {
        public TmxTileset Tileset;
        public Rectangle SubRectangle;

        public override string ToString()
        {
            return "TmxTilesetTile: " + SubRectangle + "(" + Tileset + ")";
        }
    }
}