using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine.Client.Graphics.Text
{
    /// <summary>
    /// Contains information about a BMFont binary file.
    /// </summary>
    public class FontInfo
    {
        // Info block
        public short Size { get; internal set; }
        public bool Smooth { get; internal set; }
        public bool Unicode { get; internal set; }
        public bool Italic { get; internal set; }
        public bool Bold { get; internal set; }
        public bool FixedHeight { get; internal set; }
        public byte CharSet { get; internal set; }
        public short StretchH { get; internal set; }
        public byte AA { get; internal set; }
        public byte PaddingUp { get; internal set; }
        public byte PaddingRight { get; internal set; }
        public byte PaddingDown { get; internal set; }
        public byte PaddingLeft { get; internal set; }
        public byte SpacingH { get; internal set; }
        public byte SpacingV { get; internal set; }
        public bool Outline { get; internal set; }
        public string FontName { get; internal set; }

        // Common block
        public short LineHeight { get; internal set; }
        public short Base { get; internal set; }
        public short ScaleW { get; internal set; }
        public short ScaleH { get; internal set; }
        public short Pages { get; internal set; }
        public bool Packed { get; internal set; }
        public byte AlphaChannel { get; internal set; }
        public byte RedChannel { get; internal set; }
        public byte GreenChannel { get; internal set; }
        public byte BlueChannel { get; internal set; }

        // Pages block
        public string[] PagePaths { get; internal set; }

        // Chars block
        public Dictionary<uint, CharInfo> CharInfos { get; internal set; }

        // Kerning pairs block
        public Dictionary<Vector2ui, KerningPair> KerningPairs { get; internal set; }

        /// <summary>
        /// Reads data from a BMFont file.
        /// </summary>
        /// <param name="reader">The binary data reader.</param>
        /// <param name="parser">The parser to use.</param>
        public FontInfo(BinaryReader reader, IFontParser parser)
        {
            parser.LoadData(reader, this);
            reader.Close();
        }

        public bool HasKerning(char c1, char c2)
        {
            return KerningPairs != null && KerningPairs.ContainsKey(new Vector2ui(c1, c2));
        }

        public KerningPair GetKerning(char c1, char c2)
        {
            return KerningPairs[new Vector2ui(c1, c2)];
        }
    }
}