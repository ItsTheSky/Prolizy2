using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prolizy2.API.Utils.Converters;

/// <summary>
/// Reads a <see cref="double"/> from whatever the gateway sends for a numeric field:
/// a number (<c>1</c>, <c>0.5</c>), a numeric string (<c>"1.5"</c>, <c>"1,5"</c>) or <c>null</c>.
/// Anything that is not a number becomes <c>0</c>, so that one odd value never
/// prevents the whole bulletin from loading.
/// </summary>
public sealed class LenientDoubleConverter : JsonConverter<double>
{
    public override bool HandleNull => true;

    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetDouble();

            case JsonTokenType.String:
                var text = reader.GetString();
                return double.TryParse(text?.Trim().Replace(',', '.'), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var value)
                    ? value
                    : 0d;

            case JsonTokenType.StartObject:
            case JsonTokenType.StartArray:
                reader.Skip();
                return 0d;

            default:
                return 0d;
        }
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value);
}
