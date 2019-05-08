using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;

namespace HFSExtract
{
    public class HFSFile
    {
        public const int HFHeader = 0x02014648;
        public const int CompHeader = 0x706D6F63;

        public short ExtractVersion { get; }
        public short BitFlag { get; }
        public HFSCompressionMethod CompressionMethod { get; }
        public short ModFileTime { get; }
        public short ModFileDate { get; }
        public int Checksum { get; }
        public int CompressedSize { get; }
        public int DecompressedSize { get; }
        public string Filename { get; }
        public string Extra { get; }
        public byte[] Data { get; }

        public HFSFile(BinaryReader reader)
        {
            if (reader.ReadInt32() != HFHeader)
            {
                throw new InvalidDataException("Can't parse HFS File");
            }

            ExtractVersion = reader.ReadInt16();
            BitFlag = reader.ReadInt16();
            CompressionMethod = (HFSCompressionMethod)reader.ReadInt16();
            Contract.Assert(CompressionMethod == HFSCompressionMethod.Store, "CompressionMethod == Store");
            ModFileTime = reader.ReadInt16();
            ModFileDate = reader.ReadInt16();
            Checksum = reader.ReadInt32();
            CompressedSize = reader.ReadInt32();
            DecompressedSize = reader.ReadInt32();
            var filenameLength = reader.ReadInt16();
            var extraLength = reader.ReadInt16();
            var position = (int)reader.BaseStream.Position;
            Filename = HFS.XorStringWithKey(reader.ReadBytes(filenameLength), HFSXorTruths.KeyTable, position);
            position = (int)reader.BaseStream.Position;
            Extra = HFS.XorStringWithKey(reader.ReadBytes(extraLength), HFSXorTruths.KeyTable, position);
            position = (int)reader.BaseStream.Position;
            Data = HFS.XorBlockWithKey(reader.ReadBytes(CompressedSize), HFSXorTruths.KeyTable, position);
            if(Filename.EndsWith(".comp"))
            {
                using (var ms = new MemoryStream(Data))
                using (var msReader = new BinaryReader(ms))
                {
                    ms.Position = 0;
                    var compressionHeader = msReader.ReadInt32();
                    var compressionSize = msReader.ReadInt32();
                    if (compressionHeader == CompHeader)
                    {
                        ms.Position += 2;
                        DecompressedSize = compressionSize;
                        Filename = Path.GetFileNameWithoutExtension(Filename);
                        using (var zip = new DeflateStream(ms, CompressionMode.Decompress))
                        {
                            Data = new byte[compressionSize];
                            zip.Read(Data, 0, compressionSize);
                        }
                    }
                }
            }
        }
    }
}