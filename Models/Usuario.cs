using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace BackendProjectAPI.Models
{
    public class Usuario
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [BsonElement("nombre")]
        [BsonRequired]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("apellidos")]
        [BsonRequired]
        public string Apellidos { get; set; } = string.Empty;

        [BsonElement("cedula")]
        [BsonRequired]
        [RegularExpression(@"^\d{6,12}$", ErrorMessage = "La cédula debe contener entre 6 y 12 dígitos numéricos.")]
        public string Cedula { get; set; } = string.Empty;

        [BsonElement("correo")]
        [BsonRequired]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string Correo { get; set; } = string.Empty;

        [BsonIgnore]
        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        [BsonElement("fechaUltimoAcceso")]
        public DateTime? FechaUltimoAcceso { get; set; } = DateTime.UtcNow;

        [BsonElement("clasificacion")]
        [BsonDefaultValue("Sin clasificación")]
        public string Clasificacion { get; set; } = "Sin clasificación";

        [BsonElement("puntaje")]
        [BsonDefaultValue(0)]
        [BsonRepresentation(BsonType.Int32)]
        [BsonIgnoreIfDefault]
        public int Puntaje { get; set; } = 0;
    }

    public class LoginUsuario
    {
        [BsonRequired]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string Correo { get; set; } = string.Empty;

        [BsonRequired]
        public string Password { get; set; } = string.Empty;
    }

    public class ObjectIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value != null ? ObjectId.Parse(reader.Value.ToString()) : ObjectId.Empty;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObjectId);
        }
    }
}
