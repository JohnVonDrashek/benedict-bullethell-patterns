using System;
using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Advanced
{
    /// <summary>
    /// Creates a rotating spiral of bullets.
    /// </summary>
    public sealed class SpiralPattern : IBulletPattern
    {
        /// <summary>
        /// Number of bullets per full revolution.
        /// </summary>
        public int BulletsPerRevolution { get; }

        /// <summary>
        /// Total number of revolutions to complete.
        /// </summary>
        public float TotalRevolutions { get; }

        /// <summary>
        /// Speed of the bullets in units per second.
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// Rotation speed in degrees per second.
        /// </summary>
        public float RotationSpeed { get; }

        /// <summary>
        /// Starting angle in degrees.
        /// </summary>
        public float StartAngle { get; }

        /// <summary>
        /// Total duration of the spiral pattern.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Whether this pattern loops.
        /// </summary>
        public bool IsLooping { get; }

        /// <summary>
        /// Creates a new spiral pattern.
        /// </summary>
        /// <param name="bulletsPerRevolution">Number of bullets per full revolution.</param>
        /// <param name="totalRevolutions">Total number of revolutions.</param>
        /// <param name="speed">Speed in units per second.</param>
        /// <param name="rotationSpeed">Rotation speed in degrees per second (default: 360).</param>
        /// <param name="startAngle">Starting angle in degrees (default: 0).</param>
        /// <param name="looping">Whether the spiral loops indefinitely (default: false).</param>
        public SpiralPattern(
            int bulletsPerRevolution,
            float totalRevolutions,
            float speed,
            float rotationSpeed = 360f,
            float startAngle = 0f,
            bool looping = false)
        {
            if (bulletsPerRevolution < 1)
                throw new ArgumentException("Bullets per revolution must be at least 1.", nameof(bulletsPerRevolution));
            if (totalRevolutions <= 0f && !looping)
                throw new ArgumentException("Total revolutions must be greater than 0 for non-looping patterns.", nameof(totalRevolutions));
            if (rotationSpeed <= 0f)
                throw new ArgumentException("Rotation speed must be greater than 0.", nameof(rotationSpeed));

            BulletsPerRevolution = bulletsPerRevolution;
            TotalRevolutions = totalRevolutions;
            Speed = speed;
            RotationSpeed = rotationSpeed;
            StartAngle = startAngle;
            IsLooping = looping;

            Duration = looping 
                ? float.PositiveInfinity 
                : (totalRevolutions * 360f) / rotationSpeed;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            if (currentTime < 0f) yield break;

            float timeBetweenBullets = (360f / RotationSpeed) / BulletsPerRevolution;

            // Calculate time range for query
            float queryStartTime = Math.Max(0f, lastTime);
            float queryEndTime = currentTime;

            // Find all bullets that should spawn in this time range
            int startBulletIndex = (int)(queryStartTime / timeBetweenBullets);
            int endBulletIndex = (int)Math.Ceiling(queryEndTime / timeBetweenBullets);

            // Handle looping if needed
            if (IsLooping && Duration != float.PositiveInfinity)
            {
                // For looping patterns, handle wrap-around
                float cycleDuration = Duration;
                int bulletsPerCycle = (int)(cycleDuration / timeBetweenBullets);
                
                // Normalize indices to cycle
                startBulletIndex = startBulletIndex % bulletsPerCycle;
                endBulletIndex = endBulletIndex % bulletsPerCycle;
                
                if (endBulletIndex < startBulletIndex)
                {
                    // Wrapped around - handle two segments
                    for (int i = startBulletIndex; i < bulletsPerCycle; i++)
                    {
                        float bulletSpawnTime = i * timeBetweenBullets;
                        float revolutions = (bulletSpawnTime * RotationSpeed) / 360f;
                        float angle = StartAngle + (revolutions * 360f);
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
                    
                    for (int i = 0; i < endBulletIndex; i++)
                    {
                        float bulletSpawnTime = i * timeBetweenBullets;
                        float revolutions = (bulletSpawnTime * RotationSpeed) / 360f;
                        float angle = StartAngle + (revolutions * 360f);
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
                    yield break;
                }
            }
            else if (!IsLooping && Duration != float.PositiveInfinity)
            {
                // Cap to pattern duration for non-looping patterns
                int maxBullets = (int)(Duration / timeBetweenBullets);
                endBulletIndex = Math.Min(endBulletIndex, maxBullets);
            }

            // Generate bullets in the range
            for (int i = startBulletIndex; i < endBulletIndex; i++)
            {
                float bulletSpawnTime = i * timeBetweenBullets;

                // Only spawn if this bullet's time is within the query range
                if (bulletSpawnTime >= queryStartTime && bulletSpawnTime <= queryEndTime)
                {
                    // Calculate angle at spawn time
                    float revolutions = (bulletSpawnTime * RotationSpeed) / 360f;
                    float angle = StartAngle + (revolutions * 360f);
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
