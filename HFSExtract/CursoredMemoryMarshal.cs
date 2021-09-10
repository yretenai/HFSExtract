using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HFSExtract {
    public class CursoredMemoryMarshal {
        private Memory<byte> Buffer { get; }
        public int Cursor { get; private set; }

        public CursoredMemoryMarshal(Memory<byte> buffer, int cursor = 0) {
            Buffer = buffer;
            Cursor = cursor;
        }

        public T Read<T>() where T : struct {
            var value = MemoryMarshal.Read<T>(Buffer[Cursor..].Span);
            Cursor += Unsafe.SizeOf<T>();

            return value;
        }

        public Memory<byte> Copy(int size) {
            var slice = Buffer.Slice(Cursor, size);
            Cursor += size;
            return slice;
        }
    }
}
