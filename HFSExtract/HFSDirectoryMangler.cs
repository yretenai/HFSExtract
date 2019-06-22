using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace HFSExtract
{
    public static class HFSDirectoryMangler
    {
        public static Dictionary<string, string> HashMap { get; set; } = new Dictionary<string, string>();

        public static void AddPath(string path)
        {
            HashMap[ComputeHash(path)] = path;
        }

        public static string ComputeHash(string path)
        {
            // models/monster/ogre2 => MoDeLs@mOnStEr@oGrE2.0
            // models/monster/puppet_ogre => MoDeLs@mOnStEr@pUpPeT_OgRe.0
            var buffer = new byte[path.Length + 2];
            buffer[path.Length] = (byte)'.';
            buffer[path.Length + 1] = (byte)'0';
            var inv = true;
            for (int i = 0; i < path.Length; ++i)
            {
                if (path[i] >= 'a' && path[i] <= 'z' || path[i] >= 'A' && path[i] <= 'Z')
                    buffer[i] = (byte)(path[i] ^ (inv ? 32 : 0));
                else if (path[i] == '/')
                    buffer[i] = (byte)'@';
                else
                    buffer[i] = (byte)path[i];
                inv = !inv;
            }

            using var sha = new SHA1CryptoServiceProvider();
            return BitConverter.ToString(sha.ComputeHash(buffer)).Replace("-", "");
        }

        public static string GetPath(string hash)
        {
            return HashMap.TryGetValue(hash, out var path) ? path : hash;
        }

        public static void LoadPaths(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            var allLines = File.ReadAllLines(path);

            foreach (var line in allLines.Where(x => x.Contains("->")).Select(x => (Hash: x.Substring(0, 40), Path: x.Substring(44).Trim())))
            {
#if DEBUG
                Contract.Assert(line.Hash == ComputeHash(line.Path), "Hash == Computed Hash");
#endif
                HashMap[line.Hash] = line.Path;
            }

            foreach (var line in allLines.Where(x => !x.Contains("->") && x.Length > 0))
            {
                HashMap[ComputeHash(line)] = line;
            }
        }
    }
}
