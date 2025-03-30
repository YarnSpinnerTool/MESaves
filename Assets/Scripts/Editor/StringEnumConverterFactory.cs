using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

public class StringEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter? CreateConverter(Type type, JsonSerializerOptions options)
    {
        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            typeof(StringEnumConverterInner<>).MakeGenericType(type),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
            binder: null,
            args: new[] { options },
            culture: null)!;

        return converter;
    }

    private class StringEnumConverterInner<TEnum> : JsonConverter<TEnum> where TEnum : Enum
    {
        public StringEnumConverterInner(JsonSerializerOptions options) { }

        public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();

            int value;

            if (reader.TokenType == JsonTokenType.Number)
            {
                value = reader.GetInt32();
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                if (!int.TryParse(reader.GetString(), out value))
                {
                    throw new JsonException("Failed to parse string as integer");
                }
            }
            else
            {
                throw new JsonException($"{typeof(TEnum).Name}: Expected to read a number, or a string containing a number, not a {reader.TokenType} (parsing {reader.GetString()})");
            }

            return (TEnum)(object)value;
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            var resultString = ((int)(object)value).ToString();
            writer.WriteStringValue(resultString);
        }
    }
}