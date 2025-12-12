using System;
using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Basic
{
    /// <summary>
    /// Fires multiple bullets in quick succession.
    /// </summary>
    public sealed class BurstPattern : IBulletPattern
    {
        /// <summary>
        /// Number of bullets in the burst.
        /// </summary>
        public int BulletCount { get; }

        /// <summary>
        /// Direction the bullets will travel.
        /// </summary>
        public Vector2 Direction { get; }

        /// <summary>
        /// Speed of the bullets in units per second.
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// Delay between each shot in seconds.
        /// </summary>
        public float DelayBetweenShots { get; }

        /// <summary>
        /// Total duration of the burst.
        /// </summary>
        public float Duration => (BulletCount - 1) * DelayBetweenShots;

        /// <summary>
        /// Does not loop.
        /// </summary>
        public bool IsLooping => false;

        /// <summary>
        /// Creates a new burst pattern.
        /// </summary>
        /// <param name="bulletCount">Number of bullets to fire.</param>
        /// <param name="direction">Direction vector (will be normalized).</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="delayBetweenShots">Delay between each shot in seconds.</param>
        public BurstPattern(int bulletCount, Vector2 direction, float speed, float delayBetweenShots)
        {
            if (bulletCount < 1)
                throw new ArgumentException("Bullet count must be at least 1.", nameof(bulletCount));
            if (delayBetweenShots < 0f)
                throw new ArgumentException("Delay cannot be negative.", nameof(delayBetweenShots));

            BulletCount = bulletCount;
            Direction = Vector2.Normalize(direction);
            Speed = speed;
            DelayBetweenShots = delayBetweenShots;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            if (currentTime < 0f) yield break;

            float angle = (float)(Math.Atan2(Direction.Y, Direction.X) * 180.0 / Math.PI);

            for (int i = 0; i < BulletCount; i++)
            {
                float spawnTime = i * DelayBetweenShots;

                // Spawn if this bullet's time is within the query range
                if (spawnTime >= lastTime && spawnTime <= currentTime)
                {
                    yield return new BulletSpawn
                    {
                        Position = context.Origin,
                        Direction = Direction,
                        Speed = Speed,
                        Angle = angle
                    };
                }
            }
        }
    }
}

