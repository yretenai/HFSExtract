using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace HFSExtract {
    public sealed class HFSArchive : IDisposable {
        private const int BLOCK_SIZE = 1024;
        
        private readonly Stream BaseStream;

        private int HeaderOffset { get; }
        private int TableOffset { get; }
        private int DataOffset { get; }
        public Dictionary<string, (HFSFile File, byte[] Hash)> Files { get; } = new();
        private HFSHeader Header { get; }
        private string FileName { get; }

        public HFSArchive(Stream stream, string fileName) {
            BaseStream = stream;
            FileName = fileName;

            HeaderOffset = HFSUtils.CalculateHeaderOffset(fileName);
            if (!Header.IsValid) {
                throw new InvalidDataException();
            }
            TableOffset = HFSUtils.CalculateEntryTableOffset(fileName) + HeaderOffset + 9;

            var serpent = new HFSSerpent();
            serpent.SetKey(HFSUtils.GenerateKey(FileName));
            stream.Seek(HeaderOffset, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[12];
            stream.Read(buffer);
            serpent.Decrypt(buffer);
            Header = MemoryMarshal.Read<HFSHeader>(buffer);

            Files.EnsureCapacity(Header.Count);
            stream.Seek(TableOffset, SeekOrigin.Begin);
            buffer = new byte[296 * Header.Count];
            stream.Read(buffer);
            serpent.SetKey(HFSUtils.GenerateEncodingKey(FileName));
            serpent.Decrypt(buffer);

            var marshal = new CursoredMemoryMarshal(buffer.ToArray());
            for (var i = 0; i < Header.Count; ++i) {
                var resourceNameLength = marshal.Read<int>();
                var resourceName = Encoding.Unicode.GetString(marshal.Copy(resourceNameLength * 2).Span);
                var file = marshal.Read<HFSFile>();
                var hash = marshal.Copy(16).ToArray();
            
                Debug.Assert(file.Flags.HasFlag(HFSFileFlags.Encrypted), "file.Flags.HasFlag(HFSFileFlags.Encrypted)");
                Debug.Assert(file.Flags.HasFlag(HFSFileFlags.BlockEncrypted), "file.Flags.HasFlag(HFSFileFlags.BlockEncrypted)");
                
                Files[resourceName] = (file, hash);
            }

            DataOffset = TableOffset + marshal.Cursor;
            if (DataOffset % BLOCK_SIZE > 0) {
                DataOffset += BLOCK_SIZE - DataOffset % BLOCK_SIZE;
            }
        }

        public ReadOnlyMemory<byte> ReadFile(string path) {
            if (!Files.TryGetValue(path, out var pair) || pair.File.FileSize == 0) {
                return ReadOnlyMemory<byte>.Empty;
            }

            var file = pair.File;
            var hash = pair.Hash;
            BaseStream.Seek(DataOffset + file.StartBlock * BLOCK_SIZE, SeekOrigin.Begin);
            Span<byte> buffer = new byte[file.BufferSize];
            BaseStream.Read(buffer);

            if (file.Flags.HasFlag(HFSFileFlags.Encrypted)) {
                var cipher = new HFSSerpent();
                cipher.SetKey(HFSUtils.GenerateHashedKey(path, hash));
                cipher.Decrypt(buffer);
            }

            if (file.Flags.HasFlag(HFSFileFlags.BlockEncrypted)) {
                var cipher = new HFSSerpent();
                cipher.SetKey(HFSUtils.GenerateHashedKey(path, hash));
                cipher.Decrypt(buffer[..Math.Min(buffer.Length, 1024)]);
            }

            Memory<byte> data = new byte[file.FileSize];
            
            if (file.Flags.HasFlag(HFSFileFlags.Compressed)) {
                unsafe {
                    fixed (byte* pinned = &buffer.GetPinnableReference()) {
                        using var unmanagedStream = new UnmanagedMemoryStream(pinned + 2, buffer.Length - 2, buffer.Length - 2, FileAccess.Read);
                        using var zlib = new DeflateStream(unmanagedStream, CompressionMode.Decompress, false);
                        var left = file.FileSize;
                        do {
                            left -= zlib.Read(data.Span[(data.Length - left)..]);
                        } while (left > 0);
                    }
                }
            } else {
                buffer[..file.FileSize].CopyTo(data.Span);
            }

            return data;
        }

        public void Dispose() {
            BaseStream.Dispose();
        }
    }
}
