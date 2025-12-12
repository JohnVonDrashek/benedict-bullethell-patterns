using System;
using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Advanced
{
    /// <summary>
    /// Creates bullets in a sine-wave pattern.
    /// </summary>
    public sealed class WavePattern : IBulletPattern
    {
        /// <summary>
        /// Number of bullets to fire.
        /// </summary>
        public int BulletCount { get; }

        /// <summary>
        /// Base direction of wave travel in degrees (0 = right, 90 = up).
        /// </summary>
        public float BaseDirection { get; }

        /// <summary>
        /// Side-to-side amplitude of the wave in degrees.
        /// </summary>
        public float WaveAmplitude { get; }

        /// <summary>
        /// Frequency of the wave (how many complete cycles across all bullets).
        /// </summary>
        public float WaveFrequency { get; }

        /// <summary>
        /// Speed of the bullets in units per second.
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// Duration is 0 (all bullets spawn simultaneously with different directions).
        /// </summary>
        public float Duration => 0f;

        /// <summary>
        /// Does not loop.
        /// </summary>
        public bool IsLooping => false;

        /// <summary>
        /// Creates a new wave pattern.
        /// </summary>
        /// <param name="bulletCount">Number of bullets to fire.</param>
        /// <param name="baseDirection">Base direction of wave travel in degrees.</param>
        /// <param name="waveAmplitude">Side-to-side amplitude in degrees.</param>
        /// <param name="waveFrequency">Frequency of the wave (complete cycles).</param>
        /// <param name="speed">Speed in units per second.</param>
        public WavePattern(
            int bulletCount,
            float baseDirection,
            float waveAmplitude,
            float waveFrequency,
            float speed)
        {
            if (bulletCount < 1)
                throw new ArgumentException("Bullet count must be at least 1.", nameof(bulletCount));
            if (waveAmplitude < 0f)
                throw new ArgumentException("Wave amplitude cannot be negative.", nameof(waveAmplitude));

            BulletCount = bulletCount;
            BaseDirection = baseDirection;
            WaveAmplitude = waveAmplitude;
            WaveFrequency = waveFrequency;
            Speed = speed;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            // Only spawn once at time 0
            if (lastTime <= 0f && currentTime >= 0f)
            {
                for (int i = 0; i < BulletCount; i++)
                {
                    // Calculate position in wave (0 to 1)
                    float wavePosition = BulletCount > 1 ? (float)i / (BulletCount - 1) : 0f;

                    // Calculate angle offset using sine wave
                    float sineValue = (float)Math.Sin(wavePosition * WaveFrequency * 2.0 * Math.PI);
                    float angleOffset = sineValue * WaveAmplitude;

                    float angle = BaseDirection + angleOffset;
                    float angleRad = (float)(angle * Math.PI / 180.0);

                    Vector2 direction = new Vector2((float)Math.Cos(angleRad), (float)Math.Sin(angleRad));

                    yield return new BulletSpawn
                    {
                        Position = context.Origin,
                        Direction = direction,
                        Speed = Speed,
                        Angle = angle
                    };
                }
            }
        }
    }
}


