using System;
using System.Collections.Generic;
using BenedictBulletHell.Patterns.Core;

namespace BenedictBulletHell.Patterns.Patterns.Composite
{
    /// <summary>
    /// Repeats a pattern multiple times with optional delay between repeats.
    /// </summary>
    public sealed class RepeatPattern : IBulletPattern
    {
        private readonly IBulletPattern _pattern;
        private readonly int _repeatCount;
        private readonly float _delayBetweenRepeats;

        /// <summary>
        /// The pattern being repeated.
        /// </summary>
        public IBulletPattern Pattern { get; }

        /// <summary>
        /// Number of times to repeat the pattern.
        /// </summary>
        public int RepeatCount { get; }

        /// <summary>
        /// Delay between each repeat in seconds.
        /// </summary>
        public float DelayBetweenRepeats { get; }

        /// <summary>
        /// Total duration of all repeats.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Does not loop.
        /// </summary>
        public bool IsLooping => false;

        /// <summary>
        /// Creates a new repeat pattern.
        /// </summary>
        /// <param name="pattern">The pattern to repeat.</param>
        /// <param name="repeatCount">Number of times to repeat.</param>
        /// <param name="delayBetweenRepeats">Delay between each repeat in seconds.</param>
        public RepeatPattern(IBulletPattern pattern, int repeatCount, float delayBetweenRepeats = 0f)
        {
            _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            Pattern = pattern;
            
            if (repeatCount < 1)
                throw new ArgumentException("Repeat count must be at least 1.", nameof(repeatCount));
            if (delayBetweenRepeats < 0f)
                throw new ArgumentException("Delay cannot be negative.", nameof(delayBetweenRepeats));

            _repeatCount = repeatCount;
            RepeatCount = repeatCount;
            _delayBetweenRepeats = delayBetweenRepeats;
            DelayBetweenRepeats = delayBetweenRepeats;

            // Calculate total duration
            float patternDuration = pattern.Duration == float.PositiveInfinity ? 0f : pattern.Duration;
            Duration = (patternDuration * repeatCount) + (delayBetweenRepeats * (repeatCount - 1));
        }

        /// <inheritdoc/>
        public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
        {
            if (currentTime < 0f) yield break;

            float patternDuration = _pattern.Duration == float.PositiveInfinity ? 0f : _pattern.Duration;

            for (int i = 0; i < _repeatCount; i++)
            {
                float repeatStartTime = i * (patternDuration + _delayBetweenRepeats);
                float repeatEndTime = repeatStartTime + patternDuration;

                // Check if this repeat overlaps with the query time range
                if (currentTime >= repeatStartTime && lastTime < repeatEndTime)
                {
                    // Adjust times relative to this repeat's start
                    float repeatLastTime = Math.Max(0f, lastTime - repeatStartTime);
                    float repeatCurrentTime = Math.Min(patternDuration, currentTime - repeatStartTime);

                    if (repeatCurrentTime > repeatLastTime)
                    {
                        var repeatContext = context;
                        repeatContext.PatternAge = repeatCurrentTime;
                        
                        foreach (var spawn in _pattern.GetSpawns(repeatLastTime, repeatCurrentTime, repeatContext))
                            yield return spawn;
                    }
                }
            }
        }
    }
}


