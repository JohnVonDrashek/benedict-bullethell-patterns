using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Patterns.Advanced;
using BenedictBulletHell.Patterns.Patterns.Basic;
using BenedictBulletHell.Patterns.Patterns.Composite;
using BenedictBulletHell.Patterns.Patterns.Modifiers;
using BenedictBulletHell.Patterns.Serialization;
using BenedictBulletHell.Patterns.Tests.Helpers;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests
{
    public class PatternSerializationTests
    {
        // Helper to get serializer instance (will be implemented)
        private IPatternSerializer GetSerializer()
        {
            // This will fail - IPatternSerializer doesn't exist yet
            return new JsonPatternSerializer();
        }

        // Helper to compare two patterns by their spawn behavior
        private void AssertPatternsEquivalent(IBulletPattern original, IBulletPattern deserialized, PatternContext context)
        {
            // Get all spawns from both patterns over a reasonable time range
            var originalSpawns = original.GetSpawns(0f, 10f, context).ToList();
            var deserializedSpawns = deserialized.GetSpawns(0f, 10f, context).ToList();

            Assert.Equal(originalSpawns.Count, deserializedSpawns.Count);

            for (int i = 0; i < originalSpawns.Count; i++)
            {
                var originalSpawn = originalSpawns[i];
                var deserializedSpawn = deserializedSpawns[i];

                PatternTestHelpers.AssertSpawnPosition(deserializedSpawn, originalSpawn.Position);
                PatternTestHelpers.AssertSpawnDirection(deserializedSpawn, originalSpawn.Direction);
                PatternTestHelpers.AssertSpawnSpeed(deserializedSpawn, originalSpawn.Speed);
                PatternTestHelpers.AssertSpawnAngle(deserializedSpawn, originalSpawn.Angle);
            }
        }

        #region Basic Pattern Round-Trip Tests

        [Fact]
        public void Serialize_RingPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var original = new RingPattern(8, 150f, 45f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_SingleShotPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var original = new SingleShotPattern(new Vector2(1, 0), 200f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_BurstPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var original = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_SpreadPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var original = new SpreadPattern(5, 45f, 200f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        #endregion

        #region Advanced Pattern Round-Trip Tests

        [Fact]
        public void Serialize_AimedPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var original = new AimedPattern(3, 150f, 10f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero, new Vector2(100, 50));

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_SpiralPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var original = new SpiralPattern(12, 2f, 180f, 360f, 0f, false);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_WavePattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var original = new WavePattern(10, 0f, 30f, 2f, 150f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        #endregion

        #region Composite Pattern Round-Trip Tests

        [Fact]
        public void Serialize_SequencePattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var pattern1 = new RingPattern(8, 150f);
            var pattern2 = new SpreadPattern(5, 45f, 200f);
            var original = new SequencePattern(new IBulletPattern[] { pattern1, pattern2 }, looping: false);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_ParallelPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var pattern1 = new RingPattern(8, 150f);
            var pattern2 = new SpreadPattern(5, 45f, 200f);
            var original = new ParallelPattern(new IBulletPattern[] { pattern1, pattern2 });
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_RepeatPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var basePattern = new RingPattern(8, 150f);
            var original = new RepeatPattern(basePattern, 3, 0.5f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_LoopPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var basePattern = new RingPattern(8, 150f);
            var original = new LoopPattern(basePattern);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        #endregion

        #region Modifier Pattern Round-Trip Tests

        [Fact]
        public void Serialize_RotatingPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var basePattern = new RingPattern(8, 150f);
            var original = new RotatingPattern(basePattern, 90f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        #endregion

        #region Nested Composite Pattern Tests

        [Fact]
        public void Serialize_NestedRotatingSequence_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var ring = new RingPattern(8, 150f);
            var spread = new SpreadPattern(5, 45f, 200f);
            var sequence = new SequencePattern(new IBulletPattern[] { ring, spread }, looping: false);
            var original = new RotatingPattern(sequence, 90f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_DeeplyNestedPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var inner1 = new RingPattern(4, 100f);
            var inner2 = new SpreadPattern(3, 30f, 150f);
            var inner3 = new BurstPattern(2, new Vector2(0, 1), 200f, 0.1f);
            var sequence = new SequencePattern(new IBulletPattern[] { inner1, inner2, inner3 }, looping: false);
            var parallel = new ParallelPattern(new IBulletPattern[] { sequence, new RingPattern(6, 120f) });
            var repeat = new RepeatPattern(parallel, 2, 1f);
            var original = new RotatingPattern(repeat, 45f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Serialize_EmptySequence_HandlesGracefully()
        {
            var serializer = GetSerializer();
            // Empty sequences should be invalid, but test error handling
            var json = "{\"type\":\"SequencePattern\",\"patterns\":[]}";

            Assert.Throws<ArgumentException>(() => serializer.Deserialize(json));
        }

        [Fact]
        public void Serialize_ExtremeValues_HandlesCorrectly()
        {
            var serializer = GetSerializer();
            // Test with very large counts and speeds
            var original = new RingPattern(1000, 10000f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_InfiniteLoopPattern_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var basePattern = new SpiralPattern(12, 1f, 180f, 360f, 0f, true);
            var original = new LoopPattern(basePattern);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            Assert.Equal(original.IsLooping, deserialized.IsLooping);
            Assert.Equal(original.Duration, deserialized.Duration);
        }

        [Fact]
        public void Serialize_ZeroDelayBurst_RoundTrip_ProducesIdenticalBehavior()
        {
            var serializer = GetSerializer();
            var original = new BurstPattern(5, new Vector2(1, 0), 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        #endregion

        #region Error Handling

        [Fact]
        public void Deserialize_InvalidJson_ThrowsException()
        {
            var serializer = GetSerializer();
            var invalidJson = "{ invalid json }";

            Assert.Throws<JsonException>(() => serializer.Deserialize(invalidJson));
        }

        [Fact]
        public void Deserialize_MissingTypeField_ThrowsException()
        {
            var serializer = GetSerializer();
            var json = "{\"bulletCount\":8,\"speed\":150.0}";

            Assert.Throws<ArgumentException>(() => serializer.Deserialize(json));
        }

        [Fact]
        public void Deserialize_UnknownPatternType_ThrowsException()
        {
            var serializer = GetSerializer();
            var json = "{\"type\":\"UnknownPatternType\",\"bulletCount\":8}";

            Assert.Throws<ArgumentException>(() => serializer.Deserialize(json));
        }

        [Fact]
        public void Deserialize_MissingRequiredFields_ThrowsException()
        {
            var serializer = GetSerializer();
            var json = "{\"type\":\"RingPattern\",\"bulletCount\":8}"; // Missing speed

            Assert.Throws<ArgumentException>(() => serializer.Deserialize(json));
        }

        [Fact]
        public void Deserialize_WrongFieldTypes_ThrowsException()
        {
            var serializer = GetSerializer();
            var json = "{\"type\":\"RingPattern\",\"bulletCount\":\"not a number\",\"speed\":150.0}";

            Assert.Throws<JsonException>(() => serializer.Deserialize(json));
        }

        [Fact]
        public void Deserialize_InvalidPatternValues_ThrowsException()
        {
            var serializer = GetSerializer();
            var json = "{\"type\":\"RingPattern\",\"bulletCount\":-1,\"speed\":150.0}"; // Negative count

            Assert.Throws<ArgumentException>(() => serializer.Deserialize(json));
        }

        #endregion

        #region Stream I/O Tests

        [Fact]
        public void Serialize_ToStream_WritesValidJson()
        {
            var serializer = GetSerializer();
            var pattern = new RingPattern(8, 150f, 45f);
            using var stream = new MemoryStream();

            serializer.Serialize(pattern, stream);

            stream.Position = 0;
            var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();

            Assert.NotEmpty(json);
            // Verify it's valid JSON
            JsonDocument.Parse(json);
        }

        [Fact]
        public void Deserialize_FromStream_ProducesIdenticalPattern()
        {
            var serializer = GetSerializer();
            var original = new RingPattern(8, 150f, 45f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            using var stream = new MemoryStream();
            serializer.Serialize(original, stream);
            stream.Position = 0;

            var deserialized = serializer.Deserialize(stream);

            AssertPatternsEquivalent(original, deserialized, context);
        }

        [Fact]
        public void Serialize_ToFile_CanLoadFromFile()
        {
            var serializer = GetSerializer();
            var original = new RingPattern(8, 150f, 45f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);
            var tempFile = Path.GetTempFileName();

            try
            {
                using (var stream = File.Create(tempFile))
                {
                    serializer.Serialize(original, stream);
                }

                using (var stream = File.OpenRead(tempFile))
                {
                    var deserialized = serializer.Deserialize(stream);
                    AssertPatternsEquivalent(original, deserialized, context);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void Serialize_ComplexPattern_PerformsWithinTimeLimit()
        {
            var serializer = GetSerializer();
            // Create a complex pattern with many nested levels
            var patterns = new List<IBulletPattern>();
            for (int i = 0; i < 50; i++)
            {
                patterns.Add(new RingPattern(8, 150f + i, i * 10f));
            }
            var sequence = new SequencePattern(patterns, looping: false);
            var parallel = new ParallelPattern(patterns);
            var complex = new RotatingPattern(new SequencePattern(new IBulletPattern[] { sequence, parallel }, looping: false), 90f);

            var startTime = DateTime.UtcNow;
            var json = serializer.Serialize(complex);
            var serializeTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Should complete in under 10ms for typical patterns
            Assert.True(serializeTime < 100, $"Serialization took {serializeTime}ms, expected < 100ms");
        }

        [Fact]
        public void Deserialize_ComplexPattern_PerformsWithinTimeLimit()
        {
            var serializer = GetSerializer();
            // Create a complex pattern
            var patterns = new List<IBulletPattern>();
            for (int i = 0; i < 50; i++)
            {
                patterns.Add(new RingPattern(8, 150f + i, i * 10f));
            }
            var sequence = new SequencePattern(patterns, looping: false);
            var complex = new RotatingPattern(sequence, 90f);

            var json = serializer.Serialize(complex);

            var startTime = DateTime.UtcNow;
            var deserialized = serializer.Deserialize(json);
            var deserializeTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Should complete in under 10ms for typical patterns
            Assert.True(deserializeTime < 100, $"Deserialization took {deserializeTime}ms, expected < 100ms");
        }

        #endregion

        #region Property Preservation Tests

        [Fact]
        public void Serialize_RingPattern_PreservesAllProperties()
        {
            var serializer = GetSerializer();
            var original = new RingPattern(12, 200f, 90f);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json) as RingPattern;

            Assert.NotNull(deserialized);
            Assert.Equal(original.BulletCount, deserialized.BulletCount);
            Assert.Equal(original.Speed, deserialized.Speed);
            Assert.Equal(original.StartAngle, deserialized.StartAngle);
        }

        [Fact]
        public void Serialize_BurstPattern_PreservesAllProperties()
        {
            var serializer = GetSerializer();
            var original = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json) as BurstPattern;

            Assert.NotNull(deserialized);
            Assert.Equal(original.BulletCount, deserialized.BulletCount);
            Assert.Equal(original.Speed, deserialized.Speed);
            Assert.Equal(original.DelayBetweenShots, deserialized.DelayBetweenShots);
            // Direction should be normalized, so compare normalized versions
            var originalDir = Vector2.Normalize(original.Direction);
            var deserializedDir = Vector2.Normalize(deserialized.Direction);
            Assert.True(Vector2.Distance(originalDir, deserializedDir) < 0.001f);
        }

        [Fact]
        public void Serialize_SequencePattern_PreservesAllProperties()
        {
            var serializer = GetSerializer();
            var pattern1 = new RingPattern(8, 150f);
            var pattern2 = new SpreadPattern(5, 45f, 200f);
            var original = new SequencePattern(new IBulletPattern[] { pattern1, pattern2 }, looping: true);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json) as SequencePattern;

            Assert.NotNull(deserialized);
            Assert.Equal(original.IsLooping, deserialized.IsLooping);
            Assert.Equal(original.Patterns.Count, deserialized.Patterns.Count);
        }

        [Fact]
        public void Serialize_RotatingPattern_PreservesAllProperties()
        {
            var serializer = GetSerializer();
            var basePattern = new RingPattern(8, 150f);
            var original = new RotatingPattern(basePattern, 90f);

            var json = serializer.Serialize(original);
            var deserialized = serializer.Deserialize(json) as RotatingPattern;

            Assert.NotNull(deserialized);
            Assert.Equal(original.RotationSpeed, deserialized.RotationSpeed);
            Assert.NotNull(deserialized.Pattern);
        }

        #endregion

        #region JSON Format Tests

        [Fact]
        public void Serialize_ProducesValidJson()
        {
            var serializer = GetSerializer();
            var pattern = new RingPattern(8, 150f, 45f);

            var json = serializer.Serialize(pattern);

            // Should be valid JSON
            var document = JsonDocument.Parse(json);
            Assert.NotNull(document);
        }

        [Fact]
        public void Serialize_ProducesHumanReadableJson()
        {
            var serializer = GetSerializer();
            var pattern = new RingPattern(8, 150f, 45f);

            var json = serializer.Serialize(pattern);

            // Should contain type discriminator
            Assert.Contains("type", json);
            Assert.Contains("RingPattern", json);
            // Should contain pattern properties
            Assert.Contains("bulletCount", json);
            Assert.Contains("speed", json);
        }

        [Fact]
        public void Serialize_CompositePattern_IncludesNestedPatterns()
        {
            var serializer = GetSerializer();
            var pattern1 = new RingPattern(8, 150f);
            var pattern2 = new SpreadPattern(5, 45f, 200f);
            var sequence = new SequencePattern(new IBulletPattern[] { pattern1, pattern2 }, looping: false);

            var json = serializer.Serialize(sequence);

            // Should contain patterns array
            Assert.Contains("patterns", json);
            // Should contain nested pattern types
            Assert.Contains("RingPattern", json);
            Assert.Contains("SpreadPattern", json);
        }

        #endregion
    }
}
