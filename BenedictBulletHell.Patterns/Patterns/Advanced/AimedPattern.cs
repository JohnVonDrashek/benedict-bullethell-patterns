using System;
using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Advanced
{
    /// <summary>
    /// Fires bullet(s) toward a target position.
    /// </summary>
    public sealed class AimedPattern : IBulletPattern
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
        /// Optional spread angle in degrees (if multiple bullets).
        /// </summary>
        public float SpreadAngle { get; }

        /// <summary>
        /// Duration is 0 (instantaneous spawn).
        /// </summary>
        public float Duration => 0f;

        /// <summary>
        /// Does not loop.
        /// </summary>
        public bool IsLooping => false;

        /// <summary>
        /// Creates a new aimed pattern.
        /// </summary>
        /// <param name="bulletCount">Number of bullets to fire.</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="spreadAngle">Optional spread angle in degrees for multiple bullets (default: 0).</param>
        public AimedPattern(int bulletCount, float speed, float spreadAngle = 0f)
        {
            if (bulletCount < 1)
                throw new ArgumentException("Bullet count must be at least 1.", nameof(bulletCount));
            if (spreadAngle < 0f)
                throw new ArgumentException("Spread angle cannot be negative.", nameof(spreadAngle));

            BulletCount = bulletCount;
            Speed = speed;
            SpreadAngle = spreadAngle;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            // Only spawn once at time 0
            if (lastTime <= 0f && currentTime >= 0f)
            {
                // Need a target to aim at
                if (context.Target == null)
                {
                    // If no target, fire in a default direction (right)
                    for (int i = 0; i < BulletCount; i++)
                    {
                        float angleOffset = 0f;
                        if (BulletCount > 1 && SpreadAngle > 0f)
                        {
                            float startOffset = -SpreadAngle / 2f;
                            float step = SpreadAngle / (BulletCount - 1);
                            angleOffset = startOffset + (step * i);
                        }

                        float angle = 0f + angleOffset; // 0 degrees = right
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
                else
                {
                    // Aim at target
                    Vector2 toTarget = context.Target.Value - context.Origin;
                    float distance = toTarget.Length();
                    
                    if (distance > 0.0001f) // Avoid division by zero
                    {
                        Vector2 baseDirection = Vector2.Normalize(toTarget);
                        float baseAngle = (float)(Math.Atan2(baseDirection.Y, baseDirection.X) * 180.0 / Math.PI);

                        for (int i = 0; i < BulletCount; i++)
                        {
                            float angleOffset = 0f;
                            if (BulletCount > 1 && SpreadAngle > 0f)
                            {
                                float startOffset = -SpreadAngle / 2f;
                                float step = SpreadAngle / (BulletCount - 1);
                                angleOffset = startOffset + (step * i);
                            }

                            float angle = baseAngle + angleOffset;
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
                    else
                    {
                        // Target is at origin, fire in default direction
                        Vector2 defaultDir = new Vector2(1, 0);
                        yield return new BulletSpawn
                        {
                            Position = context.Origin,
                            Direction = defaultDir,
                            Speed = Speed,
                            Angle = 0f
                        };
                    }
                }
            }
        }
    }
}


