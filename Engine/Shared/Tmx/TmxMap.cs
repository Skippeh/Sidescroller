using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Engine.Shared.Tmx
{
    public class TmxMap
    {
        #region Static
        public const int Version = 1;

        /// <summary>Loads the TMX map and textures at the given path.</summary>
        public static TmxMap Load(string path) { return Load(path, true); }

        /// <summary>Loads the TMX map and textures if loadTextures is true at the given path.</summary>
        public static TmxMap Load(string path, bool loadTextures)
        {
            var map = new TmxMap();
            JObject jMap;

            if (!File.Exists(path))
                throw new FileNotFoundException("Could not load TMX map, file not found.", path);

            using (var fs = File.OpenText(path))
            {
                string json = fs.ReadToEnd();
                jMap = JsonConvert.DeserializeObject<JObject>(json);
            }

            int version = jMap["version"].Value<int>();
            if (version != Version)
                throw new NotImplementedException("Implementation for TMX file version \"" + version + "\" is not supported. Supported version is \"" + Version + "\".");

            map.Width = jMap["width"].Value<int>();
            map.Height = jMap["height"].Value<int>();
            map.TileWidth = jMap["tilewidth"].Value<int>();
            map.TileHeight = jMap["tileheight"].Value<int>();
            map.Orientation = parseOrientation(jMap["orientation"].Value<string>());
            map.BackgroundColor = Utility.HexToColor(jMap["backgroundcolor"].Value<string>());

            var jMapProperties = jMap["properties"];
            if (jMapProperties != null)
                map.Properties = ParseTmxProperties(jMapProperties.Value<JObject>());
            else
                map.Properties = new TmxProperties();

            var jTilesetArray = jMap["tilesets"].Value<JArray>();
            map.Tilesets = new TmxTilesetCollection();
            for (int i = 0; i < jTilesetArray.Count; ++i)
            {
                var tileset = new TmxTileset();
                map.Tilesets.Add(tileset);
                var jTileset = jTilesetArray[i].Value<JObject>();

                tileset.FirstGid = jTileset["firstgid"].Value<uint>();
                tileset.Margin = jTileset["margin"].Value<int>();
                tileset.Spacing = jTileset["spacing"].Value<int>();
                tileset.Name = jTileset["name"].Value<string>();
                tileset.TileWidth = jTileset["tilewidth"].Value<int>();
                tileset.TileHeight = jTileset["tileheight"].Value<int>();
                tileset.ImageWidth = jTileset["imagewidth"].Value<int>();
                tileset.ImageHeight = jTileset["imageheight"].Value<int>();
                tileset.ImageSource = jTileset["image"].Value<string>();

                var jProperties = jTileset["properties"];
                if (jProperties != null)
                    tileset.Properties = ParseTmxProperties(jProperties.Value<JObject>());
                else
                    tileset.Properties = new TmxProperties();

                if (loadTextures)
                {
                    var texturePath = Path.Combine(Path.GetDirectoryName(path), jTileset["image"].Value<string>());
                    tileset.Texture = Utility.LoadTexture2D(texturePath, false);
                }

                tileset.InitializeTiles();
            }

            var jLayerArray = jMap["layers"].Value<JArray>();
            map.Layers = new TmxLayerCollection();
            for (int i = 0; i < jLayerArray.Count; ++i)
            {
                var layer = new TmxLayer();
                map.Layers.Add(layer);
                var jLayer = jLayerArray[i].Value<JObject>();

                layer.Width = jLayer["width"].Value<int>();
                layer.Height = jLayer["height"].Value<int>();
                layer.Name = jLayer["name"].Value<string>();
                layer.Opacity = jLayer["opacity"].Value<float>();
                layer.Type = parseLayerType(jLayer["type"].Value<string>());
                layer.Visible = jLayer["visible"].Value<bool>();
                layer.XOffset = jLayer["x"].Value<int>();
                layer.YOffset = jLayer["y"].Value<int>();

                var jProperties = jLayer["properties"];
                if (jProperties != null)
                    layer.Properties = ParseTmxProperties(jProperties.Value<JObject>());
                else
                    layer.Properties = new TmxProperties();

                switch (layer.Type)
                {
                    case TmxLayerType.Tile:
                        var jData = jLayer["data"].Value<JArray>();
                        layer.Data = new uint[jData.Count];

                        for (int j = 0; j < jData.Count; ++j)
                            layer.Data[j] = jData[j].Value<uint>();

                        break;
                    case TmxLayerType.Object:
                        var jObjects = jLayer["objects"].Value<JArray>();
                        layer.Objects = new TmxObjectCollection();

                        for (int j = 0; j < jObjects.Count; ++j)
                        {
                            var jObject = jObjects[j].Value<JObject>();
                            var tmxObject = new TmxObject();
                            layer.Objects.Add(tmxObject);

                            tmxObject.Name = jObject["name"].Value<string>();
                            tmxObject.Width = jObject["width"].Value<int>();
                            tmxObject.Height = jObject["height"].Value<int>();
                            tmxObject.X = jObject["x"].Value<int>();
                            tmxObject.Y = jObject["y"].Value<int>();
                            tmxObject.Type = jObject["type"].Value<string>();
                            tmxObject.Visible = jObject["visible"].Value<bool>();

                            var properties = jObject["properties"];
                            if (properties != null)
                                tmxObject.Properties = ParseTmxProperties(properties.Value<JObject>());
                        }

                        break;
                    case TmxLayerType.Image:
                        throw new NotImplementedException();
                }
            }

            return map;
        }

        internal static TmxProperties ParseTmxProperties(JObject jObject)
        {
            var properties = new TmxProperties();

            foreach (KeyValuePair<string, JToken> keyval in jObject)
            {
                JToken jVal = keyval.Value;
                object value;

                switch (keyval.Value.Type)
                {
                    case JTokenType.Boolean:
                        value = jVal.Value<bool>();
                        break;
                    case JTokenType.Float:
                        value = jVal.Value<float>();
                        break;
                    case JTokenType.Integer:
                        value = jVal.Value<int>();
                        break;
                    default:
                        value = jVal.Value<string>();
                        break;
                }

                properties[keyval.Key] = value;
            }

            return properties;
        }

        private static TmxOrientation parseOrientation(string orientation)
        {
            switch (orientation)
            {
                default:
                    throw new ArgumentException("Specified orientation \"" + orientation + "\" is unknown.");
                case "orthogonal":
                    return TmxOrientation.Orthogonal;
                case "isometric":
                    return TmxOrientation.Isometric;
                case "staggered":
                    return TmxOrientation.IsometricStaggered;
            }
        }

        private static TmxLayerType parseLayerType(string layerType)
        {
            switch (layerType)
            {
                default:
                    throw new ArgumentException("Specified type \"" + layerType + "\" is unknown.");
                case "tilelayer":
                    return TmxLayerType.Tile;
                case "imagelayer":
                    return TmxLayerType.Image;
                case "objectgroup":
                    return TmxLayerType.Object;
            }
        }

        #endregion

        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public TmxOrientation Orientation { get; internal set; }

        public int TileWidth { get; internal set; }
        public int TileHeight { get; internal set; }

        public TmxTilesetCollection Tilesets { get; internal set; }
        public TmxLayerCollection Layers { get; internal set; }

        public Color BackgroundColor { get; internal set; }
        public TmxProperties Properties { get; internal set; }

        internal TmxMap()
        {

        }

        public TmxTilesetTile FindTileInfo(uint tileGid)
        {
            for (int i = 0; i < Tilesets.Count; ++i)
            {
                if (tileGid > Tilesets[i].FirstGid)
                {
                    return Tilesets[i].Tiles[tileGid];
                }
            }

            return null;
        }

        /// <summary>Returns the tile's information at the given tile coordinates in the specified layer.</summary>
        public TmxTilesetTile FindTile(int x, int y, int layer)
        {
            int index = (y * Width) + x;

            if (index > Width * Height || index < 0)
                return null;

            if (layer < 0 || layer > Layers.Count)
                throw new IndexOutOfRangeException("layer out of range: " + layer + "(count = " + Layers.Count + ")");

            return FindTileInfo(Layers[layer].Data[index]);
        }
    }
}