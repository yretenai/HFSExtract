using System.Diagnostics.Contracts;
using System.IO;

namespace HFSExtract
{
    public class HFSIndex
    {
        public const int HFIndexHeader = 0x06054648;

        public short IndexNumber { get; } // zero

        public short PartitionNumber { get; } // zero

        public short DirectoryCount { get; }

        public short DirectoryPartition { get; } // partitioned?

        public int DirectoryBlockSize { get; }

        public int DirectoryOffset { get; }

        public string Comment { get; }

        public HFSIndex(BinaryReader reader)
        {
            if(reader.ReadInt32() != HFIndexHeader)
            {
                throw new HFSException(HFSError.HFSIndexMismatch);
            }

            IndexNumber = reader.ReadInt16();
            PartitionNumber = reader.ReadInt16();
            Contract.Assert(IndexNumber == PartitionNumber, "IndexNumber == DirectoryIndex");
            DirectoryCount = reader.ReadInt16();
            DirectoryPartition = reader.ReadInt16();
            Contract.Assert(DirectoryCount == DirectoryPartition, "DirectoryCount == DirectoryNumber");
            DirectoryBlockSize = reader.ReadInt32();
            DirectoryOffset = reader.ReadInt32();
            var length = reader.ReadInt16();
            var position = (int)reader.BaseStream.Position;
            Comment = HFS.XorStringWithKey(reader.ReadBytes(length), HFSXorTruths.KeyTable, position);
        }
    }
}