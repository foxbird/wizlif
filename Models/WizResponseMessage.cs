using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Foxpaws.Wizlif.Models
{
    [JsonConverter(typeof(ResponseConverter))]
    public abstract class WizResponseMessage
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("env")]
        public string Environment { get; set; }
    }


    public class ResponseConverter : JsonConverter<WizResponseMessage>
    {
        public override WizResponseMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (JsonDocument.TryParseValue(ref reader, out var doc))
            {
                if (doc.RootElement.TryGetProperty("method", out var method)) 
                {
                    var methodValue = method.GetString();
                    var rootElement = doc.RootElement.GetRawText();

                    return methodValue switch
                    {
                        "registration" => JsonSerializer.Deserialize<WizRegisterResponseMessage>(rootElement, options),
                        _ => throw new JsonException($"{methodValue} is not a valid method")
                    };
                }

                throw new JsonException($"Failed to extract register property");
            }
            throw new JsonException($"Failed to parser Json document");
        }

        public override void Write(Utf8JsonWriter writer, WizResponseMessage value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }

}
