using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("JDict.Tests")]

namespace JDict
{
    internal class InvertedIndex
    {
        //Stream stream;

        private InvertedIndex(Stream index, Stream file)
        {
            //BinaryReader r;

        }

        public static InvertedIndex Load(string path)
        {
            throw new NotImplementedException();
        }

        public static InvertedIndex CreateOrLoad(string path)
        {
            throw new NotImplementedException();
        }

        public static void Create(string path)
        {
            throw new NotImplementedException();
        }
    }

    // very primitive index class
    // only supports read operations and string to string mappings
    // file format: 32 byte header
    // 4 first bytes: "IND\x01"
    // 8 next bytes correspond to the position in the text file where the values start, little endian
    // 20 bytes reserved
    // Sequence of:
    // - null terminated UTF-8 string
    // 
    // rest of the record constitute data bytes
    // a sequence of records is an entry
    // disregarding continuation bytes
    // the entry contains the key text, followed by null character, followed by value text, followed by null character
    internal class FileBasedDictionary : IReadOnlyDictionary<string, string>, IDisposable
    {
        private string path;

        // precondition: the record length is buffer.Length
        // precondition: the stream position is positioned at the start of the record
        // postcondition: the stream position is positioned at the start of the record that corresponds to the next entry, or at the end of the file
        private static KeyValuePair<string, string> ReadEntry(Stream stream, byte[] buffer, out bool hasNext)
        {
            //long entryLengthEstimation = long.MaxValue;
            if(stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new InvalidDataException("corrupted file");
            }
            throw new InvalidDataException("corrupted file");
        }

        private struct Header
        {
            public int Version { get; }

            public ulong Count { get; }

            public uint RecordLength { get; }

            public Header(byte[] input)
            {
                using (var ms = new MemoryStream(input))
                using (var reader = new BinaryReader(ms))
                {
                    bool valid = true;
                    valid = valid & reader.ReadByte() == 'I';
                    valid = valid & reader.ReadByte() == 'N';
                    valid = valid & reader.ReadByte() == 'D';
                    if(!valid)
                    {
                        throw new InvalidDataException("not a valid header");
                    }
                    Version = reader.ReadByte();
                    if(Version != 1)
                    {
                        throw new InvalidDataException("version not supported");
                    }
                    Count = reader.ReadUInt64();
                    RecordLength = reader.ReadUInt32();
                    var ignored = reader.ReadBytes(16);
                    if(ignored.Length != 16)
                    {
                        throw new InvalidDataException("not a valid header");
                    }
                }
            }
        }

        private string Lookup(string key)
        {
            throw new NotImplementedException();
        }

        public string this[string key]
        {
            get
            {
                var value = Lookup(key);
                if (value == null)
                    throw new KeyNotFoundException();
                return value;
            }
        }

        public IEnumerable<string> Keys => this.Select(kvp => kvp.Key);

        public IEnumerable<string> Values => this.Select(kvp => kvp.Value);

        public int Count
        {
            get
            {
                ulong kvpCount;
                using (var s = File.OpenRead(path))
                {
                    var headerBytes = new byte[32];
                    if (s.Read(headerBytes, 0, headerBytes.Length) != headerBytes.Length)
                    {
                        throw new InvalidDataException("premature end of file");
                    }
                    var header = new Header(headerBytes);
                    kvpCount = header.Count;
                }
                    if (kvpCount > int.MaxValue)
                {
                    throw new ArgumentException();
                }
                return (int)kvpCount;
            }
        }

        public bool ContainsKey(string key)
        {
            var value = Lookup(key);
            return value != null;
        }

        public void Dispose()
        {
            
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            using (var s = File.OpenRead(path))
            {
                var headerBytes = new byte[32];
                if(s.Read(headerBytes, 0, headerBytes.Length) != headerBytes.Length)
                {
                    throw new InvalidDataException("premature end of file");
                }
                var header = new Header(headerBytes);
                bool hasNext = false;
                var buffer = new byte[header.RecordLength];
                while (true)
                {
                    yield return ReadEntry(s, buffer, out hasNext);
                    if (!hasNext)
                        break;
                }
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            value = Lookup(key);
            return value != null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public FileBasedDictionary(string path)
        {
            this.path = path;
        }
    }
}
