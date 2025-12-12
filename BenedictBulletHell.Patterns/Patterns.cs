using System.Collections.Generic;
using System.Numerics;
using BasicPatterns = BenedictBulletHell.Patterns.Patterns.Basic;
using CompositePatterns = BenedictBulletHell.Patterns.Patterns.Composite;
using AdvancedPatterns = BenedictBulletHell.Patterns.Patterns.Advanced;
using ModifierPatterns = BenedictBulletHell.Patterns.Patterns.Modifiers;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns
{
    /// <summary>
    /// Factory class for creating bullet patterns.
    /// Use: <c>using static BenedictBulletHell.Patterns.Pattern;</c> to call methods directly.
    /// Example: <c>var pattern = Pattern.Ring(8, 150f);</c>
    /// </summary>
    public static class Pattern
    {
        /// <summary>
        /// Creates a single shot pattern.
        /// </summary>
        /// <param name="direction">Direction vector (will be normalized).</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <returns>A single shot pattern.</returns>
        public static IBulletPattern Single(Vector2 direction, float speed)
        {
            return new BasicPatterns.SingleShotPattern(direction, speed);
        }

        /// <summary>
        /// Creates a burst pattern that fires multiple bullets in quick succession.
        /// </summary>
        /// <param name="count">Number of bullets to fire.</param>
        /// <param name="direction">Direction vector (will be normalized).</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="delay">Delay between each shot in seconds.</param>
        /// <returns>A burst pattern.</returns>
        public static IBulletPattern Burst(int count, Vector2 direction, float speed, float delay)
        {
            return new BasicPatterns.BurstPattern(count, direction, speed, delay);
        }

        /// <summary>
        /// Creates a spread pattern that fires bullets in a fan formation.
        /// </summary>
        /// <param name="count">Number of bullets to fire.</param>
        /// <param name="angleSpread">Total angle spread in degrees.</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="baseAngle">Center angle in degrees (0 = right, 90 = up).</param>
        /// <returns>A spread pattern.</returns>
        public static IBulletPattern Spread(int count, float angleSpread, float speed, float baseAngle = 0f)
        {
            return new BasicPatterns.SpreadPattern(count, angleSpread, speed, baseAngle);
        }

        /// <summary>
        /// Creates a ring pattern that fires bullets in a circle.
        /// </summary>
        /// <param name="count">Number of bullets to fire.</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="startAngle">Starting angle in degrees (0 = right, 90 = up).</param>
        /// <returns>A ring pattern.</returns>
        public static IBulletPattern Ring(int count, float speed, float startAngle = 0f)
        {
            return new BasicPatterns.RingPattern(count, speed, startAngle);
        }

        /// <summary>
        /// Creates a sequence pattern that executes patterns one after another.
        /// </summary>
        /// <param name="patterns">Patterns to execute in sequence.</param>
        /// <returns>A sequence pattern.</returns>
        public static IBulletPattern Sequence(params IBulletPattern[] patterns)
        {
            return new CompositePatterns.SequencePattern(patterns, looping: false);
        }

        /// <summary>
        /// Creates a sequence pattern that executes patterns one after another, optionally looping.
        /// </summary>
        /// <param name="looping">Whether to loop the sequence.</param>
        /// <param name="patterns">Patterns to execute in sequence.</param>
        /// <returns>A sequence pattern.</returns>
        public static IBulletPattern Sequence(bool looping, params IBulletPattern[] patterns)
        {
            return new CompositePatterns.SequencePattern(patterns, looping);
        }

        /// <summary>
        /// Creates a parallel pattern that executes multiple patterns simultaneously.
        /// </summary>
        /// <param name="patterns">Patterns to execute in parallel.</param>
        /// <returns>A parallel pattern.</returns>
        public static IBulletPattern Parallel(params IBulletPattern[] patterns)
        {
            return new CompositePatterns.ParallelPattern(patterns);
        }

        /// <summary>
        /// Creates a loop pattern that repeats a pattern indefinitely.
        /// </summary>
        /// <param name="pattern">The pattern to loop.</param>
        /// <returns>A loop pattern.</returns>
        public static IBulletPattern Loop(IBulletPattern pattern)
        {
            return new CompositePatterns.LoopPattern(pattern);
        }

        /// <summary>
        /// Creates a repeat pattern that repeats a pattern multiple times.
        /// </summary>
        /// <param name="pattern">The pattern to repeat.</param>
        /// <param name="count">Number of times to repeat.</param>
        /// <param name="delay">Delay between each repeat in seconds (default: 0).</param>
        /// <returns>A repeat pattern.</returns>
        public static IBulletPattern Repeat(IBulletPattern pattern, int count, float delay = 0f)
        {
            return new CompositePatterns.RepeatPattern(pattern, count, delay);
        }

        /// <summary>
        /// Creates a spiral pattern that fires bullets in a rotating spiral.
        /// </summary>
        /// <param name="bulletsPerRevolution">Number of bullets per full revolution.</param>
        /// <param name="totalRevolutions">Total number of revolutions.</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="rotationSpeed">Rotation speed in degrees per second (default: 360).</param>
        /// <param name="startAngle">Starting angle in degrees (default: 0).</param>
        /// <param name="looping">Whether the spiral loops indefinitely (default: false).</param>
        /// <returns>A spiral pattern.</returns>
        public static IBulletPattern Spiral(
            int bulletsPerRevolution,
            float totalRevolutions,
            float speed,
            float rotationSpeed = 360f,
            float startAngle = 0f,
            bool looping = false)
        {
            return new AdvancedPatterns.SpiralPattern(
                bulletsPerRevolution,
                totalRevolutions,
                speed,
                rotationSpeed,
                startAngle,
                looping);
        }

        /// <summary>
        /// Creates an aimed pattern that fires bullet(s) toward a target position.
        /// </summary>
        /// <param name="count">Number of bullets to fire.</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="spreadAngle">Optional spread angle in degrees for multiple bullets (default: 0).</param>
        /// <returns>An aimed pattern.</returns>
        public static IBulletPattern Aimed(int count, float speed, float spreadAngle = 0f)
        {
            return new AdvancedPatterns.AimedPattern(count, speed, spreadAngle);
        }

        /// <summary>
        /// Creates a wave pattern that fires bullets in a sine-wave formation.
        /// </summary>
        /// <param name="bulletCount">Number of bullets to fire.</param>
        /// <param name="baseDirection">Base direction of wave travel in degrees.</param>
        /// <param name="waveAmplitude">Side-to-side amplitude in degrees.</param>
        /// <param name="waveFrequency">Frequency of the wave (complete cycles).</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <returns>A wave pattern.</returns>
        public static IBulletPattern Wave(
            int bulletCount,
            float baseDirection,
            float waveAmplitude,
            float waveFrequency,
            float speed)
        {
            return new AdvancedPatterns.WavePattern(
                bulletCount,
                baseDirection,
                waveAmplitude,
                waveFrequency,
                speed);
        }

        /// <summary>
        /// Creates a rotating pattern that rotates any pattern around the origin over time.
        /// </summary>
        /// <param name="pattern">The pattern to rotate.</param>
        /// <param name="degreesPerSecond">Rotation speed in degrees per second.</param>
        /// <returns>A rotating pattern.</returns>
        public static IBulletPattern Rotating(IBulletPattern pattern, float degreesPerSecond)
        {
            return new ModifierPatterns.RotatingPattern(pattern, degreesPerSecond);
        }
    }
}

