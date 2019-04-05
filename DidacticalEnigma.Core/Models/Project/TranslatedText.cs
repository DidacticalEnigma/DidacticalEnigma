using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DidacticalEnigma.Core.Models.Project
{
    public class TranslatedText
    {
        public string Text { get; }

        public string Author { get; }

        public TranslatedText(string author, string text)
        {
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}