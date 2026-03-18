using Auction.Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auction.Api.Converters;

public class MoneyJsonConverter : JsonConverter<Money>
{
    public override Money? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object for Money type.");
        }

        decimal value = 0;
        string? currency = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name.");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName.ToLowerInvariant())
            {
                case "value":
                    value = reader.GetDecimal();
                    break;
                case "currency":
                    currency = reader.GetString();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new JsonException("Currency is required for Money type.");
        }

        var result = Money.Create(value, currency);

        if (!result.IsSuccess)
        {
            throw new JsonException($"Failed to create Money: {result.Error?.Message}");
        }

        return result.Value;
    }

    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("value", value.Value);
        writer.WriteString("currency", value.Currency);
        writer.WriteEndObject();
    }
}
