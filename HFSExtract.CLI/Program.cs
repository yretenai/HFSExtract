using System;
using System.IO;

namespace HFSExtract.CLI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.Error.WriteLine($"Usage: HFSExtract.CLI.exe hfs_directory extract_directory");
                return 1;
            }

            foreach (var file in Directory.GetFiles(args[0], "*.hfs", SearchOption.TopDirectoryOnly))
            {
                var hfs = new HFS(file);
                var dest = Path.Combine(args[1], Path.GetFileNameWithoutExtension(file));
                if (!Directory.Exists(dest))
                {
                    Directory.CreateDirectory(dest);
                }
                foreach (var entry in hfs.Entries)
                {
                    Console.Out.WriteLine(entry.File.Filename);
                    File.WriteAllBytes(Path.Combine(dest, entry.File.Filename), entry.File.Data);
                }
            }
            return 0;
        }
    }
}
