using System;
using System.Buffers.Binary;

namespace HFSExtract {
    public partial class HFSSerpent {
        private const int ROUND_ITER = 16;
        private const int BLOCK_ITER = 4;

        private byte[] Key { get; } = new byte[128];
        private uint[] SW { get; } = new uint[16];
        private uint[] KS { get; } = new uint[16];
        private uint R1 { get; set; }
        private uint R2 { get; set; }
        private int KeyIndex { get; set; }
        private int Round { get; set; }

        public void SetKey(byte[] key) {
            key[..128].CopyTo((Span<byte>) Key);
            Initialize();
        }

        private void Initialize() {
            SW[15] = HFSUtils.OverflowToInt(Key, 0);
            SW[14] = HFSUtils.OverflowToInt(Key, 4);
            SW[13] = HFSUtils.OverflowToInt(Key, 8);
            SW[12] = HFSUtils.OverflowToInt(Key, 12);
            SW[11] = ~SW[15];
            SW[10] = ~SW[14];
            SW[9] = ~SW[13];
            SW[8] = ~SW[12];
            SW[7] = SW[15];
            SW[6] = SW[14];
            SW[5] = SW[13];
            SW[4] = SW[12];
            SW[3] = ~SW[15];
            SW[2] = ~SW[14];
            SW[1] = ~SW[13];
            SW[0] = ~SW[12];

            R1 = 0;
            R2 = 0;

            for (var i = 0; i < 2; i++) {
                for (var j = 0; j < 16; j++) {
                    var w1 = (R1 + SW[(j + 15) % 16]) ^ R2;
                    SW[j] = Tables.MUL(SW[j]) ^ SW[(j + 2) % 16] ^ Tables.DIV(SW[(j + 11) % 16]) ^ w1;
                    var w2 = R2 + SW[(j + 5) % 16];
                    R2 = Tables.S1_T0[R1 & 0xff] ^ Tables.S1_T1[(R1 >> 8) & 0xff] ^ Tables.S1_T2[(R1 >> 16) & 0xff] ^ Tables.S1_T3[(R1 >> 24) & 0xff];
                    R1 = w2;
                }
            }

            KeyIndex = 0;
            Round = 16;
        }

        private void UpdateStreamKeys() {
            for (var j = 0; j < 16; j++) {
                SW[j] = Tables.MUL(SW[j]) ^ SW[(j + 2) % 16] ^ Tables.DIV(SW[(j + 11) % 16]);
                var w2 = R2 + SW[(j + 5) % 16];
                R2 = Tables.S1_T0[R1 & 0xff] ^ Tables.S1_T1[(R1 >> 8) & 0xff] ^ Tables.S1_T2[(R1 >> 16) & 0xff] ^ Tables.S1_T3[(R1 >> 24) & 0xff];
                R1 = w2;
                KS[j] = (R1 + SW[j]) ^ R2 ^ SW[(j + 1) % 16];
            }
        }

        public void Decrypt(Span<byte> buffer) {
            for (var i = 0; i < buffer.Length / BLOCK_ITER; i++) {
                if (++Round >= ROUND_ITER) {
                    UpdateStreamKeys();
                    Round = 0;
                }

                var offset = i * BLOCK_ITER;

                var value = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
                value -= KS[KeyIndex++ % 16];
                BinaryPrimitives.WriteUInt32LittleEndian(buffer[offset..], value);
            }
        }
    }
}
