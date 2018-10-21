using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JDict.Utils
{
    class ZipFile : IZipFile
    {
        private readonly string tempDir;

        private static readonly string[] executableCandidates;

        static ZipFile()
        {
            var l = new List<string>();
            try
            {
                l.Add("7z");
                l.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "7-Zip",
                    "7z.exe"));
                l.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip",
                    "7z.exe"));
                l.Add(Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), "7-Zip", "7z.exe"));
            }
            catch (Exception)
            {
                // ignored to avoid type initialization errors
                // the most important paths are filled in order they're supposed to be used
            }
            finally
            {
                executableCandidates = l.ToArray();
            }
        }

        public void Dispose()
        {
            Directory.Delete(tempDir, recursive: true);
        }

        public IEnumerable<string> Files => new DirectoryInfo(tempDir).EnumerateFiles().Select(f => f.Name);

        public Stream OpenFile(string path)
        {
            return File.OpenRead(Path.Combine(tempDir, path));
        }

        public ZipFile(string path)
        {
            tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(tempDir);
            foreach (var executable in executableCandidates)
            {
                try
                {
                    var si = new ProcessStartInfo(executable, $"x -o\"{tempDir}\" -- {path}")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };
                    using (var process = Process.Start(si))
                    {
                        process?.WaitForExit();
                        int exitCode = process?.ExitCode ?? 255;
                        if (exitCode == 0)
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }
            }
        }
    }
}