using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine.Client.Graphics.Text.Parsers
{
    public class BinaryParser : IFontParser
    {
        public const int Version = 3;

        private BinaryReader reader;
        private FontInfo info;

        public void LoadData(BinaryReader reader, FontInfo info)
        {
            this.reader = reader;
            this.info = info;

            VerifyHeader();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                switch (reader.ReadByte())
                {
                    case 1:
                        LoadInfo(); break;
                    case 2:
                        LoadCommon(); break;
                    case 3:
                        LoadPages(); break;
                    case 4:
                        LoadChars(); break;
                    case 5:
                        LoadKerningPairs(); break;
                }
            }
        }

        public void VerifyHeader()
        {
            if (reader.BaseStream.Length < 3)
                throw new Exception("Could not load BMFont, file is either corrupt or not a valid font file.");

            var chars = new char[] { 'B', 'M', 'F' };
            var bytes = reader.ReadBytes(3);

            for (int i = 0; i < chars.Length; ++i)
            {
                if (bytes[i] != chars[i])
                {
                    throw new Exception("Could not load BMFont, file is either corrupt or not a valid font file.");
                }
            }

            byte version = reader.ReadByte();

            if (version != Version)
                throw new Exception("Could not load BMFont, expected version " + Version + ", got version " + version + ".");
        }

        public void LoadInfo()
        {
            int blockSize = reader.ReadInt32();

            info.Size = reader.ReadInt16();
            var bitreader = new BitArray(new byte[] { reader.ReadByte() });
            info.Smooth = bitreader[7];
            info.Unicode = bitreader[6];
            info.Italic = bitreader[5];
            info.Bold = bitreader[4];
            info.FixedHeight = bitreader[3];

            info.CharSet = reader.ReadByte();
            info.StretchH = reader.ReadInt16();
            info.AA = reader.ReadByte();
            info.PaddingUp = reader.ReadByte();
            info.PaddingRight = reader.ReadByte();
            info.PaddingDown = reader.ReadByte();
            info.PaddingLeft = reader.ReadByte();
            info.SpacingH = reader.ReadByte();
            info.SpacingV = reader.ReadByte();
            info.Outline = reader.ReadBoolean();
            info.FontName = readString(reader);
        }

        public void LoadCommon()
        {
            int blockSize = reader.ReadInt32();
            info.LineHeight = reader.ReadInt16();
            info.Base = reader.ReadInt16();
            info.ScaleW = reader.ReadInt16();
            info.ScaleH = reader.ReadInt16();
            info.Pages = reader.ReadInt16();
            var bitreader = new BitArray(new byte[] { reader.ReadByte() });
            info.Packed = bitreader[0];
            info.AlphaChannel = reader.ReadByte();
            info.RedChannel = reader.ReadByte();
            info.GreenChannel = reader.ReadByte();
            info.BlueChannel = reader.ReadByte();
        }

        public void LoadPages()
        {
            int blockSize = reader.ReadInt32();
            int read = 0;
            var pages = new List<string>();
            var firstPage = readString(reader);
            pages.Add(firstPage);
            read += firstPage.Length + 1;

            while (read < blockSize)
            {
                pages.Add(readString(reader));
                read += firstPage.Length + 1;
            }

            info.PagePaths = pages.ToArray();
        }

        public void LoadChars()
        {
            var charInfos = new Dictionary<uint, CharInfo>();
            int blockSize = reader.ReadInt32();
            int read = 0;

            while (read < blockSize)
            {
                var charInfo = new CharInfo();

                charInfo.Id = reader.ReadUInt32();
                charInfo.X = reader.ReadUInt16();
                charInfo.Y = reader.ReadUInt16();
                charInfo.Width = reader.ReadUInt16();
                charInfo.Height = reader.ReadUInt16();
                charInfo.XOffset = reader.ReadInt16();
                charInfo.YOffset = reader.ReadInt16();
                charInfo.XAdvance = reader.ReadInt16();
                charInfo.Page = reader.ReadByte();
                charInfo.Channel = reader.ReadByte();

                charInfos.Add(charInfo.Id, charInfo);
                read += CharInfo.SizeOf;
            }

            info.CharInfos = charInfos;
        }

        public void LoadKerningPairs()
        {
            int blockSize = reader.ReadInt32();
            var kerningPairInfos = new Dictionary<Vector2ui, KerningPair>();
            int read = 0;

            while (read < blockSize)
            {
                var kerningPair = new KerningPair();

                kerningPair.First = reader.ReadUInt32();
                kerningPair.Second = reader.ReadUInt32();
                kerningPair.Amount = reader.ReadInt16();

                kerningPairInfos.Add(new Vector2ui(kerningPair.First, kerningPair.Second), kerningPair);
                read += KerningPair.SizeOf;
            }

            info.KerningPairs = kerningPairInfos;
        }

        private string readString(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();

            char currentChar;
            while ((currentChar = reader.ReadChar()) != '\0')
            {
                stringBuilder.Append(currentChar);
            }

            return stringBuilder.ToString();
        }
    }
}