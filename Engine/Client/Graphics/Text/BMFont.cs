using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Client.Graphics.Text.Parsers;
using Engine.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Engine.Client.Graphics.Text
{
    public class BMFont
    {
        public FontInfo Info { get; private set; }

        private readonly Texture2D[] pageTextures;
        private readonly bool isDistanceField;

        private static SpriteBatch spriteBatch;
        private static GraphicsDevice gDevice;
        private static Effect distanceFontEffect;

        private static void CheckArgs(GraphicsDevice gameGraphicsDevice, string directory)
        {
            if (gameGraphicsDevice == null)
                throw new ArgumentNullException("gameGraphicsDevice");

            if (directory == null)
                throw new ArgumentNullException("directory");

            if (!Directory.Exists(directory))
                throw new ArgumentException("Directory does not exist.", "directory");
        }

        public BMFont(GraphicsDevice graphicsDevice, string directory, bool isDistanceField)
        {
            if (spriteBatch == null)
            {
                spriteBatch = new SpriteBatch(graphicsDevice);
                gDevice = graphicsDevice;
                distanceFontEffect = Utility.LoadEffect("distancefont");
            }

            CheckArgs(graphicsDevice, directory);

            var directoryInfo = new DirectoryInfo(directory);
            var reader = new BinaryReader(File.OpenRead(directory + "/" + directoryInfo.Name + ".fnt"));

            Info = new FontInfo(reader, new BinaryParser());
            this.isDistanceField = isDistanceField;

            pageTextures = new Texture2D[Info.Pages];

            for (int i = 0; i < pageTextures.Length; ++i)
            {
                string path = directory + "/" + Info.PagePaths[i];

                if (!File.Exists(path))
                    throw new FileNotFoundException("Could not find font page file \"" + path + "\".");

                if (Path.GetExtension(path) == "a")
                    pageTextures[i] = loadAlphaMap(path);
                else
                {
                    using (var fstream = new FileStream(path, FileMode.Open))
                    {
                        pageTextures[i] = Texture2D.FromStream(gDevice, fstream);
                    }
                }
            }
        }

        // Todo: Optimize drawing text.
        public void Draw(SpriteBatch _spriteBatch, string text, Vector2 position, Color color, float rotation, float size = 1)
        {
            if (isDistanceField)
            {
                distanceFontEffect.Parameters["scale"].SetValue(size);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, null, null, distanceFontEffect);
            }

            float x = position.X;
            float y = position.Y;

            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];

                if (ch == '\n' || ch == '\r')
                {
                    x = 0;
                    y += (Info.LineHeight + Info.SpacingV) * size;
                    continue;
                }
                else if (ch == '\t')
                {
                    CharInfo spaceInfo = Info.CharInfos[' '];
                    x += (spaceInfo.XAdvance + Info.SpacingH) * size * 4;
                    continue;
                }

                if (!Info.CharInfos.ContainsKey(ch))
                {
                    ch = Info.CharInfos.ContainsKey('?') ? '?' : ' ';
                }

                CharInfo chInfo = Info.CharInfos[ch];
                var chKern = new KerningPair();
                if (i < text.Length - 1 && Info.HasKerning(text[i], text[i + 1]))
                {
                    chKern = Info.GetKerning(text[i], text[i + 1]);
                }

                var source = new Rectangle(chInfo.X, chInfo.Y, chInfo.Width, chInfo.Height);
                var texture = pageTextures[chInfo.Page];

                var drawPosition = new Vector2(x + (chInfo.XOffset * size), y + (chInfo.YOffset * size));

                if (isDistanceField)
                    spriteBatch.Draw(texture, drawPosition, source, color, 0f, Vector2.Zero, size, SpriteEffects.None, 0);
                else
                    _spriteBatch.Draw(texture, drawPosition, source, color, 0f, Vector2.Zero, size, SpriteEffects.None, 0);

                x += (chInfo.XAdvance + chKern.Amount + Info.SpacingH) * size;
            }

            if (isDistanceField)
                spriteBatch.End();
        }

        // Todo: Optimize measuring text.
        public Vector2 MeasureString(string text)
        {
            int width = 0;
            int height = Info.Base;

            for (int i = 0; i < text.Length; ++i)
            {
                char ch = text[i];

                if (ch == '\n' || ch == '\r')
                {
                    height += Info.LineHeight + Info.SpacingV;
                    continue;
                }
                else if (ch == '\t')
                {
                    CharInfo spaceInfo = Info.CharInfos[' '];
                    width += (spaceInfo.XAdvance + Info.SpacingH) * 4;
                    continue;
                }

                if (!Info.CharInfos.ContainsKey(ch))
                {
                    ch = Info.CharInfos.ContainsKey('?') ? '?' : ' ';
                }

                CharInfo chInfo = Info.CharInfos[ch];
                var chKern = new KerningPair();
                if (i < text.Length - 1 && Info.HasKerning(text[i], text[i + 1]))
                {
                    chKern = Info.GetKerning(text[i], text[i + 1]);
                }

                width += chInfo.XAdvance + chKern.Amount + Info.SpacingH;
            }

            return new Vector2(width, height);
        }

        private Texture2D loadAlphaMap(string path)
        {
            if (!File.Exists(path))
                return null;

            var reader = new BinaryReader(new FileStream(path, FileMode.Open));
            byte[] header = reader.ReadBytes(4);
            var expectedHeader = new byte[4];
            expectedHeader[0] = (byte)'A';
            expectedHeader[1] = (byte)'M';
            expectedHeader[2] = (byte)'A';
            expectedHeader[3] = (byte)'P';
            if (!header.SequenceEqual(expectedHeader))
                throw new Exception("Specified file is not a valid alpha map.");

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            var texture = new Texture2D(gDevice, width, height);

            var colors = new Color[width * height];
            for (int i = 0; i < width * height; ++i)
            {
                var a = reader.ReadByte();
                colors[i] = new Color(a, a, a, a);
            }

            texture.SetData(colors);
            return texture;
        }
    }
}