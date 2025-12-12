using System;
using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Basic
{
    /// <summary>
    /// Fires bullets in a circle around the origin.
    /// </summary>
    public sealed class RingPattern : IBulletPattern
    {
        /// <summary>
        /// Number of bullets to fire.
        /// </summary>
        public int BulletCount { get; }

        /// <summary>
        /// Speed of the bullets in units per second.
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// Starting angle in degrees (0 = right, 90 = up).
        /// </summary>
        public float StartAngle { get; }

        /// <summary>
        /// Duration is 0 (all bullets spawn simultaneously).
        /// </summary>
        public float Duration => 0f;

        /// <summary>
        /// Does not loop.
        /// </summary>
        public bool IsLooping => false;

        /// <summary>
        /// Creates a new ring pattern.
        /// </summary>
        /// <param name="bulletCount">Number of bullets to fire.</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="startAngle">Starting angle in degrees (0 = right, 90 = up).</param>
        public RingPattern(int bulletCount, float speed, float startAngle = 0f)
        {
            if (bulletCount < 1)
                throw new ArgumentException("Bullet count must be at least 1.", nameof(bulletCount));

            BulletCount = bulletCount;
            Speed = speed;
            StartAngle = startAngle;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            // Only spawn once at time 0
            if (lastTime <= 0f && currentTime >= 0f)
            {
                float angleStep = 360f / BulletCount;

                for (int i = 0; i < BulletCount; i++)
                {
                    float angle = StartAngle + (angleStep * i);
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

