using System.Linq;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Patterns.Basic;
using BenedictBulletHell.Patterns.Tests.Helpers;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Patterns.Basic
{
    public class RingPatternTests
    {
        [Fact]
        public void Construction_WithValidParameters_CreatesPattern()
        {
            var pattern = new RingPattern(8, 100f, 0f);

            Assert.NotNull(pattern);
            Assert.Equal(8, pattern.BulletCount);
            Assert.Equal(100f, pattern.Speed);
        }

        [Fact]
        public void Construction_WithBulletCountLessThanOne_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                new RingPattern(0, 100f, 0f));
        }

        [Fact]
        public void Duration_ReturnsZero()
        {
            var pattern = new RingPattern(8, 100f, 0f);

            Assert.Equal(0f, pattern.Duration);
        }

        [Fact]
        public void IsLooping_ReturnsFalse()
        {
            var pattern = new RingPattern(8, 100f, 0f);

            Assert.False(pattern.IsLooping);
        }

        [Fact]
        public void GetSpawns_SpawnsCorrectNumberOfBullets()
        {
            var pattern = new RingPattern(8, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 8);
        }

        [Fact]
        public void GetSpawns_AllBulletsSpawnAtTimeZero()
        {
            var pattern = new RingPattern(8, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();
            
            // All should spawn
            PatternTestHelpers.AssertSpawnCount(spawns, 8);

            // None should spawn after time 0
            var spawnsAfter = pattern.GetSpawns(0.1f, 0.2f, context).ToList();
            PatternTestHelpers.AssertSpawnCount(spawnsAfter, 0);
        }

        [Fact]
        public void GetSpawns_BulletsEvenlyDistributedIn360Degrees()
        {
            var pattern = new RingPattern(8, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            // Should be 45 degrees apart (360 / 8)
            float expectedAngleStep = 360f / 8f;
            for (int i = 0; i < spawns.Count; i++)
            {
                float expectedAngle = i * expectedAngleStep;
                PatternTestHelpers.AssertSpawnAngle(spawns[i], expectedAngle);
            }
        }

        [Fact]
        public void GetSpawns_FirstBulletAtStartAngle()
        {
            var startAngle = 90f; // Pointing up
            var pattern = new RingPattern(8, 100f, startAngle);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnAngle(spawns[0], startAngle);
        }

        [Fact]
        public void GetSpawns_AllDirectionsNormalized()
        {
            var pattern = new RingPattern(8, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            foreach (var spawn in spawns)
            {
                var length = spawn.Direction.Length();
                Assert.InRange(length, 0.99f, 1.01f); // Should be normalized (allowing floating point error)
            }
        }

        [Fact]
        public void GetSpawns_AllBulletsHaveSameSpeed()
        {
            var speed = 150f;
            var pattern = new RingPattern(8, speed, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            foreach (var spawn in spawns)
            {
                PatternTestHelpers.AssertSpawnSpeed(spawn, speed);
            }
        }

        [Fact]
        public void GetSpawns_SingleBullet_WorksCorrectly()
        {
            var pattern = new RingPattern(1, 100f, 45f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 1);
            PatternTestHelpers.AssertSpawnAngle(spawns[0], 45f);
        }

        [Fact]
        public void GetSpawns_TwoBullets_OppositeDirections()
        {
            var pattern = new RingPattern(2, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 2);
            PatternTestHelpers.AssertSpawnAngle(spawns[0], 0f);
            PatternTestHelpers.AssertSpawnAngle(spawns[1], 180f);
        }

        [Fact]
        public void GetSpawns_AllBulletsSpawnAtOrigin()
        {
            var origin = new Vector2(100, 200);
            var pattern = new RingPattern(8, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(origin);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            foreach (var spawn in spawns)
            {
                PatternTestHelpers.AssertSpawnPosition(spawn, origin);
            }
        }
    }
}


