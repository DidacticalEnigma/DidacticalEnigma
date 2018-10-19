using System;
using System.Collections.Generic;

namespace EpwingRemapper.Core
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var config = new Configuration();
            using (var e = (args as IEnumerable<string>).GetEnumerator())
            {
                while (e.MoveNext())
                {
                    switch (e.Current?.Trim())
                    {
                        case "-o":
                        case "--output":
                            ReadOutputDir(config, e);
                            break;
                        case "-i":
                        case "--bitmap-json-data":
                            ReadCapture2TextCliExecutablePath(config, e);
                            break;
                        case "-c2t":
                        case "--capture2text-path":
                            ReadInputBitmapJson(config, e);
                            break;
                        case "-h":
                        case "--help":
                            DisplayHelp();
                            return 0;
                    }
                }
            }

            return Main(config);

            void ReadOutputDir(Configuration cfg, IEnumerator<string> e)
            {
                if (!e.MoveNext())
                    throw new ArgumentException();

                cfg.OutputDirectory = e.Current;
            }

            void ReadCapture2TextCliExecutablePath(Configuration cfg, IEnumerator<string> e)
            {
                if (!e.MoveNext())
                    throw new ArgumentException();

                cfg.Capture2TextCliExecutablePath = e.Current;
            }

            void ReadInputBitmapJson(Configuration cfg, IEnumerator<string> e)
            {
                if (!e.MoveNext())
                    throw new ArgumentException();

                cfg.InputBitmapJson = e.Current;
            }

            void DisplayHelp()
            {
                Console.WriteLine("Usage: program -c2t path-to-your-capture2text-executable -i exported-bitmap-data -o output-directory");
            }
        }

        public static int Main(Configuration config)
        {
            config.Validate();


            return 0;
        }
    }

    public class Configuration
    {
        // MANDATORY
        public string Capture2TextCliExecutablePath { get; set; }

        // MANDATORY
        public string OutputDirectory { get; set; }

        // MANDATORY
        public string InputBitmapJson { get; set; }

        internal void Validate()
        {
            if(Capture2TextCliExecutablePath == null)
                throw new ArgumentException("this argument is mandatory", nameof(Capture2TextCliExecutablePath));

            if (OutputDirectory == null)
                throw new ArgumentException("this argument is mandatory", nameof(OutputDirectory));

            if (InputBitmapJson == null)
                throw new ArgumentException("this argument is mandatory", nameof(InputBitmapJson));

        }

    }
}
