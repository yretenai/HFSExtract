using System.Runtime.InteropServices;

namespace HFSExtract {
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly record struct HFSFile(uint Checksum, HFSFileFlags Flags, int StartBlock, int FileSize, int BufferSize);
}
