using System.Diagnostics.Contracts;
using System.IO;

namespace HFSExtract
{
    public class HFSDirectory
    {
        public short Version { get; }
        public short ExtractVersion { get; }
        public short BitFlag { get; }
        public HFSCompressionMethod CompressionMethod { get; }
        public short ModFileTime { get; }
        public short ModFileDate { get; }
        public int Checksum { get; }
        public int CompressedSize { get; }
        public int DecompressedSize { get; }
        public short PartitionNumber { get; }
        public short InternalAttributes { get; }
        public int Attributes { get; }
        public int DataOffset { get; }
        public string Filename { get; }
        public string Extra { get; }
        public string Comment { get; }
        public HFSFile File { get; }

        public HFSDirectory(BinaryReader reader)
        {
            if (reader.ReadInt32() != HFSFile.HFHeader)
            {
                throw new InvalidDataException("Can't parse HFS Directory");
            }

            Version = reader.ReadInt16();
            ExtractVersion = reader.ReadInt16();
            BitFlag = reader.ReadInt16();
            CompressionMethod = (HFSCompressionMethod) reader.ReadInt16();
            Contract.Assert(CompressionMethod == HFSCompressionMethod.Store, "CompressionMethod == Store");
            ModFileTime = reader.ReadInt16();
            ModFileDate = reader.ReadInt16();
            Checksum = reader.ReadInt32();
            CompressedSize = reader.ReadInt32();
            DecompressedSize = reader.ReadInt32();
            var filenameLength = reader.ReadInt16();
            var extraLength = reader.ReadInt16();
            var commentLength = reader.ReadInt16();
            PartitionNumber = reader.ReadInt16();
            InternalAttributes = reader.ReadInt16();
            Attributes = reader.ReadInt32();
            DataOffset = reader.ReadInt32();
            var position = (int)reader.BaseStream.Position;
            Filename = HFS.XorStringWithKey(reader.ReadBytes(filenameLength), HFSXorTruths.KeyTable, position);
            position = (int)reader.BaseStream.Position;
            Extra = HFS.XorStringWithKey(reader.ReadBytes(extraLength), HFSXorTruths.KeyTable, position);
            position = (int)reader.BaseStream.Position;
            Comment = HFS.XorStringWithKey(reader.ReadBytes(commentLength), HFSXorTruths.KeyTable, position);
            position = (int)reader.BaseStream.Position;
            reader.BaseStream.Position = DataOffset;
            File = new HFSFile(reader);
            reader.BaseStream.Position = position;
        }
    }
}