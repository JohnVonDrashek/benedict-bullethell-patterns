using System;
using System.Linq;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Patterns.Basic;
using BenedictBulletHell.Patterns.Tests.Helpers;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Patterns.Basic
{
    public class BurstPatternTests
    {
        [Fact]
        public void Construction_WithValidParameters_CreatesPattern()
        {
            var pattern = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);

            Assert.NotNull(pattern);
            Assert.Equal(5, pattern.BulletCount);
            Assert.Equal(100f, pattern.Speed);
            Assert.Equal(0.1f, pattern.DelayBetweenShots);
        }

        [Fact]
        public void Construction_WithBulletCountLessThanOne_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                new BurstPattern(0, new Vector2(1, 0), 100f, 0.1f));
        }

        [Fact]
        public void Construction_WithNegativeDelay_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                new BurstPattern(5, new Vector2(1, 0), 100f, -0.1f));
        }

        [Fact]
        public void Construction_NormalizesDirection()
        {
            var direction = new Vector2(3, 4);
            var pattern = new BurstPattern(3, direction, 100f, 0.1f);

            var normalized = Vector2.Normalize(direction);
            var diff = Vector2.Distance(pattern.Direction, normalized);
            Assert.True(diff < 0.001f);
        }

        [Fact]
        public void Duration_CalculatedCorrectly()
        {
            var pattern = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);

            // Duration = (count - 1) * delay = (5 - 1) * 0.1 = 0.4
            Assert.Equal(0.4f, pattern.Duration);
        }

        [Fact]
        public void Duration_SingleBullet_ReturnsZero()
        {
            var pattern = new BurstPattern(1, new Vector2(1, 0), 100f, 0.1f);

            Assert.Equal(0f, pattern.Duration);
        }

        [Fact]
        public void IsLooping_ReturnsFalse()
        {
            var pattern = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);

            Assert.False(pattern.IsLooping);
        }

        [Fact]
        public void GetSpawns_SpawnsCorrectNumberOfBullets()
        {
            var pattern = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 1f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 5);
        }

        [Fact]
        public void GetSpawns_FirstBulletSpawnsAtTimeZero()
        {
            var pattern = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0.05f, context).ToList();

            // Should get first bullet only
            Assert.True(spawns.Count >= 1);
            PatternTestHelpers.AssertSpawnPosition(spawns[0], Vector2.Zero);
        }

        [Fact]
        public void GetSpawns_BulletsSpawnAtCorrectIntervals()
        {
            var pattern = new BurstPattern(3, new Vector2(1, 0), 100f, 0.1f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Query first bullet
            var spawns1 = pattern.GetSpawns(0f, 0.05f, context).ToList();
            Assert.Equal(1, spawns1.Count);

            // Query second bullet
            var spawns2 = pattern.GetSpawns(0.05f, 0.15f, context).ToList();
            Assert.Equal(1, spawns2.Count);

            // Query third bullet
            var spawns3 = pattern.GetSpawns(0.15f, 0.25f, context).ToList();
            Assert.Equal(1, spawns3.Count);
        }

        [Fact]
        public void GetSpawns_AllBulletsHaveSameDirectionAndSpeed()
        {
            var direction = new Vector2(1, 0);
            var speed = 150f;
            var pattern = new BurstPattern(5, direction, speed, 0.1f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 1f, context).ToList();

            var normalizedDir = Vector2.Normalize(direction);
            foreach (var spawn in spawns)
            {
                PatternTestHelpers.AssertSpawnDirection(spawn, normalizedDir);
                PatternTestHelpers.AssertSpawnSpeed(spawn, speed);
            }
        }

        [Fact]
        public void GetSpawns_BeforeTimeZero_NoSpawns()
        {
            var pattern = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(-1f, -0.5f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 0);
        }

        [Fact]
        public void GetSpawns_AfterDuration_NoSpawns()
        {
            var pattern = new BurstPattern(3, new Vector2(1, 0), 100f, 0.1f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Duration is 0.2, query after that
            var spawns = pattern.GetSpawns(0.3f, 0.5f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 0);
        }

        [Fact]
        public void GetSpawns_ZeroDelay_AllBulletsAtTimeZero()
        {
            var pattern = new BurstPattern(5, new Vector2(1, 0), 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            // Should get all 5 bullets
            PatternTestHelpers.AssertSpawnCount(spawns, 5);
        }

        [Fact]
        public void GetSpawns_PartialTimeRange_OnlySpawnsInRange()
        {
            var pattern = new BurstPattern(5, new Vector2(1, 0), 100f, 0.1f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Query only middle bullets
            var spawns = pattern.GetSpawns(0.15f, 0.25f, context).ToList();

            // Should get 2 bullets (at 0.2 and possibly 0.3 if it's within range)
            Assert.True(spawns.Count >= 1);
        }
    }
}


