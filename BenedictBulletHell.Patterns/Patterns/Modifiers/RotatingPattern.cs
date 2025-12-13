using System;
using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Modifiers
{
    /// <summary>
    /// Rotates any pattern around the origin over time.
    /// </summary>
    public sealed class RotatingPattern : IBulletPattern
    {
        private readonly IBulletPattern _pattern;
        private readonly float _rotationSpeed;

        /// <summary>
        /// The base pattern being rotated.
        /// </summary>
        public IBulletPattern Pattern { get; }

        /// <summary>
        /// Rotation speed in degrees per second.
        /// </summary>
        public float RotationSpeed => _rotationSpeed;

        /// <summary>
        /// Duration matches the base pattern.
        /// </summary>
        public float Duration => _pattern.Duration;

        /// <summary>
        /// Whether this pattern loops (matches base pattern).
        /// </summary>
        public bool IsLooping => _pattern.IsLooping;

        /// <summary>
        /// Creates a new rotating pattern.
        /// </summary>
        /// <param name="pattern">The pattern to rotate.</param>
        /// <param name="rotationSpeed">Rotation speed in degrees per second.</param>
        public RotatingPattern(IBulletPattern pattern, float rotationSpeed)
        {
            _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            Pattern = pattern;
            _rotationSpeed = rotationSpeed;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            // Calculate rotation at current time
            float currentRotation = (currentTime * _rotationSpeed) % 360f;
            float currentRotationRad = (float)(currentRotation * Math.PI / 180.0);

            // Get spawns from base pattern
            foreach (var spawn in _pattern.GetSpawns(lastTime, currentTime, context))
            {
                // Rotate direction vector
                float spawnAngleRad = (float)Math.Atan2(spawn.Direction.Y, spawn.Direction.X);
                float newAngleRad = spawnAngleRad + currentRotationRad;

                Vector2 rotatedDirection = new Vector2(
                    (float)Math.Cos(newAngleRad),
                    (float)Math.Sin(newAngleRad)
                );

                // Rotate the angle as well (for sprite rotation)
                float newAngle = spawn.Angle + currentRotation;

                yield return new BulletSpawn
                {
                    Position = spawn.Position,
                    Direction = rotatedDirection,
                    Speed = spawn.Speed,
                    Angle = newAngle,
                    BulletData = spawn.BulletData
                };
            }
        }
    }
}


