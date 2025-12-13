using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Patterns.Advanced;
using BenedictBulletHell.Patterns.Patterns.Basic;
using BenedictBulletHell.Patterns.Patterns.Composite;
using BenedictBulletHell.Patterns.Patterns.Modifiers;

namespace BenedictBulletHell.Patterns.Serialization
{
    /// <summary>
    /// JSON serializer for bullet patterns using System.Text.Json.
    /// </summary>
    public sealed class JsonPatternSerializer : IPatternSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new PatternJsonConverter(), new Vector2JsonConverter() }
        };

        /// <inheritdoc/>
        public string Serialize(IBulletPattern pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            return JsonSerializer.Serialize(pattern, Options);
        }

        /// <inheritdoc/>
        public IBulletPattern Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));

            try
            {
                var pattern = JsonSerializer.Deserialize<IBulletPattern>(json, Options);
                if (pattern == null)
                    throw new ArgumentException("Deserialized pattern is null. Invalid JSON structure.", nameof(json));
                return pattern;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to parse JSON: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public void Serialize(IBulletPattern pattern, Stream stream)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            JsonSerializer.Serialize(writer, pattern, Options);
        }

        /// <inheritdoc/>
        public IBulletPattern Deserialize(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
                var json = reader.ReadToEnd();
                return Deserialize(json);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to parse JSON from stream: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Custom JSON converter for polymorphic pattern serialization.
        /// </summary>
        private sealed class PatternJsonConverter : JsonConverter<IBulletPattern>
        {
            public override IBulletPattern Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected start of object.");

                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeElement))
                    throw new ArgumentException("Missing required 'type' field in pattern JSON.");

                string? patternType = typeElement.GetString();
                if (string.IsNullOrEmpty(patternType))
                    throw new ArgumentException("Pattern type cannot be null or empty.");

                return DeserializePattern(patternType, root);
            }

            public override void Write(Utf8JsonWriter writer, IBulletPattern value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString("type", GetPatternTypeName(value));

                switch (value)
                {
                    case RingPattern ring:
                        writer.WriteNumber("bulletCount", ring.BulletCount);
                        writer.WriteNumber("speed", ring.Speed);
                        writer.WriteNumber("startAngle", ring.StartAngle);
                        break;

                    case SingleShotPattern single:
                        writer.WritePropertyName("direction");
                        JsonSerializer.Serialize(writer, single.Direction, options);
                        writer.WriteNumber("speed", single.Speed);
                        break;

                    case BurstPattern burst:
                        writer.WriteNumber("bulletCount", burst.BulletCount);
                        writer.WritePropertyName("direction");
                        JsonSerializer.Serialize(writer, burst.Direction, options);
                        writer.WriteNumber("speed", burst.Speed);
                        writer.WriteNumber("delayBetweenShots", burst.DelayBetweenShots);
                        break;

                    case SpreadPattern spread:
                        writer.WriteNumber("bulletCount", spread.BulletCount);
                        writer.WriteNumber("angleSpread", spread.AngleSpread);
                        writer.WriteNumber("speed", spread.Speed);
                        writer.WriteNumber("baseAngle", spread.BaseAngle);
                        break;

                    case AimedPattern aimed:
                        writer.WriteNumber("bulletCount", aimed.BulletCount);
                        writer.WriteNumber("speed", aimed.Speed);
                        writer.WriteNumber("spreadAngle", aimed.SpreadAngle);
                        break;

                    case SpiralPattern spiral:
                        writer.WriteNumber("bulletsPerRevolution", spiral.BulletsPerRevolution);
                        writer.WriteNumber("totalRevolutions", spiral.TotalRevolutions);
                        writer.WriteNumber("speed", spiral.Speed);
                        writer.WriteNumber("rotationSpeed", spiral.RotationSpeed);
                        writer.WriteNumber("startAngle", spiral.StartAngle);
                        writer.WriteBoolean("looping", spiral.IsLooping);
                        break;

                    case WavePattern wave:
                        writer.WriteNumber("bulletCount", wave.BulletCount);
                        writer.WriteNumber("baseDirection", wave.BaseDirection);
                        writer.WriteNumber("waveAmplitude", wave.WaveAmplitude);
                        writer.WriteNumber("waveFrequency", wave.WaveFrequency);
                        writer.WriteNumber("speed", wave.Speed);
                        break;

                    case SequencePattern sequence:
                        writer.WriteBoolean("looping", sequence.IsLooping);
                        writer.WritePropertyName("patterns");
                        writer.WriteStartArray();
                        foreach (var pattern in sequence.Patterns)
                        {
                            JsonSerializer.Serialize(writer, pattern, options);
                        }
                        writer.WriteEndArray();
                        break;

                    case ParallelPattern parallel:
                        writer.WritePropertyName("patterns");
                        writer.WriteStartArray();
                        foreach (var pattern in parallel.Patterns)
                        {
                            JsonSerializer.Serialize(writer, pattern, options);
                        }
                        writer.WriteEndArray();
                        break;

                    case RepeatPattern repeat:
                        writer.WritePropertyName("pattern");
                        JsonSerializer.Serialize(writer, repeat.Pattern, options);
                        writer.WriteNumber("repeatCount", repeat.RepeatCount);
                        writer.WriteNumber("delayBetweenRepeats", repeat.DelayBetweenRepeats);
                        break;

                    case LoopPattern loop:
                        writer.WritePropertyName("pattern");
                        JsonSerializer.Serialize(writer, loop.Pattern, options);
                        break;

                    case RotatingPattern rotating:
                        writer.WriteNumber("rotationSpeed", rotating.RotationSpeed);
                        writer.WritePropertyName("pattern");
                        JsonSerializer.Serialize(writer, rotating.Pattern, options);
                        break;

                    default:
                        throw new NotSupportedException($"Pattern type {value.GetType().Name} is not supported for serialization.");
                }

                writer.WriteEndObject();
            }

            private static string GetPatternTypeName(IBulletPattern pattern)
            {
                return pattern switch
                {
                    RingPattern => "RingPattern",
                    SingleShotPattern => "SingleShotPattern",
                    BurstPattern => "BurstPattern",
                    SpreadPattern => "SpreadPattern",
                    AimedPattern => "AimedPattern",
                    SpiralPattern => "SpiralPattern",
                    WavePattern => "WavePattern",
                    SequencePattern => "SequencePattern",
                    ParallelPattern => "ParallelPattern",
                    RepeatPattern => "RepeatPattern",
                    LoopPattern => "LoopPattern",
                    RotatingPattern => "RotatingPattern",
                    _ => throw new NotSupportedException($"Pattern type {pattern.GetType().Name} is not supported.")
                };
            }

            private static IBulletPattern DeserializePattern(string patternType, JsonElement root)
            {
                return patternType switch
                {
                    "RingPattern" => DeserializeRingPattern(root),
                    "SingleShotPattern" => DeserializeSingleShotPattern(root),
                    "BurstPattern" => DeserializeBurstPattern(root),
                    "SpreadPattern" => DeserializeSpreadPattern(root),
                    "AimedPattern" => DeserializeAimedPattern(root),
                    "SpiralPattern" => DeserializeSpiralPattern(root),
                    "WavePattern" => DeserializeWavePattern(root),
                    "SequencePattern" => DeserializeSequencePattern(root),
                    "ParallelPattern" => DeserializeParallelPattern(root),
                    "RepeatPattern" => DeserializeRepeatPattern(root),
                    "LoopPattern" => DeserializeLoopPattern(root),
                    "RotatingPattern" => DeserializeRotatingPattern(root),
                    _ => throw new ArgumentException($"Unknown pattern type: {patternType}")
                };
            }

            private static RingPattern DeserializeRingPattern(JsonElement root)
            {
                if (!root.TryGetProperty("bulletCount", out var bulletCountElement))
                    throw new ArgumentException("RingPattern missing required 'bulletCount' field.");
                if (!root.TryGetProperty("speed", out var speedElement))
                    throw new ArgumentException("RingPattern missing required 'speed' field.");

                int bulletCount = bulletCountElement.GetInt32();
                float speed = speedElement.GetSingle();
                float startAngle = root.TryGetProperty("startAngle", out var startAngleElement) 
                    ? startAngleElement.GetSingle() 
                    : 0f;

                if (bulletCount < 1)
                    throw new ArgumentException("RingPattern bulletCount must be at least 1.");

                return new RingPattern(bulletCount, speed, startAngle);
            }

            private static SingleShotPattern DeserializeSingleShotPattern(JsonElement root)
            {
                if (!root.TryGetProperty("direction", out var directionElement))
                    throw new ArgumentException("SingleShotPattern missing required 'direction' field.");
                if (!root.TryGetProperty("speed", out var speedElement))
                    throw new ArgumentException("SingleShotPattern missing required 'speed' field.");

                Vector2 direction = JsonSerializer.Deserialize<Vector2>(directionElement.GetRawText(), Options);
                float speed = speedElement.GetSingle();

                return new SingleShotPattern(direction, speed);
            }

            private static BurstPattern DeserializeBurstPattern(JsonElement root)
            {
                if (!root.TryGetProperty("bulletCount", out var bulletCountElement))
                    throw new ArgumentException("BurstPattern missing required 'bulletCount' field.");
                if (!root.TryGetProperty("direction", out var directionElement))
                    throw new ArgumentException("BurstPattern missing required 'direction' field.");
                if (!root.TryGetProperty("speed", out var speedElement))
                    throw new ArgumentException("BurstPattern missing required 'speed' field.");
                if (!root.TryGetProperty("delayBetweenShots", out var delayElement))
                    throw new ArgumentException("BurstPattern missing required 'delayBetweenShots' field.");

                int bulletCount = bulletCountElement.GetInt32();
                Vector2 direction = JsonSerializer.Deserialize<Vector2>(directionElement.GetRawText(), Options);
                float speed = speedElement.GetSingle();
                float delay = delayElement.GetSingle();

                if (bulletCount < 1)
                    throw new ArgumentException("BurstPattern bulletCount must be at least 1.");
                if (delay < 0f)
                    throw new ArgumentException("BurstPattern delayBetweenShots cannot be negative.");

                return new BurstPattern(bulletCount, direction, speed, delay);
            }

            private static SpreadPattern DeserializeSpreadPattern(JsonElement root)
            {
                if (!root.TryGetProperty("bulletCount", out var bulletCountElement))
                    throw new ArgumentException("SpreadPattern missing required 'bulletCount' field.");
                if (!root.TryGetProperty("angleSpread", out var angleSpreadElement))
                    throw new ArgumentException("SpreadPattern missing required 'angleSpread' field.");
                if (!root.TryGetProperty("speed", out var speedElement))
                    throw new ArgumentException("SpreadPattern missing required 'speed' field.");

                int bulletCount = bulletCountElement.GetInt32();
                float angleSpread = angleSpreadElement.GetSingle();
                float speed = speedElement.GetSingle();
                float baseAngle = root.TryGetProperty("baseAngle", out var baseAngleElement) 
                    ? baseAngleElement.GetSingle() 
                    : 0f;

                if (bulletCount < 1)
                    throw new ArgumentException("SpreadPattern bulletCount must be at least 1.");

                return new SpreadPattern(bulletCount, angleSpread, speed, baseAngle);
            }

            private static AimedPattern DeserializeAimedPattern(JsonElement root)
            {
                if (!root.TryGetProperty("bulletCount", out var bulletCountElement))
                    throw new ArgumentException("AimedPattern missing required 'bulletCount' field.");
                if (!root.TryGetProperty("speed", out var speedElement))
                    throw new ArgumentException("AimedPattern missing required 'speed' field.");

                int bulletCount = bulletCountElement.GetInt32();
                float speed = speedElement.GetSingle();
                float spreadAngle = root.TryGetProperty("spreadAngle", out var spreadAngleElement) 
                    ? spreadAngleElement.GetSingle() 
                    : 0f;

                if (bulletCount < 1)
                    throw new ArgumentException("AimedPattern bulletCount must be at least 1.");
                if (spreadAngle < 0f)
                    throw new ArgumentException("AimedPattern spreadAngle cannot be negative.");

                return new AimedPattern(bulletCount, speed, spreadAngle);
            }

            private static SpiralPattern DeserializeSpiralPattern(JsonElement root)
            {
                if (!root.TryGetProperty("bulletsPerRevolution", out var bulletsPerRevElement))
                    throw new ArgumentException("SpiralPattern missing required 'bulletsPerRevolution' field.");
                if (!root.TryGetProperty("totalRevolutions", out var totalRevolutionsElement))
                    throw new ArgumentException("SpiralPattern missing required 'totalRevolutions' field.");
                if (!root.TryGetProperty("speed", out var speedElement))
                    throw new ArgumentException("SpiralPattern missing required 'speed' field.");

                int bulletsPerRevolution = bulletsPerRevElement.GetInt32();
                float totalRevolutions = totalRevolutionsElement.GetSingle();
                float speed = speedElement.GetSingle();
                float rotationSpeed = root.TryGetProperty("rotationSpeed", out var rotationSpeedElement) 
                    ? rotationSpeedElement.GetSingle() 
                    : 360f;
                float startAngle = root.TryGetProperty("startAngle", out var startAngleElement) 
                    ? startAngleElement.GetSingle() 
                    : 0f;
                bool looping = root.TryGetProperty("looping", out var loopingElement) 
                    && loopingElement.GetBoolean();

                if (bulletsPerRevolution < 1)
                    throw new ArgumentException("SpiralPattern bulletsPerRevolution must be at least 1.");
                if (totalRevolutions <= 0f && !looping)
                    throw new ArgumentException("SpiralPattern totalRevolutions must be greater than 0 for non-looping patterns.");
                if (rotationSpeed <= 0f)
                    throw new ArgumentException("SpiralPattern rotationSpeed must be greater than 0.");

                return new SpiralPattern(bulletsPerRevolution, totalRevolutions, speed, rotationSpeed, startAngle, looping);
            }

            private static WavePattern DeserializeWavePattern(JsonElement root)
            {
                if (!root.TryGetProperty("bulletCount", out var bulletCountElement))
                    throw new ArgumentException("WavePattern missing required 'bulletCount' field.");
                if (!root.TryGetProperty("baseDirection", out var baseDirectionElement))
                    throw new ArgumentException("WavePattern missing required 'baseDirection' field.");
                if (!root.TryGetProperty("waveAmplitude", out var waveAmplitudeElement))
                    throw new ArgumentException("WavePattern missing required 'waveAmplitude' field.");
                if (!root.TryGetProperty("waveFrequency", out var waveFrequencyElement))
                    throw new ArgumentException("WavePattern missing required 'waveFrequency' field.");
                if (!root.TryGetProperty("speed", out var speedElement))
                    throw new ArgumentException("WavePattern missing required 'speed' field.");

                int bulletCount = bulletCountElement.GetInt32();
                float baseDirection = baseDirectionElement.GetSingle();
                float waveAmplitude = waveAmplitudeElement.GetSingle();
                float waveFrequency = waveFrequencyElement.GetSingle();
                float speed = speedElement.GetSingle();

                if (bulletCount < 1)
                    throw new ArgumentException("WavePattern bulletCount must be at least 1.");
                if (waveAmplitude < 0f)
                    throw new ArgumentException("WavePattern waveAmplitude cannot be negative.");

                return new WavePattern(bulletCount, baseDirection, waveAmplitude, waveFrequency, speed);
            }

            private static SequencePattern DeserializeSequencePattern(JsonElement root)
            {
                if (!root.TryGetProperty("patterns", out var patternsElement))
                    throw new ArgumentException("SequencePattern missing required 'patterns' field.");

                var patterns = new List<IBulletPattern>();
                foreach (var patternElement in patternsElement.EnumerateArray())
                {
                    var pattern = JsonSerializer.Deserialize<IBulletPattern>(patternElement.GetRawText(), Options);
                    if (pattern == null)
                        throw new ArgumentException("SequencePattern contains null pattern.");
                    patterns.Add(pattern);
                }

                if (patterns.Count == 0)
                    throw new ArgumentException("SequencePattern must contain at least one pattern.");

                bool looping = root.TryGetProperty("looping", out var loopingElement) 
                    && loopingElement.GetBoolean();

                return new SequencePattern(patterns, looping);
            }

            private static ParallelPattern DeserializeParallelPattern(JsonElement root)
            {
                if (!root.TryGetProperty("patterns", out var patternsElement))
                    throw new ArgumentException("ParallelPattern missing required 'patterns' field.");

                var patterns = new List<IBulletPattern>();
                foreach (var patternElement in patternsElement.EnumerateArray())
                {
                    var pattern = JsonSerializer.Deserialize<IBulletPattern>(patternElement.GetRawText(), Options);
                    if (pattern == null)
                        throw new ArgumentException("ParallelPattern contains null pattern.");
                    patterns.Add(pattern);
                }

                if (patterns.Count == 0)
                    throw new ArgumentException("ParallelPattern must contain at least one pattern.");

                return new ParallelPattern(patterns);
            }

            private static RepeatPattern DeserializeRepeatPattern(JsonElement root)
            {
                if (!root.TryGetProperty("pattern", out var patternElement))
                    throw new ArgumentException("RepeatPattern missing required 'pattern' field.");
                if (!root.TryGetProperty("repeatCount", out var repeatCountElement))
                    throw new ArgumentException("RepeatPattern missing required 'repeatCount' field.");

                var pattern = JsonSerializer.Deserialize<IBulletPattern>(patternElement.GetRawText(), Options);
                if (pattern == null)
                    throw new ArgumentException("RepeatPattern pattern cannot be null.");

                int repeatCount = repeatCountElement.GetInt32();
                float delayBetweenRepeats = root.TryGetProperty("delayBetweenRepeats", out var delayElement) 
                    ? delayElement.GetSingle() 
                    : 0f;

                if (repeatCount < 1)
                    throw new ArgumentException("RepeatPattern repeatCount must be at least 1.");
                if (delayBetweenRepeats < 0f)
                    throw new ArgumentException("RepeatPattern delayBetweenRepeats cannot be negative.");

                return new RepeatPattern(pattern, repeatCount, delayBetweenRepeats);
            }

            private static LoopPattern DeserializeLoopPattern(JsonElement root)
            {
                if (!root.TryGetProperty("pattern", out var patternElement))
                    throw new ArgumentException("LoopPattern missing required 'pattern' field.");

                var pattern = JsonSerializer.Deserialize<IBulletPattern>(patternElement.GetRawText(), Options);
                if (pattern == null)
                    throw new ArgumentException("LoopPattern pattern cannot be null.");

                return new LoopPattern(pattern);
            }

            private static RotatingPattern DeserializeRotatingPattern(JsonElement root)
            {
                if (!root.TryGetProperty("rotationSpeed", out var rotationSpeedElement))
                    throw new ArgumentException("RotatingPattern missing required 'rotationSpeed' field.");
                if (!root.TryGetProperty("pattern", out var patternElement))
                    throw new ArgumentException("RotatingPattern missing required 'pattern' field.");

                float rotationSpeed = rotationSpeedElement.GetSingle();
                var pattern = JsonSerializer.Deserialize<IBulletPattern>(patternElement.GetRawText(), Options);
                if (pattern == null)
                    throw new ArgumentException("RotatingPattern pattern cannot be null.");

                return new RotatingPattern(pattern, rotationSpeed);
            }
        }
    }
}
