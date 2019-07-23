using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OCR.Abstractions.Models
{
    public partial class Annotation
    {
        [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
        public string Locale { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("boundingPoly")]
        public BoundingPoly BoundingPoly { get; set; }

        public string Text { get; set; }

        public string Time { get; set; }
    }

    public partial class BoundingPoly
    {
        [JsonProperty("vertices")]
        public List<Vertex> Vertices { get; set; }
    }

    public partial class Vertex
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public partial class Annotation
    {
        public static List<Annotation> FromJson(string json) => JsonConvert.DeserializeObject<List<Annotation>>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<Annotation> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
