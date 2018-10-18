using System;
using System.Net;

namespace DidacticalEnigma.Core.Models
{
    public class SearchEngine
    {
        public string Comment { get; }

        public string UrlRoot { get; }

        public string AdditionalText { get; }

        public bool Literal { get; }

        public string BuildSearch(string text)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            string query = text;
            if (Literal)
                query = "\"" + query + "\"";
            if (!string.IsNullOrEmpty(AdditionalText))
                query += " " + AdditionalText;

            return UrlRoot + WebUtility.UrlEncode(query);
        }

        public SearchEngine(string urlRoot, string additionalText = null, bool literal = false, string comment = null)
        {
            UrlRoot = urlRoot ?? throw new ArgumentNullException(nameof(urlRoot));
            AdditionalText = additionalText;
            Comment = comment ?? (!string.IsNullOrEmpty(additionalText) ? additionalText : new Uri(UrlRoot).Host);
            Literal = literal;
        }

        public override string ToString() => Comment;
    }
}
