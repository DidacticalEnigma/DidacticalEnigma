// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Swagger.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class KanjiLookupResult
    {
        /// <summary>
        /// Initializes a new instance of the KanjiLookupResult class.
        /// </summary>
        public KanjiLookupResult()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the KanjiLookupResult class.
        /// </summary>
        public KanjiLookupResult(IList<string> kanji, IList<string> possibleRadicals, IDictionary<string, string> usedRadicals)
        {
            Kanji = kanji;
            PossibleRadicals = possibleRadicals;
            UsedRadicals = usedRadicals;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "kanji")]
        public IList<string> Kanji { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "possibleRadicals")]
        public IList<string> PossibleRadicals { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "usedRadicals")]
        public IDictionary<string, string> UsedRadicals { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Kanji == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Kanji");
            }
            if (PossibleRadicals == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "PossibleRadicals");
            }
            if (UsedRadicals == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "UsedRadicals");
            }
        }
    }
}
