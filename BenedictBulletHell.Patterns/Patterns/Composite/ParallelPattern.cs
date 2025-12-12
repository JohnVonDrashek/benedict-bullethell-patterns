using System;
using System.Collections.Generic;
using System.Linq;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Composite
{
    /// <summary>
    /// Executes multiple patterns simultaneously.
    /// </summary>
    public sealed class ParallelPattern : IBulletPattern
    {
        private readonly IBulletPattern[] _patterns;

        /// <summary>
        /// The patterns in this parallel group.
        /// </summary>
        public IReadOnlyList<IBulletPattern> Patterns { get; }

        /// <summary>
        /// Duration is the maximum of all patterns.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Whether any pattern in this group loops.
        /// </summary>
        public bool IsLooping { get; }

        /// <summary>
        /// Creates a new parallel pattern.
        /// </summary>
        /// <param name="patterns">Patterns to execute in parallel.</param>
        public ParallelPattern(IEnumerable<IBulletPattern> patterns)
        {
            if (patterns == null)
                throw new ArgumentNullException(nameof(patterns));

            _patterns = patterns.ToArray();
            if (_patterns.Length == 0)
                throw new ArgumentException("Parallel pattern must contain at least one pattern.", nameof(patterns));

            Patterns = _patterns;

            Duration = _patterns.Any() 
                ? _patterns.Max(p => p.Duration)
                : 0f;
            IsLooping = _patterns.Any(p => p.IsLooping);
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            foreach (var pattern in _patterns)
            {
                foreach (var spawn in pattern.GetSpawns(lastTime, currentTime, context))
                    yield return spawn;
            }
        }
    }
}

