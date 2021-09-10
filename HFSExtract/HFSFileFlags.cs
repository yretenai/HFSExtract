using System;

namespace HFSExtract {
    [Flags]
    public enum HFSFileFlags : uint {
        Compressed = 1,
        Encrypted = 2,
        BlockEncrypted = 4,
    }
}
