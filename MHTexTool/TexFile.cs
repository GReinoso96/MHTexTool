using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHTexTool
{
    class TexFile
    {
        private uint texCount;
        private uint imageOffset;
        private uint imageSize;
        private byte[] imageData;

        public TexFile()
        {
        }

        public uint TexCount { get => texCount; set => texCount = value; }
        public uint ImageOffset { get => imageOffset; set => imageOffset = value; }
        public uint ImageSize { get => imageSize; set => imageSize = value; }
        public byte[] ImageData { get => imageData; set => imageData = value; }
    }
}
