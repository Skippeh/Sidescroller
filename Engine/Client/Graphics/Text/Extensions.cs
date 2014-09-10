using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Client.Graphics.Text
{
    public static class Extensions
    {
        public static void DrawString(this SpriteBatch spriteBatch, BMFont font, string text, Vector2 position) { DrawString(spriteBatch, font, text, position, Color.White); }
        public static void DrawString(this SpriteBatch spriteBatch, BMFont font, string text, Vector2 position, Color color) { DrawString(spriteBatch, font, text, position, color, 0, 1); }
        public static void DrawString(this SpriteBatch spriteBatch, BMFont font, string text, Vector2 position, Color color, float rotation) { DrawString(spriteBatch, font, text, position, color, rotation, 1); }
        public static void DrawString(this SpriteBatch spriteBatch, BMFont font, string text, Vector2 position, Color color, float rotation, float size)
        {
            font.Draw(spriteBatch, text, position, color, rotation, size);
        }
    }
}