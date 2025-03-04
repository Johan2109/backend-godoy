using MongoDB.Bson;
using Newtonsoft.Json;
using System;

namespace BackendProjectAPI.Converters
{
    using MongoDB.Bson;
    using Newtonsoft.Json;
    using System;

    public class ObjectIdConverter : JsonConverter<ObjectId>
    {
        public override void WriteJson(JsonWriter writer, ObjectId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override ObjectId ReadJson(JsonReader reader, Type objectType, ObjectId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            return value != null ? new ObjectId(value) : ObjectId.Empty;
        }
    }
}