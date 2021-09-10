using System.Runtime.InteropServices;

namespace HFSExtract {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly record struct HFSHeader(int Checksum, byte Version, int Count) {
        public bool IsValid => Checksum == Count + Version;
    }
}
