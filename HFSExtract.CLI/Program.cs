using System;
using System.IO;
using System.Linq;

namespace HFSExtract.CLI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.Error.WriteLine($"Usage: HFSExtract.CLI.exe hfs_directory extract_directory [summary.txt]");
                return 1;
            }

            HFSDirectoryMangler.LoadPaths(args.ElementAtOrDefault(2));

            foreach (var file in Directory.GetFiles(args[0], "*.hfs", SearchOption.TopDirectoryOnly))
            {
                var hfs = new HFS(file);
                var path = HFSDirectoryMangler.GetPath(Path.GetFileNameWithoutExtension(file));
                var dest = Path.Combine(args[1], path);
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
