using System;
using System.Linq;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Patterns.Advanced;
using BenedictBulletHell.Patterns.Tests.Helpers;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Patterns.Advanced
{
    public class AimedPatternTests
    {
        [Fact]
        public void Construction_WithValidParameters_CreatesPattern()
        {
            var pattern = new AimedPattern(1, 100f, 0f);

            Assert.NotNull(pattern);
            Assert.Equal(1, pattern.BulletCount);
            Assert.Equal(100f, pattern.Speed);
            Assert.Equal(0f, pattern.SpreadAngle);
        }

        [Fact]
        public void Construction_WithBulletCountLessThanOne_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                new AimedPattern(0, 100f, 0f));
        }

        [Fact]
        public void Construction_WithNegativeSpreadAngle_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                new AimedPattern(1, 100f, -5f));
        }

        [Fact]
        public void Duration_ReturnsZero()
        {
            var pattern = new AimedPattern(1, 100f, 0f);

            Assert.Equal(0f, pattern.Duration);
        }

        [Fact]
        public void IsLooping_ReturnsFalse()
        {
            var pattern = new AimedPattern(1, 100f, 0f);

            Assert.False(pattern.IsLooping);
        }

        [Fact]
        public void GetSpawns_WithTarget_SpawnsCorrectNumberOfBullets()
        {
            var pattern = new AimedPattern(3, 100f, 0f);
            var origin = new Vector2(0, 0);
            var target = new Vector2(100, 0);
            var context = PatternTestHelpers.CreateContext(origin, target);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 3);
        }

        [Fact]
        public void GetSpawns_WithTarget_AimsAtTarget()
        {
            var pattern = new AimedPattern(1, 100f, 0f);
            var origin = new Vector2(0, 0);
            var target = new Vector2(100, 0); // Target to the right
            var context = PatternTestHelpers.CreateContext(origin, target);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            var expectedDirection = Vector2.Normalize(target - origin); // Should point right
            PatternTestHelpers.AssertSpawnDirection(spawns[0], expectedDirection);
            PatternTestHelpers.AssertSpawnAngle(spawns[0], 0f); // 0 degrees = right
        }

        [Fact]
        public void GetSpawns_WithTargetAndSpread_SpreadsAroundAimDirection()
        {
            var pattern = new AimedPattern(3, 100f, 30f); // 30 degree spread
            var origin = new Vector2(0, 0);
            var target = new Vector2(100, 0); // Target to the right (0 degrees)
            var context = PatternTestHelpers.CreateContext(origin, target);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            // First bullet should be at -15 degrees (left of center)
            PatternTestHelpers.AssertSpawnAngle(spawns[0], -15f, tolerance: 1f);
            // Middle bullet should be at 0 degrees (center/aim direction)
            PatternTestHelpers.AssertSpawnAngle(spawns[1], 0f, tolerance: 1f);
            // Last bullet should be at +15 degrees (right of center)
            PatternTestHelpers.AssertSpawnAngle(spawns[2], 15f, tolerance: 1f);
        }

        [Fact]
        public void GetSpawns_WithoutTarget_DefaultsToRightDirection()
        {
            var pattern = new AimedPattern(1, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero, null);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 1);
            var expectedDirection = new Vector2(1, 0); // Right
            PatternTestHelpers.AssertSpawnDirection(spawns[0], expectedDirection);
            PatternTestHelpers.AssertSpawnAngle(spawns[0], 0f);
        }

        [Fact]
        public void GetSpawns_TargetAtOrigin_HandlesGracefully()
        {
            var pattern = new AimedPattern(1, 100f, 0f);
            var origin = new Vector2(100, 100);
            var target = new Vector2(100, 100); // Same as origin
            var context = PatternTestHelpers.CreateContext(origin, target);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            // Should still spawn, but in default direction
            PatternTestHelpers.AssertSpawnCount(spawns, 1);
        }

        [Fact]
        public void GetSpawns_AllBulletsHaveSameSpeed()
        {
            var speed = 150f;
            var pattern = new AimedPattern(3, speed, 10f);
            var origin = new Vector2(0, 0);
            var target = new Vector2(100, 100);
            var context = PatternTestHelpers.CreateContext(origin, target);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            foreach (var spawn in spawns)
            {
                PatternTestHelpers.AssertSpawnSpeed(spawn, speed);
            }
        }

        [Fact]
        public void GetSpawns_AllBulletsSpawnAtOrigin()
        {
            var origin = new Vector2(50, 75);
            var pattern = new AimedPattern(3, 100f, 20f);
            var target = new Vector2(100, 100);
            var context = PatternTestHelpers.CreateContext(origin, target);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            foreach (var spawn in spawns)
            {
                PatternTestHelpers.AssertSpawnPosition(spawn, origin);
            }
        }

        [Fact]
        public void GetSpawns_SingleBulletNoSpread_AimsDirectlyAtTarget()
        {
            var pattern = new AimedPattern(1, 100f, 0f);
            var origin = new Vector2(0, 0);
            var target = new Vector2(100, 100); // 45 degrees up-right
            var context = PatternTestHelpers.CreateContext(origin, target);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            var expectedDirection = Vector2.Normalize(target - origin);
            PatternTestHelpers.AssertSpawnDirection(spawns[0], expectedDirection);
            PatternTestHelpers.AssertSpawnAngle(spawns[0], 45f, tolerance: 1f);
        }
    }
}


