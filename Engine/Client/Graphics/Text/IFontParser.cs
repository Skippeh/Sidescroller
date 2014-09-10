using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Engine.Client.Graphics.Text
{
    public interface IFontParser
    {
        void LoadData(BinaryReader reader, FontInfo info);
        void LoadInfo();
        void LoadCommon();
        void LoadPages();
        void LoadChars();
        void LoadKerningPairs();
    }
}