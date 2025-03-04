using MongoDB.Bson;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BackendProjectAPI.Models
{
    public class Usuario
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }


        [BsonRequired]
        public string Nombre { get; set; }


        [BsonRequired]
        public string Apellidos { get; set; }


        [BsonRequired]
        public string Cedula { get; set; }

        [BsonRequired]
        public string Correo { get; set; }

        [BsonRequired]
        public string Password { get; set; }


        public DateTime? FechaUltimoAcceso { get; set; }

        public string? Clasificacion { get; set; }
    }

    public class LoginUsuario
    {
        [BsonRequired]
        public string Correo { get; set; }

        [BsonRequired]
        public string Password { get; set; }
    }

    public class ObjectIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((ObjectId)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ObjectId.Parse((string)reader.Value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObjectId);
        }
    }
}