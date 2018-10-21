using System;
using System.Collections.Generic;
using System.IO;

namespace JDict.Utils
{
    public interface IZipFile : IDisposable
    {
        // recursively list all files in the archive
        // no requirements are placed on the order the files are listed in
        // the directory separator is /
        // file paths don't start with /
        // example:
        // a zip file that contains the following in the root directory:
        // - an empty directory named A
        // - a directory named B that contains a file named C
        // - a file named D
        // is expected to provide two paths in the returned enumerable:
        // "B/C" and "D"
        IEnumerable<string> Files { get; }

        // opens the file in a read-only mode
        // valid strings are the ones provided by Files
        // if the file doesn't exist, throw a FileNotFoundException
        Stream OpenFile(string path);
    }
}