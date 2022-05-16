using FarmCraft.Community.Data.DTOs;
using Newtonsoft.Json;

namespace FarmCraft.Community.Data.Converters
{
    public class ResponseStatusConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object? ReadJson(
            JsonReader reader, 
            Type objectType, 
            object? existingValue, 
            JsonSerializer serializer
        )
        {
            try
            {
                string value = reader.Value == null
                    ? ""
                    : (string)reader.Value;

                switch(value)
                {
                    case "Success":
                        return ResponseStatus.Success;
                    case "Failure":
                        return ResponseStatus.Failure;
                    default:
                        throw new Exception();
                }
            }
            catch (Exception)
            {
                return ResponseStatus.Failure;
            }
        }

        public override void WriteJson(
            JsonWriter writer, 
            object? value, 
            JsonSerializer 
            serializer
        )
        {
            try
            {
                ResponseStatus status = value != null
                    ? (ResponseStatus)value
                    : ResponseStatus.Failure;

                switch(status)
                {
                    case ResponseStatus.Success:
                        writer.WriteValue("Failure");
                        break;
                    case ResponseStatus.Failure:
                        writer.WriteValue("Failure");
                        break;
                    default:
                        throw new Exception();
                }
            }
            catch(Exception)
            {
                writer.WriteNull();
            }
        }
    }
}
