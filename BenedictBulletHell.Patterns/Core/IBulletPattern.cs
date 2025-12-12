using System.Collections.Generic;
using System.Numerics;

namespace BenedictBulletHell.Patterns.Core
{
    /// <summary>
    /// Defines a bullet pattern that generates spawn instructions over time.
    /// </summary>
    public interface IBulletPattern
    {
        /// <summary>
        /// Gets all bullet spawn instructions that should occur between lastTime and currentTime.
        /// </summary>
        /// <param name="lastTime">The last time this pattern was queried (or start time for first call).</param>
        /// <param name="currentTime">The current time.</param>
        /// <param name="context">Contextual information (origin, target, etc.).</param>
        /// <returns>Bullet spawn instructions.</returns>
        IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context);

        /// <summary>
        /// Total duration of this pattern in seconds. Returns float.PositiveInfinity if looping.
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// Whether this pattern loops indefinitely.
        /// </summary>
        bool IsLooping { get; }
    }
}


