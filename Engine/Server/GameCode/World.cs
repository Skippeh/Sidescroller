using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Engine.Client.Graphics;
using Engine.Shared.GameCode;
using Engine.Shared.Tmx;
using Microsoft.Xna.Framework;

namespace Engine.Server.GameCode
{
    public class World : GameComponent
    {
        /// <summary>Tile size in pixels</summary>
        public static int TileSize = 16;

        public int Width { get { return Map.Width; } }
        public int Height { get { return Map.Height; } }
        public float Scale = 1f;

        internal TmxMap Map;

        public World(Game game, TmxMap map) : base(game)
        {
            Map = map;
        }

        public override void Initialize()
        {
            if (Map.Properties.ContainsKey("Scale"))
            {
                var strScale = Map.Properties.GetValue<string>("Scale");
                Scale = float.Parse(strScale, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
        }
    }
}