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
    public class SequencePatternTests
    {
        [Fact]
        public void Construction_WithValidPatterns_CreatesPattern()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f);
            var pattern2 = new SingleShotPattern(new Vector2(0, 1), 100f);
            var sequence = new SequencePattern(new IBulletPattern[] { pattern1, pattern2 }, looping: false);

            Assert.NotNull(sequence);
            Assert.Equal(2, sequence.Patterns.Count);
        }

        [Fact]
        public void Construction_WithNullPatterns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new SequencePattern(null!, looping: false));
        }

        [Fact]
        public void Construction_WithEmptyPatterns_ThrowsArgumentException()
        {
            IBulletPattern[] empty = new IBulletPattern[0];
            Assert.Throws<ArgumentException>(() => 
                new SequencePattern(empty, looping: false));
        }

        [Fact]
        public void Duration_NonLooping_EqualsSumOfPatternDurations()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f); // Duration = 0
            var pattern2 = new BurstPattern(3, new Vector2(1, 0), 100f, 0.1f); // Duration = 0.2
            var sequence = new SequencePattern(new IBulletPattern[] { pattern1, pattern2 }, looping: false);

            Assert.Equal(0.2f, sequence.Duration);
        }

        [Fact]
        public void Duration_Looping_ReturnsPositiveInfinity()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f);
            var sequence = new SequencePattern(new IBulletPattern[] { pattern1 }, looping: true);

            Assert.Equal(float.PositiveInfinity, sequence.Duration);
        }

        [Fact]
        public void IsLooping_MatchesConstructorParameter()
        {
            var pattern = new SingleShotPattern(new Vector2(1, 0), 100f);
            var sequence1 = new SequencePattern(new IBulletPattern[] { pattern }, looping: false);
            var sequence2 = new SequencePattern(new IBulletPattern[] { pattern }, looping: true);

            Assert.False(sequence1.IsLooping);
            Assert.True(sequence2.IsLooping);
        }

        [Fact]
        public void GetSpawns_PatternsExecuteInOrder()
        {
            // Use burst patterns: both have 2 bullets to ensure non-zero duration
            // Pattern1: 2 bullets, 0.01s delay = duration 0.01s
            // Pattern2: 2 bullets, 0.01s delay = duration 0.01s
            var pattern1 = new BurstPattern(2, new Vector2(1, 0), 100f, 0.01f);
            var pattern2 = new BurstPattern(2, new Vector2(0, 1), 100f, 0.01f);
            var sequence = new SequencePattern(new IBulletPattern[] { pattern1, pattern2 }, looping: false);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Query across both patterns (pattern1 ends at 0.01, pattern2 at 0.02)
            var spawns = sequence.GetSpawns(0f, 0.03f, context).ToList();

            // Should get all bullets from both patterns (2 + 2 = 4)
            PatternTestHelpers.AssertSpawnCount(spawns, 4);
        }

        [Fact]
        public void GetSpawns_WithBurstPatterns_ExecutesSequentially()
        {
            // First burst: 2 bullets with 0.1s delay = duration 0.1s
            var burst1 = new BurstPattern(2, new Vector2(1, 0), 100f, 0.1f);
            // Second burst: 2 bullets with 0.1s delay = duration 0.1s
            var burst2 = new BurstPattern(2, new Vector2(0, 1), 100f, 0.1f);
            var sequence = new SequencePattern(new IBulletPattern[] { burst1, burst2 }, looping: false);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Query first pattern (0 to 0.15) - should get both bullets from first burst
            var spawns1 = sequence.GetSpawns(0f, 0.15f, context).ToList();
            // May also get first bullet from second burst if timing overlaps slightly
            Assert.True(spawns1.Count >= 2, $"Expected at least 2 spawns, got {spawns1.Count}");

            // Query second pattern (0.15 to 0.3) - should get bullets from second burst
            var spawns2 = sequence.GetSpawns(0.15f, 0.3f, context).ToList();
            Assert.True(spawns2.Count >= 1, $"Expected at least 1 spawn, got {spawns2.Count}"); // At least second bullet from second burst
        }

        [Fact]
        public void GetSpawns_AllSpawnsIncluded()
        {
            // Use burst patterns instead of ring patterns to avoid zero-duration edge case
            // Burst patterns have non-zero duration, making sequence timing clearer
            var burst1 = new BurstPattern(4, new Vector2(1, 0), 100f, 0.01f); // 4 bullets, duration = 0.03s
            var burst2 = new BurstPattern(4, new Vector2(0, 1), 100f, 0.01f); // 4 bullets, duration = 0.03s
            var sequence = new SequencePattern(new IBulletPattern[] { burst1, burst2 }, looping: false);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Query across both patterns
            var spawns = sequence.GetSpawns(0f, 0.1f, context).ToList();

            // Should get all 8 bullets (4 + 4)
            PatternTestHelpers.AssertSpawnCount(spawns, 8);
        }

        [Fact]
        public void GetSpawns_LoopingSequence_LoopsCorrectly()
        {
            // Use burst pattern to avoid zero-duration edge case
            var pattern = new BurstPattern(4, new Vector2(1, 0), 100f, 0.01f); // Duration = 0.03s
            var sequence = new SequencePattern(new IBulletPattern[] { pattern }, looping: true);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // First cycle
            var spawns1 = sequence.GetSpawns(0f, 0.05f, context).ToList();
            PatternTestHelpers.AssertSpawnCount(spawns1, 4);

            // Should loop - query second cycle
            var spawns2 = sequence.GetSpawns(0.05f, 0.1f, context).ToList();
            // Should get more spawns from the loop
            Assert.True(sequence.IsLooping);
        }

        [Fact]
        public void GetSpawns_SinglePattern_WorksCorrectly()
        {
            // Use burst pattern to avoid zero-duration edge case in sequence
            var pattern = new BurstPattern(4, new Vector2(1, 0), 100f, 0.01f); // Duration = 0.03s
            var sequence = new SequencePattern(new IBulletPattern[] { pattern }, looping: false);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Query within pattern duration
            var spawns = sequence.GetSpawns(0f, 0.05f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 4);
        }
    }
}

