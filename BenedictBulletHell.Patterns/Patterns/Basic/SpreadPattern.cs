using System;
using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Basic
{
    /// <summary>
    /// Fires bullets in a fan/spread formation.
    /// </summary>
    public sealed class SpreadPattern : IBulletPattern
    {
        /// <summary>
        /// Number of bullets to fire.
        /// </summary>
        public int BulletCount { get; }

        /// <summary>
        /// Total angle spread in degrees.
        /// </summary>
        public float AngleSpread { get; }

        /// <summary>
        /// Center angle in degrees (0 = right, 90 = up).
        /// </summary>
        public float BaseAngle { get; }

        /// <summary>
        /// Speed of the bullets in units per second.
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// Duration is 0 (all bullets spawn simultaneously).
        /// </summary>
        public float Duration => 0f;

        /// <summary>
        /// Does not loop.
        /// </summary>
        public bool IsLooping => false;

        /// <summary>
        /// Creates a new spread pattern.
        /// </summary>
        /// <param name="bulletCount">Number of bullets to fire.</param>
        /// <param name="angleSpread">Total angle spread in degrees.</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="baseAngle">Center angle in degrees (0 = right, 90 = up).</param>
        public SpreadPattern(int bulletCount, float angleSpread, float speed, float baseAngle = 0f)
        {
            if (bulletCount < 1)
                throw new ArgumentException("Bullet count must be at least 1.", nameof(bulletCount));

            BulletCount = bulletCount;
            AngleSpread = angleSpread;
            Speed = speed;
            BaseAngle = baseAngle;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            // Only spawn once at time 0
            if (lastTime <= 0f && currentTime >= 0f)
            {
                float startAngle = BaseAngle - (AngleSpread / 2f);
                float angleStep = BulletCount > 1 ? AngleSpread / (BulletCount - 1) : 0f;

                for (int i = 0; i < BulletCount; i++)
                {
                    float angle = startAngle + (angleStep * i);
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

