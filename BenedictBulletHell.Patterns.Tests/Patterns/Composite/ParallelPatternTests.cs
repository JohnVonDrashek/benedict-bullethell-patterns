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
    public class ParallelPatternTests
    {
        [Fact]
        public void Construction_WithValidPatterns_CreatesPattern()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f);
            var pattern2 = new SingleShotPattern(new Vector2(0, 1), 100f);
            IBulletPattern[] patterns = new IBulletPattern[] { pattern1, pattern2 };
            var parallel = new ParallelPattern(patterns);

            Assert.NotNull(parallel);
            Assert.Equal(2, parallel.Patterns.Count);
        }

        [Fact]
        public void Construction_WithNullPatterns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new ParallelPattern(null!));
        }

        [Fact]
        public void Construction_WithEmptyPatterns_ThrowsArgumentException()
        {
            IBulletPattern[] empty = new IBulletPattern[0];
            Assert.Throws<ArgumentException>(() => 
                new ParallelPattern(empty));
        }

        [Fact]
        public void Duration_EqualsMaxOfPatternDurations()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f); // Duration = 0
            var pattern2 = new BurstPattern(3, new Vector2(1, 0), 100f, 0.1f); // Duration = 0.2
            var parallel = new ParallelPattern(new IBulletPattern[] { pattern1, pattern2 });

            Assert.Equal(0.2f, parallel.Duration); // Max of 0 and 0.2
        }

        [Fact]
        public void IsLooping_TrueIfAnyPatternLoops()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f); // Not looping
            var pattern2 = new LoopPattern(new RingPattern(4, 100f, 0f)); // Looping
            IBulletPattern[] patterns = new IBulletPattern[] { pattern1, pattern2 };
            var parallel = new ParallelPattern(patterns);

            Assert.True(parallel.IsLooping);
        }

        [Fact]
        public void IsLooping_FalseIfNoPatternsLoop()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f);
            var pattern2 = new RingPattern(4, 100f, 0f);
            IBulletPattern[] patterns = new IBulletPattern[] { pattern1, pattern2 };
            var parallel = new ParallelPattern(patterns);

            Assert.False(parallel.IsLooping);
        }

        [Fact]
        public void GetSpawns_AllPatternsExecuteSimultaneously()
        {
            var pattern1 = new RingPattern(4, 100f, 0f); // 4 bullets
            var pattern2 = new RingPattern(4, 100f, 0f); // 4 bullets
            var parallel = new ParallelPattern(new IBulletPattern[] { pattern1, pattern2 });
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = parallel.GetSpawns(0f, 0f, context).ToList();

            // Should get all 8 bullets (4 + 4)
            PatternTestHelpers.AssertSpawnCount(spawns, 8);
        }

        [Fact]
        public void GetSpawns_AllSpawnsIncluded()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f);
            var pattern2 = new SingleShotPattern(new Vector2(0, 1), 100f);
            var pattern3 = new SingleShotPattern(new Vector2(-1, 0), 100f);
            IBulletPattern[] patterns = new IBulletPattern[] { pattern1, pattern2, pattern3 };
            var parallel = new ParallelPattern(patterns);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = parallel.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 3);
        }

        [Fact]
        public void GetSpawns_SinglePattern_WorksCorrectly()
        {
            var pattern = new RingPattern(4, 100f, 0f);
            IBulletPattern[] patterns = new IBulletPattern[] { pattern };
            var parallel = new ParallelPattern(patterns);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            var spawns = parallel.GetSpawns(0f, 0f, context).ToList();

            PatternTestHelpers.AssertSpawnCount(spawns, 4);
        }

        [Fact]
        public void GetSpawns_PatternsWithDifferentDurations_AllSpawn()
        {
            var pattern1 = new SingleShotPattern(new Vector2(1, 0), 100f); // Instant
            var pattern2 = new BurstPattern(2, new Vector2(0, 1), 100f, 0.1f); // 0.1s duration
            IBulletPattern[] patterns = new IBulletPattern[] { pattern1, pattern2 };
            var parallel = new ParallelPattern(patterns);
            var context = PatternTestHelpers.CreateContext(Vector2.Zero);

            // Query at time 0
            var spawns1 = parallel.GetSpawns(0f, 0f, context).ToList();
            Assert.Equal(2, spawns1.Count); // 1 from pattern1, 1 from pattern2

            // Query later to get second bullet from burst
            var spawns2 = parallel.GetSpawns(0.05f, 0.15f, context).ToList();
            Assert.Equal(1, spawns2.Count); // Second bullet from pattern2
        }
    }
}

