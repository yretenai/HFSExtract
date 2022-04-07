using System;
using System.IO;

namespace HFSExtract.CLI {
    internal static class Program {
        public static int Main(string[] args) {
            Console.Out.WriteLine("HFSExtract v2 - Special Thanks to EKey");
            
            if (args.Length < 2) {
                Console.Error.WriteLine("Usage: HFSExtract.CLI.exe hfs_directory extract_directory");
                return 1;
            }

            var output = args[1];
            
            foreach (var file in Directory.GetFiles(args[0], "*.hfs", SearchOption.TopDirectoryOnly)) {
                Console.WriteLine(Path.GetFileName(file));
                try {
                    using var hfs = new HFSArchive(File.OpenRead(file), Path.GetFileName(file));
                    foreach (var filename in hfs.Files.Keys) {
                        var target = Path.Combine(output, filename);
                        var dir = Path.GetDirectoryName(target) ?? output;
                        if (!Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }

                        Console.WriteLine(filename);
                        File.WriteAllBytes(target, hfs.ReadFile(filename).ToArray());
                    }
                } catch (Exception e) {
                    Console.Error.WriteLine(e.Message);
                }
            }

            return 0;
        }
    }
}
