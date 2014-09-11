using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Engine.Shared
{
    public static class Utility
    {
        private static string _contentDirectory;
        /// <summary>Gets the base directory for all content.</summary>
        public static string ContentDirectory
        {
            get { return _contentDirectory; }
            set
            {
                if (!value.EndsWith("/"))
                    value += "/";

                _contentDirectory = value;
            }
        }

        public static GraphicsDevice GraphicsDevice { get { return gDevice; } }
        public static SpriteBatch SpriteBatch { get; private set; }

        private static GraphicsDevice gDevice;
        private static GraphicsDeviceManager graphics;
        private static ContentManager content;

        private static readonly Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Effect> loadedEffects = new Dictionary<string, Effect>();

        private static readonly Random random = new Random();

        public static void Initialize(GraphicsDeviceManager graphics, ContentManager contentManager, string contentDirectory)
        {
            Utility.graphics = graphics;
            gDevice = graphics.GraphicsDevice;
            ContentDirectory = contentDirectory;
            SpriteBatch = new SpriteBatch(gDevice);
            content = contentManager;
        }

        public static string JsonRemoveComments(string json)
        {
            string[] lines = json.Replace("\r", "").Split('\n');
            var builder = new StringBuilder();

            foreach (string _line in lines)
            {
                string line = _line;

                if (line.Contains("//"))
                {
                    var substring = line.Substring(line.LastIndexOf("//"));

                    if (line.Trim().StartsWith("//")) // If line starts with '//'
                        line = "";
                    else if (!substring.Contains("\"")) // If the last '//' isn't part of a string value
                        line = line.Substring(0, line.LastIndexOf("//"));
                }

                builder.AppendLine(line);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Tries to get the type based on full name but without the assembly text. Not sure how slow this is, iterates through all assemblies in CurrentDomain and calls assembly.GetType(string).
        /// </summary>
        /// <param name="fullTypeName"></param>
        /// <returns></returns>
        public static Type GetType(string fullTypeName)
        {
            var type = Type.GetType(fullTypeName);

            if (type != null)
                return type;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(fullTypeName);

                if (type != null)
                    return type;
            }

            return null;
        }

        public static string RemoveInvalidPathChars(this string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            var invalidChars = Path.GetInvalidFileNameChars();

            return invalidChars.Aggregate(str, (current, ch) => current.Replace(ch.ToString(), ""));
        }

        public static Vector2 GetCenter(Vector2 area, Vector2 size)
        {
            return (area / 2f) - (size / 2f);
        }

        public static Vector2 GetWindowCenter(Vector2? size = null)
        {
            var displayMode = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            return GetCenter(new Vector2(displayMode.X, displayMode.Y), size ?? Vector2.Zero);
        }

        public static Vector2 GetResolution()
        {
            return new Vector2(gDevice.DisplayMode.Width, gDevice.DisplayMode.Height);
        }

        public static DisplayMode[] GetDisplayModes()
        {
            return gDevice.Adapter.SupportedDisplayModes.ToArray();
        }

        /// <param name="filePath">If the path starts with '/' or '\' they will be ignored.</param>
        public static string GetContentDir(string filePath)
        {
            if (filePath.StartsWith("/") || filePath.StartsWith("\\"))
                filePath = filePath.Substring(1);

            return Path.Combine(ContentDirectory, filePath);
        }

        /// <param name="fromContent">If true then the path will be assumed to be relative to the content directory.</param>
        public static Texture2D LoadTexture2D(string path, bool fromContent = true)
        {
            path = fromContent ? GetContentDir(path) : path;

            if (!File.Exists(path))
                return null;

            if (loadedTextures.ContainsKey(path))
                return loadedTextures[path];

            using (var filestream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                Texture2D texture = Texture2D.FromStream(gDevice, filestream);
                loadedTextures.Add(path, texture);
                return texture;
            }
        }

        public static Effect LoadEffect(string contentPath)
        {
            contentPath = GetContentDir("shaders/" + contentPath + ".fx.2MGFX");

            if (!File.Exists(contentPath))
                return null;

            if (loadedEffects.ContainsKey(contentPath))
                return loadedEffects[contentPath];

            using (var filestream = new FileStream(contentPath, FileMode.Open, FileAccess.Read))
            {
                var bytes = new byte[filestream.Length];
                filestream.Read(bytes, 0, bytes.Length);

                var effect = new Effect(gDevice, bytes);
                loadedEffects.Add(contentPath, effect);
                return effect;
            }
        }

        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");

            if (hex.Length != 6 && hex.Length != 8)
                throw new ArgumentException("Hex format is not valid. Needs to be 6 or 8 in length, excluding optional '#' character.");

            byte r, g, b, a;
            byte pos = 0;

            if (hex.Length == 6)
            {
                a = 255;
            }
            else
            {
                a = Convert.ToByte(hex.Substring(pos, 2), 16);
                pos += 2;
            }

            r = Convert.ToByte(hex.Substring(pos, 2), 16);
            pos += 2;

            g = Convert.ToByte(hex.Substring(pos, 2), 16);
            pos += 2;

            b = Convert.ToByte(hex.Substring(pos, 2), 16);

            return new Color(r, g, b, a);
        }

        /// <param name="min">If null then black.</param>
        /// <param name="max">If null then white.</param>
        /// <param name="alpha">If null then the alpha will be randomized aswell.</param>
        public static Color GetRandomColor(Color? min = null, Color? max = null, byte? alpha = 255)
        {
            Color _min = min ?? Color.Black;
            Color _max = max ?? Color.White;
            
            int r, g, b, a;
            a = alpha ?? 0;

            r = random.Next(_min.R, _max.R + 1);
            g = random.Next(_min.G, _max.G + 1);
            b = random.Next(_min.B, _max.B + 1);

            if (alpha == null)
                a = random.Next(_min.A, _max.A + 1);

            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Gets the color inbetween min and max at "distance". Black -> White would become gray.
        /// </summary>
        /// <param name="distance">0 = min, 1 = max</param>
        public static Color GetColorGradient(Color min, Color max, float distance)
        {
            return new Color(Vector4.Lerp(min.ToVector4(), max.ToVector4(), distance));
        }
    }
}