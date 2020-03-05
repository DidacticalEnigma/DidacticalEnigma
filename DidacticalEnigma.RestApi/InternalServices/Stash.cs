using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Optional;

namespace DidacticalEnigma.RestApi.InternalServices
{
    public class Stash<T> : IDisposable, IStash<T>
    {
        private readonly ConcurrentDictionary<string, (TimeSpan timestamp, T value)> data = 
            new ConcurrentDictionary<string, (TimeSpan timestamp, T value)>();

        private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        private readonly Stopwatch watch = Stopwatch.StartNew();

        private readonly TimeSpan expiryTime;

        public Option<T> Get(string identifier)
        {
            if (data.TryGetValue(identifier, out var v))
            {
                data[identifier] = (watch.Elapsed, v.value);
                return v.value.Some();
            }

            return Option.None<T>();
        }

        public void Delete(string identifier)
        {
            data.TryRemove(identifier, out _);
        }

        public string Put(T value)
        {
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            var identifier = Convert.ToBase64String(bytes);
            data[identifier] = (watch.Elapsed, value);
            return identifier;
        }

        public void IssueCleanup()
        {
            var current = watch.Elapsed;
            foreach (var (identifier, (timestamp, _)) in data)
            {
                if (current - timestamp >= expiryTime)
                {
                    data.TryRemove(identifier, out _);
                }
            }
        }

        public void Dispose()
        {
            rng.Dispose();
        }

        public Stash(TimeSpan expiryTime)
        {
            this.expiryTime = expiryTime;
        }
    }
}
