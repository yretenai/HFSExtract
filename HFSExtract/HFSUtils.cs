using System;
using System.Linq;

namespace HFSExtract {
    internal static class HFSUtils {
        public static uint OverflowToInt(byte[] lpData, int index) {
            unchecked {
                var A = (sbyte) lpData[index];
                var B = (sbyte) lpData[index + 1];
                var C = (sbyte) lpData[index + 2];
                var D = (sbyte) lpData[index + 3];

#pragma warning disable 675
                return (uint) (D | ((C | ((B | (A << 8)) << 8)) << 8));
#pragma warning restore 675
            }
        }

        private const string STATIC_KEY = "MBHEROES!@0u9";
        private const int KEY_ITER = 0x3F;
        private const int KEY_SIZE = 128;
        private const int HASH_SIZE = 16;

        public static byte[] GenerateKey(string key) { 
            var keyBuffer = (key.ToLower() + STATIC_KEY).AsSpan();
            var keyBlob = new byte[KEY_SIZE];
            for (var i = 0; i < KEY_SIZE; i++) {
                keyBlob[i] = (byte) (keyBuffer[i % KEY_ITER] + i);
            }

            return keyBlob;
        }

        public static byte[] GenerateEncodingKey(string key) {
            Span<char> keyBuffer = (key.ToLower() + STATIC_KEY).ToArray();
            var keyBlob = new byte[KEY_SIZE];
            for (var i = 0; i < KEY_SIZE; i++) {
                keyBlob[i] = (byte) (i + ((byte) i % 3 + 2) * (byte) keyBuffer[^(i % KEY_ITER + 1)]);
            }

            return keyBlob;
        }

        public static byte[] GenerateHashedKey(string key, byte[] hash) {
            var keyBlob = new byte[KEY_SIZE];
            for (var i = 0; i < KEY_SIZE; i++) {
                keyBlob[i] = (byte) ((byte) (hash[i % HASH_SIZE] + 2 + (byte) i % 5) * (byte) key[i % key.Length] + i);
            }

            return keyBlob;
        }

        public static int CalculateHeaderOffset(string fileName) {
            var offset = 0;
            fileName = fileName.ToLower();
            foreach (var ch in fileName) {
                offset += ch;
            }

            return offset % 312 + 30;
        }

        public static int CalculateEntryTableOffset(string fileName) {
            var offset = 0;
            fileName = fileName.ToLower();
            foreach (var ch in fileName) {
                offset += ch * 3;
            }

            return offset % 212 + 33;
        }

        public static int CalculateChecksum(byte[] hash) {
            var checksum = 0;
            var baseValue = hash[1] + hash[2] + hash[3] + hash[4];
            for (var i = 0; i < 16; i += 2) {
                baseValue += (byte) (hash[6] + i);
                checksum += (byte) (hash[6] + i + 1);
            }

            return checksum + baseValue;
        }
    }
}
