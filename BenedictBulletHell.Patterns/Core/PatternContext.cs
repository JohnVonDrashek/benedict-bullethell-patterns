using System.Collections.Generic;
using System.Numerics;

namespace BenedictBulletHell.Patterns.Core
{
    /// <summary>
    /// Contextual information passed to patterns at execution time.
    /// </summary>
    public struct PatternContext
    {
        /// <summary>
        /// World position where the pattern originates (typically enemy/boss position).
        /// </summary>
        public Vector2 Origin { get; set; }

        /// <summary>
        /// Optional target position (used by aimed/homing patterns).
        /// </summary>
        public Vector2? Target { get; set; }

        /// <summary>
        /// How long this specific pattern instance has been running (useful for loops).
        /// </summary>
        public float PatternAge { get; set; }

        /// <summary>
        /// Optional runtime parameter overrides (speed multiplier, angle offset, etc.).
        /// </summary>
        public IReadOnlyDictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Gets a typed metadata value, or default if not present.
        /// </summary>
        public T? GetMetadata<T>(string key) where T : class
        {
            return Metadata?.TryGetValue(key, out var value) == true ? value as T : null;
        }

        /// <summary>
        /// Gets a typed metadata value, or default value type if not present.
        /// </summary>
        public T GetMetadataValue<T>(string key, T defaultValue = default) where T : struct
        {
            if (Metadata?.TryGetValue(key, out var value) == true && value is T typed)
                return typed;
            return defaultValue;
        }
    }
}

