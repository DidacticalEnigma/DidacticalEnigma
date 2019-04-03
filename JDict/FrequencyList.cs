using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JDict
{
    public class FrequencyList
    {
        private readonly Dictionary<string, double> frequency;

        public FrequencyList(string path, Encoding encoding)
        {
            this.frequency = File.ReadLines(path, encoding)
                .Skip(4)
                .Where(line => line != "")
                .Select(line => line.Split(' '))
                .ToDictionary(i => i[2], i => double.Parse(i[1], CultureInfo.InvariantCulture));
        }

        public IEnumerable<KeyValuePair<string, double>> GetAllWords() =>
            frequency.OrderByDescending(kvp => kvp.Value);

        public double RateFrequency(string s)
        {
            if (frequency.TryGetValue(s, out var value))
            {
                return value;
            }
            return double.NegativeInfinity;
        }
    }
}
