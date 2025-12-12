using System.Numerics;

namespace BenedictBulletHell.Patterns.Core
{
    /// <summary>
    /// Represents a single bullet spawn instruction.
    /// </summary>
    public struct BulletSpawn
    {
        /// <summary>
        /// World position where the bullet should spawn.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Initial direction vector (should be normalized, but not enforced).
        /// </summary>
        public Vector2 Direction { get; set; }

        /// <summary>
        /// Initial speed in units per second.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Initial rotation angle in degrees (useful for sprite orientation).
        /// </summary>
        public float Angle { get; set; }

        /// <summary>
        /// Optional custom data specific to bullet type or game logic.
        /// </summary>
        public object? BulletData { get; set; }
    }
}

