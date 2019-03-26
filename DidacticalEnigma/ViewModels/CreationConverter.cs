using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional.Collections;

namespace DidacticalEnigma.ViewModels
{
    public class ElementCreationConverter : JsonConverter
    {
        private readonly Func<Element> factory;

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public ElementCreationConverter(Func<Element> factory)
        {
            this.factory = factory;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var e = default(Element);
            switch (jsonObject["Type"].Value<string>())
            {
                case "root":
                    e = new Root(factory);
                    break;
                case "vsplit":
                    e = new VSplit();
                    break;
                case "hsplit":
                    e = new HSplit();
                    break;
                case "end":
                    e = factory();
                    break;
            }
            serializer.Populate(jsonObject.CreateReader(), e);
            return e;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Element).IsAssignableFrom(objectType);
        }
    }

    public class DataSourceConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DataSourcePreviewVM o)
            {
                writer.WriteValue(o.SelectedDataSource?.Entity.Descriptor.Guid);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = (DataSourcePreviewVM)existingValue;
            var input = (string)reader.Value;
            var guid = input != null ? Guid.Parse(input) : (Guid?)null;
            var dataSourceIndex = o.Parent.DataSources
                .Select((d, i) => (d, i))
                .Where(p => p.d.Entity.Descriptor.Guid == guid)
                .Select(p => p.i)
                .SingleOrNone()
                .ValueOr(-1);
            o.SelectedDataSourceIndex = dataSourceIndex;
            return existingValue;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }
    }
}