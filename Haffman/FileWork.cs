using System;
using System.IO;

namespace Huffman
{
    class ArchWriter
    {
        const byte bufSize = 10;
        byte[] buf;
        byte oneByte;
        byte bitsCount;
        byte bytesCount;
        FileStream fs;
        public ArchWriter(string fileName)
        {
            buf = new byte[bufSize];
            oneByte = 0;
            bitsCount = bytesCount = 0;
            fs = new FileStream(fileName, FileMode.Create);
        }
        private void WriteByte()
        {
            buf[bytesCount++] = oneByte;
            oneByte = 0;
            bitsCount = 0;
            if (bytesCount == bufSize)
            {
                fs.Write(buf, 0, bufSize);
                bytesCount = 0;
            }
        }
        public void WriteBit(byte bit)
        {
            oneByte = (byte)((oneByte << 1) + bit);
            bitsCount++;
            if (bitsCount == 8)
                WriteByte();
        }
        public void WriteWord(string w)
        {
            foreach (var c in w)
                if (c == '0')
                    WriteBit(0);
                else if (c == '1')
                    WriteBit(1);
        }
        public void Finish()
        {
            while (bitsCount > 0) WriteBit(0);
            if (bytesCount > 0)
                fs.Write(buf, 0, bytesCount);
            fs.Close();
        }
    }
    class ArchReader
    {
        const byte bufSize = 10;
        byte[] buf;
        byte[] oneByte; //actually bits[]
        byte bitsCount;
        byte bytesCount, byteIdx;
        FileStream fs;
        public ArchReader(string fileName)
        {
            buf = new byte[bufSize];
            oneByte = new byte[8];
            bitsCount = bytesCount = byteIdx = 0;
            fs = new FileStream(fileName, FileMode.Open);
        }
        private bool ReadByte()
        {
            if (bytesCount == byteIdx)
            {
                bytesCount = (byte)fs.Read(buf, 0, bufSize);
                byteIdx = 0;
            }
            if (bytesCount == 0)
                return false;
            var onebyte = buf[byteIdx++];
            for (int i = 0; i < 8; i++)
            {
                oneByte[i] = (byte)(onebyte & 1);
                onebyte = (byte)(onebyte >> 1);
            }
            bitsCount = 8;
            return true;
        }
        public bool ReadBit(out byte bit)
        {
            bool res = true;
            if (bitsCount == 0)
                res = ReadByte();
            if (res)
                bit = oneByte[--bitsCount];
            else
                bit = 0;
            return res;
        }
        public void Finish()
        {
            fs.Close();
        }
    }
}
