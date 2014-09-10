using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Shared.Tmx
{
    public class TmxTileset
    {
        public string Name { get; internal set; }
        public uint FirstGid { get; internal set; }
        public Texture2D Texture { get; internal set; }
        public string ImageSource { get; internal set; }
        public int ImageWidth { get; internal set; }
        public int ImageHeight { get; internal set; }
        public int Margin { get; internal set; }
        public int Spacing { get; internal set; }
        public int TileWidth { get; internal set; }
        public int TileHeight { get; internal set; }
        public TmxProperties Properties { get; internal set; }

        /// <summary>The key is the TileGid.</summary>
        public Dictionary<uint, TmxTilesetTile> Tiles { get; internal set; } 

        internal TmxTileset()
        {
        }

        internal void InitializeTiles()
        {
            Tiles = new Dictionary<uint, TmxTilesetTile>();

            uint gid = FirstGid;
            for (int y = 0; y < ImageHeight; y += TileHeight + Spacing + Margin)
            {
                for (int x = 0; x < ImageWidth; x += TileWidth + Spacing + Margin)
                {
                    var tile = new TmxTilesetTile();
                    Tiles.Add(gid, tile);

                    tile.SubRectangle = new Rectangle(x, y, TileWidth, TileHeight);
                    tile.Tileset = this;

                    ++gid;
                }
            }
        }

        public override string ToString()
        {
            return "TmxTileset: " + Name;
        }
    }
}