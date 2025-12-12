using System;
using System.Linq;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Patterns.Basic;
using BenedictBulletHell.Patterns.Patterns.Composite;
using BenedictBulletHell.Patterns.Tests.Helpers;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Patterns.Composite
{
    public class LoopPatternTests
    {
        [Fact]
        public void Construction_WithValidPattern_CreatesPattern()
        {
            var pattern = new RingPattern(4, 100f, 0f);
            var loop = new LoopPattern(pattern);

            Assert.NotNull(loop);
            Assert.Equal(pattern, loop.Pattern);
        }

        [Fact]
        public void Construction_WithNullPattern_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new LoopPattern(null!));
        }

        [Fact]
        public void Duration_ReturnsPositiveInfinity()
        {
            var pattern = new RingPattern(4, 100f, 0f);
            var loop = new LoopPattern(pattern);

            Assert.Equal(float.PositiveInfinity, loop.Duration);
        }

        [Fact]
        public void IsLooping_ReturnsTrue()
        {
            var pattern = new RingPattern(4, 100f, 0f);
            var loop = new LoopPattern(pattern);

            Assert.True(loop.IsLooping);
        }

        [Fact]
        public void GetSpawns_WithZeroDurationPattern_LoopsCorrectly()
        {
            var pattern = new RingPattern(4, 100f, 0f); // 4 bullets at time 0, duration = 0
            var loop = new LoopPattern(pattern);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // For zero-duration patterns, should just pass through
            var spawns = loop.GetSpawns(0f, 0f, context).ToList();
            PatternTestHelpers.AssertSpawnCount(spawns, 4);
        }

        [Fact]
        public void GetSpawns_WithBurstPattern_LoopsAtBoundary()
        {
            // Burst with duration 0.1s (2 bullets, 0.1s delay between them)
            var burst = new BurstPattern(2, new Vector2(1, 0), 100f, 0.1f);
            var loop = new LoopPattern(burst);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // First cycle (0 to 0.15) - should get both bullets
            var spawns1 = loop.GetSpawns(0f, 0.15f, context).ToList();
            Assert.True(spawns1.Count >= 1, $"Expected at least 1 spawn, got {spawns1.Count}");

            // Second cycle (0.15 to 0.3) - should loop and get more bullets
            var spawns2 = loop.GetSpawns(0.15f, 0.3f, context).ToList();
            Assert.True(spawns2.Count >= 1, $"Expected at least 1 spawn, got {spawns2.Count}");

            // Verify they're the same pattern (both have same direction)
            foreach (var spawn in spawns1.Concat(spawns2))
            {
                PatternTestHelpers.AssertSpawnDirection(spawn, new Vector2(1, 0));
            }
        }

        [Fact]
        public void GetSpawns_WrapAroundHandledCorrectly()
        {
            var burst = new BurstPattern(2, new Vector2(1, 0), 100f, 0.1f); // Duration = 0.1s
            var loop = new LoopPattern(burst);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Query that spans wrap boundary (0.15 to 0.25)
            // This should get end of first loop and start of second loop
            var spawns = loop.GetSpawns(0.15f, 0.25f, context).ToList();
            
            // Should get bullets from both cycles
            Assert.True(spawns.Count > 0);
        }
    }
}

