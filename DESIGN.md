# BenedictBulletHell.Patterns - Design Document

## Overview

**BenedictBulletHell.Patterns** is a framework-agnostic C# library for defining and executing bullet hell pattern generation. It provides a clean API for creating complex bullet patterns while remaining completely decoupled from rendering, physics, and game engine concerns.

### Core Principles

1. **Framework Agnostic**: Works with MonoGame, FNA, Unity, or any .NET game framework
2. **Pattern Definition Only**: Defines *when* and *how* bullets spawn, not their lifecycle
3. **Composable**: Simple patterns combine into complex behaviors
4. **Time-Based**: All patterns are driven by absolute time
5. **Performance-First**: Allocation-free hot paths with convenience APIs available
6. **Stateless & Immutable**: Patterns are predictable and thread-safe

## Architecture

### Core Abstractions

#### IBulletPattern

The fundamental interface for all bullet patterns:

```csharp
public interface IBulletPattern
{
    /// <summary>
    /// Gets all bullet spawn instructions that should occur between lastTime and currentTime.
    /// </summary>
    /// <param name="lastTime">The last time this pattern was queried (or start time for first call)</param>
    /// <param name="currentTime">The current time</param>
    /// <param name="context">Contextual information (origin, target, etc.)</param>
    /// <returns>Bullet spawn instructions</returns>
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
```

**Design Rationale:**
- Uses `IEnumerable<BulletSpawn>` for convenience API (Tier 2)
- Future: Allocation-free `Span<BulletSpawn>` overload will be added (Tier 1)
- Absolute time prevents duplicate spawns when queried multiple times
- Context provides runtime data without mutating the pattern

#### BulletSpawn

A value type representing a single bullet spawn instruction:

```csharp
public struct BulletSpawn
{
    /// <summary>
    /// World position where the bullet should spawn.
    /// </summary>
    public Vector2 Position { get; init; }
    
    /// <summary>
    /// Initial direction vector (should be normalized, but not enforced).
    /// </summary>
    public Vector2 Direction { get; init; }
    
    /// <summary>
    /// Initial speed in units per second.
    /// </summary>
    public float Speed { get; init; }
    
    /// <summary>
    /// Initial rotation angle in degrees (useful for sprite orientation).
    /// </summary>
    public float Angle { get; init; }
    
    /// <summary>
    /// Optional custom data specific to bullet type or game logic.
    /// </summary>
    public object? BulletData { get; init; }
}
```

**Design Rationale:**
- `struct` for value semantics and stack allocation
- `init` properties for immutable construction
- `BulletData` allows game-specific metadata (bullet type, damage, etc.)
- `Angle` separate from `Direction` for sprite rotation needs

#### PatternContext

Contextual information passed to patterns at execution time:

```csharp
public struct PatternContext
{
    /// <summary>
    /// World position where the pattern originates (typically enemy/boss position).
    /// </summary>
    public Vector2 Origin { get; init; }
    
    /// <summary>
    /// Optional target position (used by aimed/homing patterns).
    /// </summary>
    public Vector2? Target { get; init; }
    
    /// <summary>
    /// How long this specific pattern instance has been running (useful for loops).
    /// </summary>
    public float PatternAge { get; init; }
    
    /// <summary>
    /// Optional runtime parameter overrides (speed multiplier, angle offset, etc.).
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
    
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
```

**Design Rationale:**
- Immutable struct for safe context passing
- `Metadata` dictionary allows runtime tweaks without pattern mutation
- Helper methods provide type-safe metadata access
- `PatternAge` enables patterns to adapt over time (e.g., increasing difficulty)

### Pattern Types

#### Basic Patterns

##### SingleShotPattern

Fires a single bullet in a specified direction.

```csharp
public sealed class SingleShotPattern : IBulletPattern
{
    public Vector2 Direction { get; }
    public float Speed { get; }
    public float Duration { get; }
    public bool IsLooping => false;
    
    public SingleShotPattern(Vector2 direction, float speed)
    {
        Direction = Vector2.Normalize(direction);
        Speed = speed;
        Duration = 0f; // Instantaneous
    }
}
```

**Use Cases:**
- Basic enemy attacks
- Player weapon firing
- Building blocks for composite patterns

##### BurstPattern

Fires multiple bullets in quick succession.

```csharp
public sealed class BurstPattern : IBulletPattern
{
    public int BulletCount { get; }
    public Vector2 Direction { get; }
    public float Speed { get; }
    public float DelayBetweenShots { get; }
    public float Duration => (BulletCount - 1) * DelayBetweenShots;
    public bool IsLooping => false;
    
    public BurstPattern(int bulletCount, Vector2 direction, float speed, float delayBetweenShots)
    {
        BulletCount = bulletCount;
        Direction = Vector2.Normalize(direction);
        Speed = speed;
        DelayBetweenShots = delayBetweenShots;
    }
}
```

**Use Cases:**
- Shotgun-style weapons
- Rapid-fire sequences
- Building blocks for more complex patterns

##### SpreadPattern

Fires bullets in a fan/spread formation.

```csharp
public sealed class SpreadPattern : IBulletPattern
{
    public int BulletCount { get; }
    public float AngleSpread { get; } // Total angle in degrees
    public float BaseAngle { get; } // Center angle in degrees (0 = right, 90 = up)
    public float Speed { get; }
    public float Duration => 0f; // All bullets spawn simultaneously
    public bool IsLooping => false;
    
    public SpreadPattern(int bulletCount, float angleSpread, float speed, float baseAngle = 0f)
    {
        BulletCount = bulletCount;
        AngleSpread = angleSpread;
        BaseAngle = baseAngle;
        Speed = speed;
    }
}
```

**Use Cases:**
- Shotgun patterns
- Cone attacks
- Scatter shots

##### RingPattern

Fires bullets in a circle around the origin.

```csharp
public sealed class RingPattern : IBulletPattern
{
    public int BulletCount { get; }
    public float Speed { get; }
    public float StartAngle { get; } // Starting angle in degrees
    public float Duration => 0f; // All bullets spawn simultaneously
    public bool IsLooping => false;
    
    public RingPattern(int bulletCount, float speed, float startAngle = 0f)
    {
        BulletCount = bulletCount;
        Speed = speed;
        StartAngle = startAngle;
    }
}
```

**Use Cases:**
- 360-degree attacks
- Explosion patterns
- Defense patterns

##### AimedPattern

Fires bullet(s) toward a target position.

```csharp
public sealed class AimedPattern : IBulletPattern
{
    public int BulletCount { get; }
    public float Speed { get; }
    public float SpreadAngle { get; } // Optional spread if multiple bullets
    public float Duration => 0f;
    public bool IsLooping => false;
    
    public AimedPattern(int bulletCount, float speed, float spreadAngle = 0f)
    {
        BulletCount = bulletCount;
        Speed = speed;
        SpreadAngle = spreadAngle;
    }
    
    // Direction computed from Origin -> Target in context
}
```

**Use Cases:**
- Enemy tracking shots
- Homing-like behavior (direction locked at spawn)
- Precision attacks

#### Advanced Patterns

##### SpiralPattern

Creates a rotating spiral of bullets.

```csharp
public sealed class SpiralPattern : IBulletPattern
{
    public int BulletsPerRevolution { get; }
    public float TotalRevolutions { get; }
    public float Speed { get; }
    public float RotationSpeed { get; } // Degrees per second
    public float StartAngle { get; }
    public float Duration { get; }
    public bool IsLooping { get; }
    
    public SpiralPattern(
        int bulletsPerRevolution, 
        float totalRevolutions, 
        float speed, 
        float rotationSpeed = 360f,
        float startAngle = 0f,
        bool looping = false)
    {
        BulletsPerRevolution = bulletsPerRevolution;
        TotalRevolutions = totalRevolutions;
        Speed = speed;
        RotationSpeed = rotationSpeed;
        StartAngle = startAngle;
        Duration = looping ? float.PositiveInfinity : CalculateDuration();
        IsLooping = looping;
    }
    
    private float CalculateDuration()
    {
        // Duration needed for totalRevolutions at rotationSpeed
        return (TotalRevolutions * 360f) / RotationSpeed;
    }
}
```

**Use Cases:**
- Boss attack patterns
- Complex bullet hell sequences
- Rotating defense patterns

##### WavePattern

Creates bullets in a sine-wave pattern.

```csharp
public sealed class WavePattern : IBulletPattern
{
    public int BulletCount { get; }
    public float BaseDirection { get; } // Direction of wave travel (degrees)
    public float WaveAmplitude { get; } // Side-to-side amplitude (degrees)
    public float WaveFrequency { get; } // Frequency of the wave
    public float Speed { get; }
    public float Duration => 0f; // All bullets spawn at once with different directions
    public bool IsLooping => false;
    
    public WavePattern(
        int bulletCount, 
        float baseDirection, 
        float waveAmplitude, 
        float waveFrequency, 
        float speed)
    {
        BulletCount = bulletCount;
        BaseDirection = baseDirection;
        WaveAmplitude = waveAmplitude;
        WaveFrequency = waveFrequency;
        Speed = speed;
    }
}
```

**Use Cases:**
- Sinusoidal attack patterns
- Wavy bullet streams
- Decorative patterns

##### HomingPattern

Special pattern that requires target tracking (note: direction updates over time, handled by game code).

```csharp
public sealed class HomingPattern : IBulletPattern
{
    public float TurnRate { get; } // Degrees per second the bullet can turn
    public float Speed { get; }
    public float Duration => 0f; // Spawn is instant, tracking happens in game loop
    public bool IsLooping => false;
    
    public HomingPattern(float speed, float turnRate = 90f)
    {
        Speed = speed;
        TurnRate = turnRate;
    }
    
    // Spawn includes metadata for game code to handle homing behavior
    public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
    {
        if (context.Target == null)
            yield break;
            
        var direction = Vector2.Normalize(context.Target.Value - context.Origin);
        yield return new BulletSpawn
        {
            Position = context.Origin,
            Direction = direction,
            Speed = Speed,
            Angle = MathF.Atan2(direction.Y, direction.X) * 180f / MathF.PI,
            BulletData = new HomingData { TurnRate = TurnRate }
        };
    }
}

public class HomingData
{
    public float TurnRate { get; init; }
}
```

**Use Cases:**
- Tracking missiles
- Heat-seeking bullets
- Smart projectiles

**Note:** Full homing behavior requires game code to update bullet direction each frame toward target.

#### Composite Patterns

##### SequencePattern

Executes multiple patterns one after another.

```csharp
public sealed class SequencePattern : IBulletPattern
{
    private readonly IBulletPattern[] _patterns;
    private readonly float[] _patternStartTimes;
    private readonly float _totalDuration;
    
    public IReadOnlyList<IBulletPattern> Patterns { get; }
    public float Duration { get; }
    public bool IsLooping { get; }
    
    public SequencePattern(IEnumerable<IBulletPattern> patterns, bool looping = false)
    {
        _patterns = patterns.ToArray();
        Patterns = _patterns;
        IsLooping = looping;
        
        // Precompute start times for each pattern
        _patternStartTimes = new float[_patterns.Length];
        float currentTime = 0f;
        
        for (int i = 0; i < _patterns.Length; i++)
        {
            _patternStartTimes[i] = currentTime;
            currentTime += _patterns[i].Duration;
        }
        
        _totalDuration = currentTime;
        Duration = looping ? float.PositiveInfinity : _totalDuration;
    }
}
```

**Use Cases:**
- Multi-phase boss attacks
- Complex attack sequences
- Pattern combinations

##### ParallelPattern

Executes multiple patterns simultaneously.

```csharp
public sealed class ParallelPattern : IBulletPattern
{
    public IReadOnlyList<IBulletPattern> Patterns { get; }
    public float Duration { get; }
    public bool IsLooping { get; }
    
    public ParallelPattern(IEnumerable<IBulletPattern> patterns, bool looping = false)
    {
        Patterns = patterns.ToArray();
        
        // Duration is the maximum of all patterns
        Duration = Patterns.Any() 
            ? (looping ? float.PositiveInfinity : Patterns.Max(p => p.Duration))
            : 0f;
        IsLooping = looping || Patterns.Any(p => p.IsLooping);
    }
}
```

**Use Cases:**
- Layered attacks
- Multiple simultaneous patterns
- Complex bullet hell sequences

##### RepeatPattern

Repeats a pattern multiple times with optional delay.

```csharp
public sealed class RepeatPattern : IBulletPattern
{
    private readonly IBulletPattern _pattern;
    private readonly int _repeatCount;
    private readonly float _delayBetweenRepeats;
    
    public IBulletPattern Pattern { get; }
    public int RepeatCount { get; }
    public float DelayBetweenRepeats { get; }
    public float Duration { get; }
    public bool IsLooping => false;
    
    public RepeatPattern(IBulletPattern pattern, int repeatCount, float delayBetweenRepeats = 0f)
    {
        _pattern = pattern;
        Pattern = pattern;
        _repeatCount = repeatCount;
        RepeatCount = repeatCount;
        _delayBetweenRepeats = delayBetweenRepeats;
        
        Duration = (pattern.Duration * repeatCount) + (delayBetweenRepeats * (repeatCount - 1));
    }
}
```

**Use Cases:**
- Repeating simple patterns
- Rhythmic attacks
- Building complexity from simple patterns

##### LoopPattern

Makes any pattern loop indefinitely.

```csharp
public sealed class LoopPattern : IBulletPattern
{
    private readonly IBulletPattern _pattern;
    
    public IBulletPattern Pattern { get; }
    public float Duration => float.PositiveInfinity;
    public bool IsLooping => true;
    
    public LoopPattern(IBulletPattern pattern)
    {
        _pattern = pattern;
        Pattern = pattern;
    }
    
    public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
    {
        float patternDuration = _pattern.Duration;
        if (patternDuration <= 0f) return Enumerable.Empty<BulletSpawn>();
        
        // Loop the time within pattern duration
        float normalizedLast = lastTime % patternDuration;
        float normalizedCurrent = currentTime % patternDuration;
        
        // Handle wrap-around
        if (normalizedCurrent < normalizedLast)
        {
            // Pattern looped
            foreach (var spawn in _pattern.GetSpawns(normalizedLast, patternDuration, context))
                yield return spawn;
            foreach (var spawn in _pattern.GetSpawns(0f, normalizedCurrent, context))
                yield return spawn;
        }
        else
        {
            foreach (var spawn in _pattern.GetSpawns(normalizedLast, normalizedCurrent, context))
                yield return spawn;
        }
    }
}
```

**Use Cases:**
- Continuous attacks
- Repeating patterns
- Ongoing bullet hell sequences

#### Modifier Patterns

Modifier patterns wrap other patterns to add transformations.

##### RotatingPattern

Rotates any pattern around the origin over time.

```csharp
public sealed class RotatingPattern : IBulletPattern
{
    private readonly IBulletPattern _pattern;
    private readonly float _rotationSpeed; // Degrees per second
    
    public IBulletPattern Pattern { get; }
    public float RotationSpeed { get; }
    public float Duration => _pattern.Duration;
    public bool IsLooping => _pattern.IsLooping;
    
    public RotatingPattern(IBulletPattern pattern, float rotationSpeed)
    {
        _pattern = pattern;
        _rotationSpeed = rotationSpeed;
    }
    
    public IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
    {
        // Calculate rotation angles
        float lastRotation = (lastTime * _rotationSpeed) % 360f;
        float currentRotation = (currentTime * _rotationSpeed) % 360f;
        
        // Get spawns from base pattern
        foreach (var spawn in _pattern.GetSpawns(lastTime, currentTime, context))
        {
            // Rotate direction vector
            float angleRad = MathF.Atan2(spawn.Direction.Y, spawn.Direction.X) * MathF.PI / 180f;
            float newAngle = angleRad + (currentRotation * MathF.PI / 180f);
            
            yield return spawn with
            {
                Direction = new Vector2(MathF.Cos(newAngle), MathF.Sin(newAngle)),
                Angle = spawn.Angle + currentRotation
            };
        }
    }
}
```

**Use Cases:**
- Rotating spread shots
- Spinning attacks
- Dynamic pattern variations

##### OscillatingPattern

Oscillates a parameter (like spread angle) over time.

```csharp
public sealed class OscillatingPattern : IBulletPattern
{
    private readonly IBulletPattern _pattern;
    private readonly float _frequency;
    private readonly float _amplitude;
    private readonly Func<PatternContext, float, float> _parameterModifier;
    
    public IBulletPattern Pattern { get; }
    public float Frequency { get; }
    public float Amplitude { get; }
    public float Duration => _pattern.Duration;
    public bool IsLooping => _pattern.IsLooping;
    
    // Example: Oscillate spread angle
    public OscillatingPattern(
        IBulletPattern pattern, 
        float frequency, 
        float amplitude,
        Func<PatternContext, float, float> parameterModifier)
    {
        _pattern = pattern;
        _frequency = frequency;
        _amplitude = amplitude;
        _parameterModifier = parameterModifier;
    }
    
    // Implementation would modify context metadata before passing to pattern
}
```

**Use Cases:**
- Pulsing spread patterns
- Breathing attacks
- Dynamic pattern adjustments

## API Design

### Primary API: Factory Methods

The main entry point is the `Patterns` static class with factory methods:

```csharp
public static class Patterns
{
    // Basic Patterns
    public static IBulletPattern Single(Vector2 direction, float speed);
    public static IBulletPattern Burst(int count, Vector2 direction, float speed, float delay);
    public static IBulletPattern Spread(int count, float angleSpread, float speed, float baseAngle = 0f);
    public static IBulletPattern Ring(int count, float speed, float startAngle = 0f);
    public static IBulletPattern Aimed(int count, float speed, float spreadAngle = 0f);
    
    // Advanced Patterns
    public static IBulletPattern Spiral(
        int bulletsPerRevolution, 
        float totalRevolutions, 
        float speed, 
        float rotationSpeed = 360f,
        float startAngle = 0f,
        bool looping = false);
    
    public static IBulletPattern Wave(
        int bulletCount, 
        float baseDirection, 
        float waveAmplitude, 
        float waveFrequency, 
        float speed);
    
    public static IBulletPattern Homing(float speed, float turnRate = 90f);
    
    // Composite Patterns
    public static IBulletPattern Sequence(params IBulletPattern[] patterns);
    public static IBulletPattern Sequence(bool looping, params IBulletPattern[] patterns);
    public static IBulletPattern Parallel(params IBulletPattern[] patterns);
    public static IBulletPattern Repeat(IBulletPattern pattern, int count, float delay = 0f);
    public static IBulletPattern Loop(IBulletPattern pattern);
    
    // Modifiers
    public static IBulletPattern Rotating(IBulletPattern pattern, float degreesPerSecond);
    public static IBulletPattern Oscillating(IBulletPattern pattern, float frequency, float amplitude);
}
```

### Usage Examples

#### Simple Single Shot
```csharp
var pattern = Patterns.Single(new Vector2(1, 0), speed: 200f);

float lastTime = 0f;
foreach (var spawn in pattern.GetSpawns(lastTime, 0.1f, new PatternContext { Origin = enemyPos }))
{
    CreateBullet(spawn.Position, spawn.Direction, spawn.Speed);
}
```

#### Complex Boss Pattern
```csharp
var phase1 = Patterns.Ring(8, speed: 150f);
var phase2 = Patterns.Spiral(bulletsPerRevolution: 12, totalRevolutions: 2, speed: 180f);
var phase3 = Patterns.Spread(count: 5, angleSpread: 45f, speed: 200f);

var bossAttack = Patterns.Sequence(
    phase1,
    phase2,
    phase3
).Loop(); // Repeat indefinitely
```

#### Layered Attack
```csharp
var ring = Patterns.Ring(8, speed: 150f);
var spiral = Patterns.Spiral(12, 2, speed: 180f, looping: true);
var rotatingSpread = Patterns.Rotating(
    Patterns.Spread(3, 30f, speed: 200f),
    rotationSpeed: 90f // 90 degrees per second
);

// All three patterns execute simultaneously
var layeredAttack = Patterns.Parallel(ring, spiral, rotatingSpread);
```

#### Rotating Spiral
```csharp
var baseSpiral = Patterns.Spiral(8, 3, speed: 150f);
var rotatingSpiral = Patterns.Rotating(baseSpiral, rotationSpeed: 45f); // Slow rotation
```

### Optional Fluent Builder API

For complex compositions, an optional fluent builder is available:

```csharp
public class PatternBuilder
{
    public PatternBuilder Ring(int count, float speed, float startAngle = 0f);
    public PatternBuilder Spread(int count, float angleSpread, float speed, float baseAngle = 0f);
    public PatternBuilder Spiral(int bulletsPerRev, float revs, float speed);
    // ... other pattern methods
    
    public PatternBuilder Then(); // Sequence next pattern
    public PatternBuilder And(); // Parallel next pattern
    public PatternBuilder Repeat(int count, float delay = 0f);
    public PatternBuilder Loop();
    public PatternBuilder Rotating(float degreesPerSecond);
    
    public IBulletPattern Build();
}

// Usage:
var complexPattern = PatternBuilder
    .Start()
    .Ring(8, 200)
    .Then()
    .Spiral(12, 2, 150)
    .Then()
    .Spread(5, 45, 180)
    .Loop()
    .Build();
```

## Integration Guide

### Basic Integration Pattern

The library does not manage bullets - your game code does:

```csharp
public class Enemy
{
    private IBulletPattern _attackPattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    
    public void Initialize()
    {
        _attackPattern = Patterns.Spiral(12, 3, speed: 150f);
        _patternStartTime = 0f; // Set when attack starts
        _lastQueryTime = 0f;
    }
    
    public void Update(float currentGameTime)
    {
        // Only spawn if pattern is active
        if (_patternStartTime <= 0f) return;
        
        float patternTime = currentGameTime - _patternStartTime;
        float lastPatternTime = _lastQueryTime;
        
        // Get spawns since last query
        var spawns = _attackPattern.GetSpawns(
            lastPatternTime, 
            patternTime, 
            new PatternContext 
            { 
                Origin = Position,
                Target = _player?.Position 
            }
        );
        
        // Create bullets in your game
        foreach (var spawn in spawns)
        {
            var bullet = _bulletPool.Get();
            bullet.Initialize(
                spawn.Position,
                spawn.Direction,
                spawn.Speed,
                spawn.Angle
            );
            _bulletSystem.Add(bullet);
        }
        
        _lastQueryTime = patternTime;
    }
    
    public void StartAttack(float gameTime)
    {
        _patternStartTime = gameTime;
        _lastQueryTime = 0f;
    }
}
```

### With ECS (Arch/MonoGame.Extended.Entities)

```csharp
public class BulletPatternSystem : ISystem
{
    public void Update(World world, float deltaTime)
    {
        var query = new QueryDescription().WithAll<BulletPatternComponent, Position>();
        
        world.Query(in query, (ref BulletPatternComponent pattern, ref Position pos) =>
        {
            pattern.ElapsedTime += deltaTime;
            
            var context = new PatternContext
            {
                Origin = new Vector2(pos.X, pos.Y),
                PatternAge = pattern.ElapsedTime
            };
            
            var spawns = pattern.Pattern.GetSpawns(
                pattern.LastQueryTime,
                pattern.ElapsedTime,
                context
            );
            
            foreach (var spawn in spawns)
            {
                // Spawn bullet entity
                world.Create(
                    new Position { X = spawn.Position.X, Y = spawn.Position.Y },
                    new Velocity 
                    { 
                        X = spawn.Direction.X * spawn.Speed, 
                        Y = spawn.Direction.Y * spawn.Speed 
                    },
                    new Sprite { /* ... */ }
                );
            }
            
            pattern.LastQueryTime = pattern.ElapsedTime;
        });
    }
}
```

### Framework Conversion Helpers

```csharp
public static class VectorExtensions
{
    // MonoGame
    public static Microsoft.Xna.Framework.Vector2 ToXna(this Vector2 v) 
        => new(v.X, v.Y);
    
    public static Vector2 FromXna(Microsoft.Xna.Framework.Vector2 v) 
        => new(v.X, v.Y);
    
    // Unity
    public static UnityEngine.Vector2 ToUnity(this Vector2 v) 
        => new(v.X, v.Y);
    
    public static Vector2 FromUnity(UnityEngine.Vector2 v) 
        => new(v.x, v.y);
}
```

## Performance Considerations

### Allocation-Free Future Design

The current API uses `IEnumerable<BulletSpawn>` which can allocate. Future versions will add allocation-free overloads:

```csharp
public interface IBulletPattern
{
    // Current (Tier 2 - Convenience)
    IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context);
    
    // Future (Tier 1 - Performance)
    int GetSpawns(float lastTime, float currentTime, PatternContext context, Span<BulletSpawn> buffer);
    
    // Or using ref struct enumerator
    BulletSpanEnumerator GetSpawns(float lastTime, float currentTime, PatternContext context);
}

public ref struct BulletSpanEnumerator
{
    // Allocation-free iteration
    public BulletSpawn Current { get; }
    public bool MoveNext();
}
```

### Optimization Strategies

1. **Pool Pattern Instances**: Create patterns once, reuse many times
2. **Batch Queries**: Query multiple pattern instances together if possible
3. **Time Quantization**: Round time to fixed intervals to reduce computation
4. **Early Exit**: Patterns should exit early if no spawns possible in time range

## Package Structure

```
BenedictBulletHell.Patterns/
├── BenedictBulletHell.Patterns.csproj
├── README.md
├── DESIGN.md (this file)
│
├── Core/
│   ├── IBulletPattern.cs
│   ├── BulletSpawn.cs
│   ├── PatternContext.cs
│   └── BulletSpanEnumerator.cs (future)
│
├── Patterns/
│   ├── Basic/
│   │   ├── SingleShotPattern.cs
│   │   ├── BurstPattern.cs
│   │   ├── SpreadPattern.cs
│   │   ├── RingPattern.cs
│   │   └── AimedPattern.cs
│   │
│   ├── Advanced/
│   │   ├── SpiralPattern.cs
│   │   ├── WavePattern.cs
│   │   └── HomingPattern.cs
│   │
│   ├── Composite/
│   │   ├── SequencePattern.cs
│   │   ├── ParallelPattern.cs
│   │   ├── RepeatPattern.cs
│   │   └── LoopPattern.cs
│   │
│   └── Modifiers/
│       ├── RotatingPattern.cs
│       └── OscillatingPattern.cs
│
├── Builders/
│   └── PatternBuilder.cs
│
├── Extensions/
│   ├── PatternExtensions.cs
│   └── VectorExtensions.cs
│
└── Patterns.cs (factory class - main API entry point)
```

## Implementation Phases

### Phase 1: MVP (Core Functionality)
- [ ] Core interfaces and types (`IBulletPattern`, `BulletSpawn`, `PatternContext`)
- [ ] Basic patterns: `SingleShot`, `Spread`, `Ring`, `Burst`
- [ ] Composite patterns: `Sequence`, `Parallel`
- [ ] Factory API (`Patterns` static class)
- [ ] Basic unit tests
- [ ] README with examples

### Phase 2: Advanced Features
- [ ] Advanced patterns: `Spiral`, `Wave`, `Aimed`
- [ ] Modifiers: `Rotating`, `Oscillating`
- [ ] Composite patterns: `Repeat`, `Loop`
- [ ] Fluent builder API (`PatternBuilder`)
- [ ] Comprehensive unit tests
- [ ] Integration examples

### Phase 3: Performance & Polish
- [ ] `HomingPattern` (with proper documentation)
- [ ] Allocation-free `Span<BulletSpawn>` overloads
- [ ] Performance benchmarks
- [ ] XML documentation for all public APIs
- [ ] NuGet package preparation
- [ ] Sample project demonstrating usage

## Design Decisions Summary

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Math Library | `System.Numerics.Vector2` | Standard, hardware-accelerated, framework-agnostic |
| Time Model | Absolute time with delta detection | Prevents duplicate spawns, predictable behavior |
| Pattern Mutability | Immutable patterns, mutable context | Thread-safe, predictable, cacheable |
| Pattern State | Stateless computation | Reusable, testable, no synchronization needed |
| Composition | Support deep nesting with optional flattening | Flexibility with performance optimization path |
| Performance | Two-tier API (allocation-free + convenience) | Performance when needed, convenience when prototyping |
| Framework Coupling | Completely decoupled | Works with any .NET game framework |

## Testing Strategy

### Unit Tests

Each pattern type should have comprehensive unit tests:

```csharp
[Test]
public void RingPattern_GeneratesCorrectBulletCount()
{
    var pattern = Patterns.Ring(count: 8, speed: 100f);
    var context = new PatternContext { Origin = Vector2.Zero };
    
    var spawns = pattern.GetSpawns(0f, 0f, context).ToList();
    
    Assert.AreEqual(8, spawns.Count);
    // Verify all directions are evenly spaced
    // Verify all speeds are correct
}

[Test]
public void SpiralPattern_LoopsCorrectly()
{
    var pattern = Patterns.Spiral(8, 1, 100f, looping: true);
    // Test that pattern loops at correct intervals
}

[Test]
public void SequencePattern_ExecutesPatternsInOrder()
{
    // Verify patterns execute sequentially
}
```

### Integration Tests

Test real-world usage scenarios:
- Boss fight pattern sequences
- Complex nested compositions
- Performance under load (1000+ bullets)

## Future Enhancements

Potential additions (post-MVP):

1. **Pattern Serialization**: Save/load patterns from JSON/YAML
2. **Pattern Editor**: Visual tool for designing patterns
3. **Curved Patterns**: Bezier curves, splines for bullet paths
4. **Adaptive Patterns**: Patterns that adjust difficulty based on player skill
5. **Pattern Libraries**: Pre-built pattern collections
6. **Physics Integration**: Optional hooks for physics engine integration
7. **Pattern Events**: Callbacks for pattern start/end/completion

## License & Distribution

- **Target Framework**: .NET Standard 2.1 (broad compatibility)
- **Dependencies**: None (uses only .NET Standard libraries)
- **License**: TBD (MIT recommended for maximum adoption)

## Conclusion

This design provides a clean, composable, performant foundation for bullet pattern generation in C# games. The separation of pattern definition from bullet management keeps the library focused and framework-agnostic, while the composable architecture enables complex behaviors from simple building blocks.


