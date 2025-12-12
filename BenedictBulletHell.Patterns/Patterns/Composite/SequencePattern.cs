using System;
using System.Collections.Generic;
using System.Linq;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Composite
{
    /// <summary>
    /// Executes multiple patterns one after another.
    /// </summary>
    public sealed class SequencePattern : IBulletPattern
    {
        private readonly IBulletPattern[] _patterns;
        private readonly float[] _patternStartTimes;

        /// <summary>
        /// The patterns in this sequence.
        /// </summary>
        public IReadOnlyList<IBulletPattern> Patterns { get; }

        /// <summary>
        /// Total duration of all patterns combined.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Whether this sequence loops.
        /// </summary>
        public bool IsLooping { get; }

        /// <summary>
        /// Creates a new sequence pattern.
        /// </summary>
        /// <param name="patterns">Patterns to execute in sequence.</param>
        /// <param name="looping">Whether to loop the sequence.</param>
        public SequencePattern(IEnumerable<IBulletPattern> patterns, bool looping = false)
        {
            if (patterns == null)
                throw new ArgumentNullException(nameof(patterns));

            _patterns = patterns.ToArray();
            if (_patterns.Length == 0)
                throw new ArgumentException("Sequence must contain at least one pattern.", nameof(patterns));

            Patterns = _patterns;
            IsLooping = looping;

            // Precompute start times for each pattern
            _patternStartTimes = new float[_patterns.Length];
            float currentTime = 0f;

            for (int i = 0; i < _patterns.Length; i++)
            {
                _patternStartTimes[i] = currentTime;
                float patternDuration = _patterns[i].Duration;
                if (patternDuration == float.PositiveInfinity)
                {
                    // If any pattern is infinite, the sequence is infinite (unless looping handles it)
                    if (looping)
                    {
                        // For looping sequences, we use a cycle duration
                        currentTime += _patterns[i].Duration > 0 && _patterns[i].Duration != float.PositiveInfinity 
                            ? _patterns[i].Duration 
                            : 1f; // Default 1 second for infinite patterns in sequence
                    }
                    else
                    {
                        currentTime = float.PositiveInfinity;
                        break;
                    }
                }
                else
                {
                    currentTime += patternDuration;
                }
            }

            Duration = looping ? float.PositiveInfinity : currentTime;
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            if (IsLooping && Duration != float.PositiveInfinity && Duration > 0f)
            {
                // Handle looping
                float cycleDuration = Duration;
                float lastCycleTime = lastTime % cycleDuration;
                float currentCycleTime = currentTime % cycleDuration;

                if (currentCycleTime < lastCycleTime)
                {
                    // Wrapped around - handle two segments
                    foreach (var spawn in GetSpawnsInRange(lastCycleTime, cycleDuration, context))
                        yield return spawn;
                    foreach (var spawn in GetSpawnsInRange(0f, currentCycleTime, context))
                        yield return spawn;
                }
                else
                {
                    foreach (var spawn in GetSpawnsInRange(lastCycleTime, currentCycleTime, context))
                        yield return spawn;
                }
            }
            else
            {
                foreach (var spawn in GetSpawnsInRange(lastTime, currentTime, context))
                    yield return spawn;
            }
        }

        private IEnumerable<BulletSpawn> GetSpawnsInRange(float lastTime, float currentTime, PatternContext context)
        {
            for (int i = 0; i < _patterns.Length; i++)
            {
                float patternStart = _patternStartTimes[i];
                float patternEnd = i < _patterns.Length - 1 
                    ? _patternStartTimes[i + 1] 
                    : (IsLooping ? float.PositiveInfinity : Duration);

                // Check if this pattern overlaps with the query time range
                if (currentTime >= patternStart && lastTime < patternEnd)
                {
                    // Adjust times relative to this pattern's start
                    float patternLastTime = Math.Max(0f, lastTime - patternStart);
                    float patternCurrentTime = Math.Min(
                        _patterns[i].Duration != float.PositiveInfinity 
                            ? _patterns[i].Duration 
                            : (currentTime - patternStart),
                        currentTime - patternStart);

                    if (patternCurrentTime > patternLastTime)
                    {
                        var patternContext = context;
                        patternContext.PatternAge = patternCurrentTime;
                        foreach (var spawn in _patterns[i].GetSpawns(patternLastTime, patternCurrentTime, patternContext))
                            yield return spawn;
                    }
                }
            }
        }
    }
}

