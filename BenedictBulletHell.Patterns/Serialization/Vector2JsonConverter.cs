using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BenedictBulletHell.Patterns.Serialization
{
    /// <summary>
    /// JSON converter for System.Numerics.Vector2.
    /// </summary>
    internal sealed class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object for Vector2.");

            float x = 0f, y = 0f;
            bool hasX = false, hasY = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    if (propertyName == null)
                        continue;
                    reader.Read();

                    switch (propertyName.ToLowerInvariant())
                    {
                        case "x":
                            x = reader.GetSingle();
                            hasX = true;
                            break;
                        case "y":
                            y = reader.GetSingle();
                            hasY = true;
                            break;
                    }
                }
            }

            if (!hasX || !hasY)
                throw new JsonException("Vector2 must have both 'x' and 'y' properties.");

            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.X);
            writer.WriteNumber("y", value.Y);
            writer.WriteEndObject();
        }
    }
}
