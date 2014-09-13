using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Server.GameCode;
using Engine.Shared.Tmx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Client.Graphics
{
    internal class TileWorldRenderer
    {
        public class RenderedChunk
        {
            public Texture2D Texture;
            public Vector2 DrawPosition;

            public RenderedChunk(Vector2 drawPosition, Texture2D texture)
            {
                Texture = texture;
                DrawPosition = drawPosition;
            }
        }

        public RenderedChunk[] RenderedChunks { get; private set; }

        private WorldRenderer worldRenderer;
        private int chunkSize;
        private SpriteBatch spriteBatch;

        public TileWorldRenderer(WorldRenderer worldRenderer, uint chunkSize)
        {
            this.worldRenderer = worldRenderer;
            this.chunkSize = (int)chunkSize;
            spriteBatch = new SpriteBatch(worldRenderer.Game.GraphicsDevice);
        }

        ~TileWorldRenderer()
        {
            DisposeChunks();
        }

        public void Render()
        {
            // Dispose old chunks' textures first.
            DisposeChunks();

            var chunkList = new List<RenderedChunk>();

            for (var y = 0; y < worldRenderer.World.Height * World.TileSize; y += chunkSize)
            {
                for (var x = 0; x < worldRenderer.World.Width * World.TileSize; x += chunkSize)
                {
                    var drawPosition = new Vector2(x, y);
                    var texture = RenderChunk(drawPosition);
                    chunkList.Add(new RenderedChunk(drawPosition, texture));
                }
            }

            RenderedChunks = chunkList.ToArray();
        }

        private void DisposeChunks()
        {
            if (RenderedChunks != null)
            {
                foreach (var chunk in RenderedChunks)
                    chunk.Texture.Dispose();
            }
        }

        private Texture2D RenderChunk(Vector2 position)
        {
            GraphicsDevice gDevice = worldRenderer.Game.GraphicsDevice;
            var renderTarget = new RenderTarget2D(gDevice, chunkSize, chunkSize, false, gDevice.PresentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            gDevice.SetRenderTarget(renderTarget);
            gDevice.Clear(Color.Transparent);

            // Render tiles
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null);

            var world = worldRenderer.World;
            var tileWidth = world.Map.TileWidth;
            var tileHeight = world.Map.TileHeight;
            
            foreach (var layer in world.Map.Layers)
            {
                if (!layer.Visible || layer.Opacity == 0f || layer.Type != TmxLayerType.Tile)
                    continue;

                for (int y = getYStart(position.Y, chunkSize); y < getYEnd(position.Y, chunkSize); ++y)
                {
                    for (int x = getXStart(position.X, chunkSize); x < getXEnd(position.X, chunkSize); ++x)
                    {
                        float absX = (x * tileWidth) - position.X; // Negate the position so we render them inside the texture. Remember, we're drawing them to a texture and not the screen.
                        float absY = (y * tileHeight) - position.Y;
                        var tileGid = layer.Data[x + (y * world.Width)];

                        if (tileGid == 0)
                            continue;
            
                        TmxTilesetTile tileInfo = world.Map.FindTileInfo(tileGid);

                        spriteBatch.Draw(tileInfo.Tileset.Texture, new Vector2(absX, absY), tileInfo.SubRectangle, Color.White);
                    }
                }
            }

            spriteBatch.End();

            gDevice.SetRenderTarget(null);
            return renderTarget;
        }

        private int getXStart(float absX, int drawWidth)
        {
            var x = (int)((absX / worldRenderer.World.Map.TileWidth) - ((float)drawWidth / worldRenderer.World.Map.TileWidth));
            return MathHelper.Clamp(x, 0, worldRenderer.World.Width - 1);
        }

        private float getXEnd(float absX, int drawWidth)
        {
            var x = (int)Math.Ceiling((absX / worldRenderer.World.Map.TileWidth) + ((float)drawWidth / worldRenderer.World.Map.TileWidth));
            return MathHelper.Clamp(x, 0, worldRenderer.World.Width - 1);
        }

        private int getYStart(float absY, int drawHeight)
        {
            var y = (int)((absY / worldRenderer.World.Map.TileHeight) - ((float)drawHeight / worldRenderer.World.Map.TileHeight));
            return MathHelper.Clamp(y, 0, worldRenderer.World.Height - 1);
        }

        private int getYEnd(float absY, int drawHeight)
        {
            var y = (int)Math.Ceiling((absY / worldRenderer.World.Map.TileHeight) + ((float)drawHeight / worldRenderer.World.Map.TileHeight));
            return MathHelper.Clamp(y, 0, worldRenderer.World.Height - 1);
        }
    }
}