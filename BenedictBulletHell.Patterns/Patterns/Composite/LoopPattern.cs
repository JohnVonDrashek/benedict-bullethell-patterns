using System;
using System.Collections.Generic;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Composite
{
    /// <summary>
    /// Makes any pattern loop indefinitely.
    /// </summary>
    public sealed class LoopPattern : IBulletPattern
    {
        private readonly IBulletPattern _pattern;

        /// <summary>
        /// The pattern being looped.
        /// </summary>
        public IBulletPattern Pattern { get; }

        /// <summary>
        /// Duration is infinite (loops forever).
        /// </summary>
        public float Duration => float.PositiveInfinity;

        /// <summary>
        /// Always loops.
        /// </summary>
        public bool IsLooping => true;

        /// <summary>
        /// Creates a new loop pattern.
        /// </summary>
        /// <param name="pattern">The pattern to loop.</param>
        public LoopPattern(IBulletPattern pattern)
        {
            _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            Pattern = pattern;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            float patternDuration = _pattern.Duration;
            
            // If pattern duration is 0 or infinite, handle specially
            if (patternDuration <= 0f || patternDuration == float.PositiveInfinity)
            {
                // For zero-duration patterns, just query once
                // For infinite patterns, pass through directly
                foreach (var spawn in _pattern.GetSpawns(lastTime, currentTime, context))
                    yield return spawn;
                yield break;
            }

            // Normalize times to pattern duration
            float lastNormalized = lastTime % patternDuration;
            float currentNormalized = currentTime % patternDuration;

            // Handle wrap-around case
            if (currentNormalized < lastNormalized)
            {
                // Pattern looped - handle two segments
                foreach (var spawn in _pattern.GetSpawns(lastNormalized, patternDuration, context))
                    yield return spawn;
                
                var context2 = context;
                context2.PatternAge = currentNormalized;
                foreach (var spawn in _pattern.GetSpawns(0f, currentNormalized, context2))
                    yield return spawn;
            }
            else
            {
                // No wrap-around
                var loopedContext = context;
                loopedContext.PatternAge = currentNormalized;
                foreach (var spawn in _pattern.GetSpawns(lastNormalized, currentNormalized, loopedContext))
                    yield return spawn;
            }
        }
    }
}


