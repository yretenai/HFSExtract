using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HFSExtract
{
    public class HFS
    {
        public HFSIndex Index { get; set; }

        public IEnumerable<HFSDirectory> Entries { get; set; }

        public HFS(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                Initialize(stream);
            }
        }

        public HFS(Stream stream)
        {
            Initialize(stream);
        }

        private void Initialize(Stream stream)
        {
            stream.Position = stream.Length - 0x16;

            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            Index = new HFSIndex(reader);

            stream.Position = Index.DirectoryOffset;
            var directories = new List<HFSDirectory>(Index.DirectoryCount);
            for(var i = 0; i < Index.DirectoryCount; ++i)
            {
                directories.Add(new HFSDirectory(reader));
            }
            Entries = directories;
        }

        public static string XorStringWithKey(byte[] buffer, IEnumerable<byte> key, int offset)
        {
            return new string(XorBlockWithKey(buffer, key, offset).Select(x => (char)x).ToArray());
        }

        public static byte[] XorBlockWithKey(byte[] buffer, IEnumerable<byte> key, int offset)
        {
            if (buffer.Length == 0) return buffer;
            var bytes = new byte[buffer.Length];
            var length = key.Count();
            for (int x = 0; x < buffer.Length; x++)
            {
                bytes[x] = (byte)(buffer[x] ^ key.ElementAt((offset + x) & (length - 1)));
            }
            return bytes;
        }
    }
}
