using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Helpers
{
    public static class PatternTestHelpers
    {
        /// <summary>
        /// Creates a standard test context.
        /// </summary>
        public static PatternContext CreateContext(Vector2 origin, Vector2? target = null)
        {
            return new PatternContext
            {
                Origin = origin,
                Target = target,
                PatternAge = 0f,
                Metadata = null
            };
        }

        /// <summary>
        /// Asserts the spawn count matches expected.
        /// </summary>
        public static void AssertSpawnCount(IEnumerable<BulletSpawn> spawns, int expected)
        {
            var list = spawns.ToList();
            Assert.Equal(expected, list.Count);
        }

        /// <summary>
        /// Asserts spawn direction matches expected (with tolerance for floating point).
        /// </summary>
        public static void AssertSpawnDirection(BulletSpawn spawn, Vector2 expected, float tolerance = 0.001f)
        {
            var diff = Vector2.Distance(spawn.Direction, expected);
            Assert.True(diff < tolerance, $"Direction mismatch. Expected: {expected}, Actual: {spawn.Direction}, Diff: {diff}");
        }

        /// <summary>
        /// Asserts spawn position matches expected.
        /// </summary>
        public static void AssertSpawnPosition(BulletSpawn spawn, Vector2 expected, float tolerance = 0.001f)
        {
            var diff = Vector2.Distance(spawn.Position, expected);
            Assert.True(diff < tolerance, $"Position mismatch. Expected: {expected}, Actual: {spawn.Position}, Diff: {diff}");
        }

        /// <summary>
        /// Asserts spawn speed matches expected.
        /// </summary>
        public static void AssertSpawnSpeed(BulletSpawn spawn, float expected, float tolerance = 0.001f)
        {
            Assert.Equal(expected, spawn.Speed, tolerance);
        }

        /// <summary>
        /// Asserts spawn angle matches expected (handles angle wrapping).
        /// </summary>
        public static void AssertSpawnAngle(BulletSpawn spawn, float expected, float tolerance = 0.1f)
        {
            // Normalize angles to 0-360 range
            float normalizedActual = NormalizeAngle(spawn.Angle);
            float normalizedExpected = NormalizeAngle(expected);
            
            float diff = Math.Abs(normalizedActual - normalizedExpected);
            // Handle wrap-around (e.g., 359° and 1° are close)
            if (diff > 180f) diff = 360f - diff;
            
            Assert.True(diff < tolerance, $"Angle mismatch. Expected: {expected}° ({normalizedExpected}°), Actual: {spawn.Angle}° ({normalizedActual}°), Diff: {diff}°");
        }

        /// <summary>
        /// Asserts no duplicate spawns exist.
        /// </summary>
        public static void AssertNoDuplicates(IEnumerable<BulletSpawn> spawns)
        {
            var list = spawns.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    var s1 = list[i];
                    var s2 = list[j];
                    
                    // Consider duplicates if position and direction are very close
                    bool positionsMatch = Vector2.Distance(s1.Position, s2.Position) < 0.001f;
                    bool directionsMatch = Vector2.Distance(s1.Direction, s2.Direction) < 0.001f;
                    bool speedsMatch = Math.Abs(s1.Speed - s2.Speed) < 0.001f;
                    
                    Assert.False(positionsMatch && directionsMatch && speedsMatch, 
                        $"Duplicate spawn found at index {i} and {j}");
                }
            }
        }

        /// <summary>
        /// Asserts angle values are approximately equal (handles wrapping).
        /// </summary>
        public static void AssertAngleApproximatelyEqual(float angle1, float angle2, float tolerance = 0.1f)
        {
            float normalized1 = NormalizeAngle(angle1);
            float normalized2 = NormalizeAngle(angle2);
            
            float diff = Math.Abs(normalized1 - normalized2);
            if (diff > 180f) diff = 360f - diff;
            
            Assert.True(diff < tolerance, $"Angles not approximately equal. Angle1: {angle1}° ({normalized1}°), Angle2: {angle2}° ({normalized2}°), Diff: {diff}°");
        }

        /// <summary>
        /// Normalizes an angle to 0-360 range.
        /// </summary>
        private static float NormalizeAngle(float angle)
        {
            angle = angle % 360f;
            if (angle < 0f) angle += 360f;
            return angle;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static float ToRadians(float degrees)
        {
            return (float)(degrees * Math.PI / 180.0);
        }

        /// <summary>
        /// Creates a direction vector from an angle in degrees.
        /// </summary>
        public static Vector2 DirectionFromAngle(float angleDegrees)
        {
            float angleRad = ToRadians(angleDegrees);
            return new Vector2((float)Math.Cos(angleRad), (float)Math.Sin(angleRad));
        }

        /// <summary>
        /// Gets angle in degrees from a direction vector.
        /// </summary>
        public static float AngleFromDirection(Vector2 direction)
        {
            return (float)(Math.Atan2(direction.Y, direction.X) * 180.0 / Math.PI);
        }
    }
}

