using System.Numerics;
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Patterns.Basic;
using BenedictBulletHell.Patterns.Patterns.Advanced;
using BenedictBulletHell.Patterns.Patterns.Composite;
using BenedictBulletHell.Patterns.Patterns.Modifiers;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Factory
{
    public class PatternFactoryTests
    {
        [Fact]
        public void Single_CreatesSingleShotPattern()
        {
            var pattern = Pattern.Single(new Vector2(1, 0), 100f);

            Assert.IsType<SingleShotPattern>(pattern);
        }

        [Fact]
        public void Burst_CreatesBurstPattern()
        {
            var pattern = Pattern.Burst(5, new Vector2(1, 0), 100f, 0.1f);

            Assert.IsType<BurstPattern>(pattern);
        }

        [Fact]
        public void Spread_CreatesSpreadPattern()
        {
            var pattern = Pattern.Spread(5, 45f, 100f, 0f);

            Assert.IsType<SpreadPattern>(pattern);
        }

        [Fact]
        public void Ring_CreatesRingPattern()
        {
            var pattern = Pattern.Ring(8, 100f, 0f);

            Assert.IsType<RingPattern>(pattern);
        }

        [Fact]
        public void Sequence_CreatesSequencePattern()
        {
            var pattern1 = Pattern.Single(new Vector2(1, 0), 100f);
            var pattern2 = Pattern.Single(new Vector2(0, 1), 100f);
            var sequence = Pattern.Sequence(pattern1, pattern2);

            Assert.IsType<SequencePattern>(sequence);
        }

        [Fact]
        public void Sequence_WithLooping_CreatesLoopingSequencePattern()
        {
            var pattern = Pattern.Single(new Vector2(1, 0), 100f);
            var sequence = Pattern.Sequence(looping: true, pattern);

            Assert.IsType<SequencePattern>(sequence);
            Assert.True(sequence.IsLooping);
        }

        [Fact]
        public void Parallel_CreatesParallelPattern()
        {
            var pattern1 = Pattern.Single(new Vector2(1, 0), 100f);
            var pattern2 = Pattern.Single(new Vector2(0, 1), 100f);
            var parallel = Pattern.Parallel(pattern1, pattern2);

            Assert.IsType<ParallelPattern>(parallel);
        }

        [Fact]
        public void Loop_CreatesLoopPattern()
        {
            var pattern = Pattern.Single(new Vector2(1, 0), 100f);
            var loop = Pattern.Loop(pattern);

            Assert.IsType<LoopPattern>(loop);
        }

        [Fact]
        public void Repeat_CreatesRepeatPattern()
        {
            var pattern = Pattern.Single(new Vector2(1, 0), 100f);
            var repeat = Pattern.Repeat(pattern, 3, 0.1f);

            Assert.IsType<RepeatPattern>(repeat);
        }

        [Fact]
        public void Spiral_CreatesSpiralPattern()
        {
            var pattern = Pattern.Spiral(12, 3, 150f, 360f, 0f, false);

            Assert.IsType<SpiralPattern>(pattern);
        }

        [Fact]
        public void Aimed_CreatesAimedPattern()
        {
            var pattern = Pattern.Aimed(1, 200f, 0f);

            Assert.IsType<AimedPattern>(pattern);
        }

        [Fact]
        public void Wave_CreatesWavePattern()
        {
            var pattern = Pattern.Wave(10, 90f, 30f, 2f, 180f);

            Assert.IsType<WavePattern>(pattern);
        }

        [Fact]
        public void Rotating_CreatesRotatingPattern()
        {
            var pattern = Pattern.Spread(5, 30f, 180f);
            var rotating = Pattern.Rotating(pattern, 90f);

            Assert.IsType<RotatingPattern>(rotating);
        }

        [Fact]
        public void FactoryMethods_PassParametersCorrectly()
        {
            // Test that parameters are passed through correctly
            var burst = Pattern.Burst(7, new Vector2(1, 0), 150f, 0.2f);
            var burstPattern = Assert.IsType<BurstPattern>(burst);
            
            Assert.Equal(7, burstPattern.BulletCount);
            Assert.Equal(150f, burstPattern.Speed);
            Assert.Equal(0.2f, burstPattern.DelayBetweenShots);
        }
    }
}


