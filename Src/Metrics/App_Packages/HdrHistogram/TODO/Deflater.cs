// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HdrHistogram
{
    class Deflater
    {
        private int compressionLevel;

        public Deflater(int compressionLevel)
        {
            this.compressionLevel = compressionLevel;
        }

        internal void setInput(object p1, int p2, int uncompressedLength)
        {
            throw new NotImplementedException();
        }

        internal void finish()
        {
            throw new NotImplementedException();
        }

        internal int deflate(byte[] targetArray, int compressedTargetOffset, int p)
        {
            throw new NotImplementedException();
        }

        internal void end()
        {
            throw new NotImplementedException();
        }

        public static int DEFAULT_COMPRESSION { get; set; }
    }
}
