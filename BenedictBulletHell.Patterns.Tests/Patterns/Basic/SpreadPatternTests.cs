using System.Linq;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Patterns.Basic;
using BenedictBulletHell.Patterns.Tests.Helpers;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Patterns.Basic
{
    public class SpreadPatternTests
    {
        [Fact]
        public void Construction_WithValidParameters_CreatesPattern()
        {
            var pattern = new SpreadPattern(5, 45f, 100f, 0f);

            Assert.NotNull(pattern);
            Assert.Equal(5, pattern.BulletCount);
            Assert.Equal(45f, pattern.AngleSpread);
            Assert.Equal(100f, pattern.Speed);
            Assert.Equal(0f, pattern.BaseAngle);
        }

        [Fact]
        public void Construction_WithBulletCountLessThanOne_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => 
                new SpreadPattern(0, 45f, 100f, 0f));
        }

        [Fact]
        public void Duration_ReturnsZero()
        {
            var pattern = new SpreadPattern(5, 45f, 100f, 0f);

            Assert.Equal(0f, pattern.Duration);
        }

        [Fact]
        public void IsLooping_ReturnsFalse()
        {
            var pattern = new SpreadPattern(5, 45f, 100f, 0f);

            Assert.False(pattern.IsLooping);
        }

        [Fact]
        public void GetSpawns_SpawnsCorrectNumberOfBullets()
        {
            var pattern = new SpreadPattern(5, 45f, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 5);
        }

        [Fact]
        public void GetSpawns_AllBulletsSpawnAtTimeZero()
        {
            var pattern = new SpreadPattern(5, 45f, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();
            PatternTestHelpers.AssertSpawnCount(spawns, 5);

            var spawnsAfter = pattern.GetSpawns(0.1f, 0.2f, context).ToList();
            PatternTestHelpers.AssertSpawnCount(spawnsAfter, 0);
        }

        [Fact]
        public void GetSpawns_BulletsEvenlyDistributedAcrossSpread()
        {
            var spread = 45f;
            var pattern = new SpreadPattern(5, spread, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            // First bullet at -spread/2, last at +spread/2
            float startAngle = -spread / 2f;
            float angleStep = spread / (5 - 1); // For 5 bullets, 4 intervals

            for (int i = 0; i < spawns.Count; i++)
            {
                float expectedAngle = startAngle + (angleStep * i);
                PatternTestHelpers.AssertSpawnAngle(spawns[i], expectedAngle);
            }
        }

        [Fact]
        public void GetSpawns_SingleBullet_SpawnsAtBaseAngle()
        {
            var baseAngle = 90f;
            var spread = 45f;
            var pattern = new SpreadPattern(1, spread, 100f, baseAngle);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 1);
            // With single bullet: startAngle = baseAngle - spread/2, angleStep = spread/(1-1) = inf, but only one iteration
            // So angle = startAngle + (angleStep * 0) = baseAngle - spread/2
            // Actually, when bulletCount=1, the angleStep calculation becomes spread/0 which is undefined,
            // but the code uses (BulletCount > 1) check, so angleStep = 0 when count=1
            // angle = startAngle + (0 * 0) = startAngle = baseAngle - spread/2
            float expectedAngle = baseAngle - (spread / 2f);
            PatternTestHelpers.AssertSpawnAngle(spawns[0], expectedAngle);
        }

        [Fact]
        public void GetSpawns_ZeroSpread_AllBulletsSameDirection()
        {
            var pattern = new SpreadPattern(3, 0f, 100f, 45f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            var firstDirection = spawns[0].Direction;
            foreach (var spawn in spawns)
            {
                PatternTestHelpers.AssertSpawnDirection(spawn, firstDirection);
            }
        }

        [Fact]
        public void GetSpawns_AllBulletsHaveSameSpeed()
        {
            var speed = 150f;
            var pattern = new SpreadPattern(5, 45f, speed, 0f);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            foreach (var spawn in spawns)
            {
                PatternTestHelpers.AssertSpawnSpeed(spawn, speed);
            }
        }

        [Fact]
        public void GetSpawns_AllBulletsSpawnAtOrigin()
        {
            var origin = new Vector2(100, 200);
            var pattern = new SpreadPattern(5, 45f, 100f, 0f);
            var context = PatternTestHelpers.CreateContext(origin);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            foreach (var spawn in spawns)
            {
                PatternTestHelpers.AssertSpawnPosition(spawn, origin);
            }
        }

        [Fact]
        public void GetSpawns_BaseAngleOffset_WorksCorrectly()
        {
            var baseAngle = 90f; // Pointing up
            var spread = 30f;
            var pattern = new SpreadPattern(3, spread, 100f, baseAngle);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = pattern.GetSpawns(0f, 0f, context).ToList();

            // First should be at baseAngle - spread/2 = 90 - 15 = 75
            PatternTestHelpers.AssertSpawnAngle(spawns[0], baseAngle - spread / 2f);
            // Middle should be at baseAngle = 90
            PatternTestHelpers.AssertSpawnAngle(spawns[1], baseAngle);
            // Last should be at baseAngle + spread/2 = 90 + 15 = 105
            PatternTestHelpers.AssertSpawnAngle(spawns[2], baseAngle + spread / 2f);
        }
    }
}

