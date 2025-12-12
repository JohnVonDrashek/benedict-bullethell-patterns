using System;
using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Basic
{
    /// <summary>
    /// Fires a single bullet in a specified direction.
    /// </summary>
    public sealed class SingleShotPattern : IBulletPattern
    {
        /// <summary>
        /// Direction the bullet will travel.
        /// </summary>
        public Vector2 Direction { get; }

        /// <summary>
        /// Speed of the bullet in units per second.
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// Duration is 0 (instantaneous spawn).
        /// </summary>
        public float Duration => 0f;

        /// <summary>
        /// Does not loop.
        /// </summary>
        public bool IsLooping => false;

        /// <summary>
        /// Creates a new single shot pattern.
        /// </summary>
        /// <param name="direction">Direction vector (will be normalized).</param>
        /// <param name="speed">Speed in units per second.</param>
        public SingleShotPattern(Vector2 direction, float speed)
        {
            Direction = Vector2.Normalize(direction);
            Speed = speed;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            // Only spawn once at time 0
            if (lastTime <= 0f && currentTime >= 0f)
            {
                float angle = (float)(Math.Atan2(Direction.Y, Direction.X) * 180.0 / Math.PI);
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

