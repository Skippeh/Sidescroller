using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Server.GameCode;
using Engine.Shared;
using Engine.Shared.Tmx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Client.Graphics
{
    public class WorldRenderer : DrawableGameComponent
    {
        public World World { get; private set; }
        public TmxMap Map { get { return World.Map; } }
        public Color ClearColor;

        private readonly TileWorldRenderer tileRenderer;

        public WorldRenderer(Game game, World world) : base(game)
        {
            World = world;
            tileRenderer = new TileWorldRenderer(this, 512);
        }

        public override void Initialize()
        {
            if (Map.Properties.ContainsKey("ClearColor"))
            {
                string strClearColor = Map.Properties.GetValue<string>("ClearColor");
                string[] strRgb = strClearColor.Replace(" ", "").Split(',');
                byte r = byte.Parse(strRgb[0]);
                byte g = byte.Parse(strRgb[1]);
                byte b = byte.Parse(strRgb[2]);
                ClearColor = new Color(r, g, b);
            }

            tileRenderer.Render();
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = Utility.SpriteBatch;

            foreach (var chunk in tileRenderer.RenderedChunks)
            {
                spriteBatch.Draw(chunk.Texture, chunk.DrawPosition * World.Scale, null, Color.White, 0f, Vector2.Zero, World.Scale, SpriteEffects.None, 0);
            }
        }
    }
}