using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DidacticalEnigma.Models
{
    public enum ThemeType
    {
        Default,
        Dark
    }

    public class Settings : INotifyPropertyChanged
    {
        private ThemeType themeType;

        private IReadOnlyList<SearchEngine> searchEngines;

        [JsonConverter(typeof(StringEnumConverter))]
        public ThemeType ThemeType
        {
            get => themeType;
            set
            {
                if (value == themeType) return;
                themeType = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<SearchEngine> SearchEngines
        {
            get => searchEngines;
            set
            {
                if (Equals(value, searchEngines)) return;
                searchEngines = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Settings CreateDefault()
        {
            return new Settings
            {
                ThemeType = ThemeType.Default,
                SearchEngines = new List<SearchEngine>
                {
                    new SearchEngine("https://duckduckgo.com/?q=", "site:japanese.stackexchange.com", literal: true,
                        comment: "Search Japanese Stack Exchange"),
                    new SearchEngine("https://duckduckgo.com/?q=", "site:maggiesensei.com", literal: true,
                        comment: "Search Maggie Sensei website"),
                    new SearchEngine("https://duckduckgo.com/?q=", "site:www.japanesewithanime.com", literal: true,
                        comment: "Search Japanese with Anime blog"),
                    new SearchEngine("https://duckduckgo.com/?q=", "とは", literal: true, comment: "What is...?"),
                    new SearchEngine("https://duckduckgo.com/?q=", "意味", literal: true, comment: "Meaning...?"),
                    new SearchEngine("https://duckduckgo.com/?q=", "英語", literal: true, comment: "English...?"),
                    new SearchEngine(
                        "http://www.nihongoresources.com/dictionaries/universal.html?type=sfx&query=",
                        null,
                        literal: false,
                        comment: "nihongoresources.com SFX search"),
                    new SearchEngine(
                        "http://thejadednetwork.com/sfx/search/?submitSearch=Search+SFX&x=&keyword=",
                        null,
                        literal: false,
                        comment: "The JADED Network SFX search"),

                }.AsReadOnly()
            };
        }
    }
}
