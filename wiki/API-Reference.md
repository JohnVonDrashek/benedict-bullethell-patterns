# API Reference

Quick reference for main classes and methods.

## Pattern (Factory Class)

Main entry point for creating bullet patterns.

### Static Methods

#### Basic Patterns

```csharp
public static IBulletPattern Single(Vector2 direction, float speed)
public static IBulletPattern Burst(int count, Vector2 direction, float speed, float delay)
public static IBulletPattern Spread(int count, float angleSpread, float speed, float baseAngle = 0f)
public static IBulletPattern Ring(int count, float speed, float startAngle = 0f)
```

#### Advanced Patterns

```csharp
public static IBulletPattern Spiral(
    int bulletsPerRevolution,
    float totalRevolutions,
    float speed,
    float rotationSpeed = 360f,
    float startAngle = 0f,
    bool looping = false)

public static IBulletPattern Aimed(int count, float speed, float spreadAngle = 0f)

public static IBulletPattern Wave(
    int bulletCount,
    float baseDirection,
    float waveAmplitude,
    float waveFrequency,
    float speed)
```

#### Composite Patterns

```csharp
public static IBulletPattern Sequence(params IBulletPattern[] patterns)
public static IBulletPattern Sequence(bool looping, params IBulletPattern[] patterns)
public static IBulletPattern Parallel(params IBulletPattern[] patterns)
public static IBulletPattern Loop(IBulletPattern pattern)
public static IBulletPattern Repeat(IBulletPattern pattern, int count, float delay = 0f)
```

#### Modifier Patterns

```csharp
public static IBulletPattern Rotating(IBulletPattern pattern, float degreesPerSecond)
```

---

## IBulletPattern

The fundamental interface for all bullet patterns.

### Properties

```csharp
float Duration { get; }
bool IsLooping { get; }
```

### Methods

```csharp
IEnumerable<BulletSpawn> GetSpawns(float lastTime, float currentTime, PatternContext context)
```

**Parameters:**
- `lastTime` - The last time this pattern was queried (or start time for first call)
- `currentTime` - The current time
- `context` - Contextual information (origin, target, etc.)

**Returns:** `IEnumerable<BulletSpawn>` - Bullet spawn instructions

---

## BulletSpawn

A value type representing a single bullet spawn instruction.

### Properties

```csharp
public Vector2 Position { get; set; }
public Vector2 Direction { get; set; }
public float Speed { get; set; }
public float Angle { get; set; }
public object? BulletData { get; set; }
```

**Fields:**
- `Position` - World position where the bullet should spawn
- `Direction` - Initial direction vector (should be normalized)
- `Speed` - Initial speed in units per second
- `Angle` - Initial rotation angle in degrees (useful for sprite orientation)
- `BulletData` - Optional custom data specific to bullet type or game logic

---

## PatternContext

Contextual information passed to patterns at execution time.

### Properties

```csharp
public Vector2 Origin { get; set; }
public Vector2? Target { get; set; }
public float PatternAge { get; set; }
public IReadOnlyDictionary<string, object>? Metadata { get; set; }
```

**Fields:**
- `Origin` - World position where the pattern originates (typically enemy/boss position)
- `Target` - Optional target position (used by aimed/homing patterns)
- `PatternAge` - How long this specific pattern instance has been running (useful for loops)
- `Metadata` - Optional runtime parameter overrides (speed multiplier, angle offset, etc.)

### Methods

```csharp
public T? GetMetadata<T>(string key) where T : class
public T GetMetadataValue<T>(string key, T defaultValue = default) where T : struct
```

**GetMetadata:**
- Gets a typed metadata value, or default if not present
- Returns `null` for reference types if key not found

**GetMetadataValue:**
- Gets a typed metadata value, or default value type if not present
- Returns `defaultValue` if key not found

**Example:**
```csharp
var context = new PatternContext
{
    Origin = new Vector2(400, 300),
    Metadata = new Dictionary<string, object>
    {
        ["SpeedMultiplier"] = 1.5f,
        ["AngleOffset"] = 15f
    }
};

float speedMultiplier = context.GetMetadataValue<float>("SpeedMultiplier", 1.0f);
float angleOffset = context.GetMetadataValue<float>("AngleOffset", 0f);
```

---

## VectorExtensions

Extension methods for vector conversion between frameworks.

### MonoGame / FNA

```csharp
public static Microsoft.Xna.Framework.Vector2 ToXna(this Vector2 vector)
public static Vector2 FromXna(Microsoft.Xna.Framework.Vector2 vector)
```

### Unity

```csharp
public static UnityEngine.Vector2 ToUnity(this Vector2 vector)
public static Vector2 FromUnity(UnityEngine.Vector2 vector)
```

**Usage:**
```csharp
using BenedictBulletHell.Patterns.Extensions;

// Convert to MonoGame/FNA
var xnaVector = spawn.Position.ToXna();

// Convert to Unity
var unityVector = spawn.Position.ToUnity();
```

---

## Pattern Types

### Basic Patterns

#### SingleShotPattern
```csharp
public sealed class SingleShotPattern : IBulletPattern
{
    public Vector2 Direction { get; }
    public float Speed { get; }
    public float Duration { get; }
    public bool IsLooping => false;
}
```

#### BurstPattern
```csharp
public sealed class BurstPattern : IBulletPattern
{
    public int BulletCount { get; }
    public Vector2 Direction { get; }
    public float Speed { get; }
    public float DelayBetweenShots { get; }
    public float Duration { get; }
    public bool IsLooping => false;
}
```

#### SpreadPattern
```csharp
public sealed class SpreadPattern : IBulletPattern
{
    public int BulletCount { get; }
    public float AngleSpread { get; }
    public float Speed { get; }
    public float BaseAngle { get; }
    public float Duration => 0f;
    public bool IsLooping => false;
}
```

#### RingPattern
```csharp
public sealed class RingPattern : IBulletPattern
{
    public int BulletCount { get; }
    public float Speed { get; }
    public float StartAngle { get; }
    public float Duration => 0f;
    public bool IsLooping => false;
}
```

### Advanced Patterns

#### SpiralPattern
```csharp
public sealed class SpiralPattern : IBulletPattern
{
    public int BulletsPerRevolution { get; }
    public float TotalRevolutions { get; }
    public float Speed { get; }
    public float RotationSpeed { get; }
    public float StartAngle { get; }
    public bool Looping { get; }
    public float Duration { get; }
    public bool IsLooping { get; }
}
```

#### AimedPattern
```csharp
public sealed class AimedPattern : IBulletPattern
{
    public int BulletCount { get; }
    public float Speed { get; }
    public float SpreadAngle { get; }
    public float Duration => 0f;
    public bool IsLooping => false;
}
```

#### WavePattern
```csharp
public sealed class WavePattern : IBulletPattern
{
    public int BulletCount { get; }
    public float BaseDirection { get; }
    public float WaveAmplitude { get; }
    public float WaveFrequency { get; }
    public float Speed { get; }
    public float Duration => 0f;
    public bool IsLooping => false;
}
```

### Composite Patterns

#### SequencePattern
```csharp
public sealed class SequencePattern : IBulletPattern
{
    public IReadOnlyList<IBulletPattern> Patterns { get; }
    public bool Looping { get; }
    public float Duration { get; }
    public bool IsLooping { get; }
}
```

#### ParallelPattern
```csharp
public sealed class ParallelPattern : IBulletPattern
{
    public IReadOnlyList<IBulletPattern> Patterns { get; }
    public float Duration { get; }
    public bool IsLooping { get; }
}
```

#### LoopPattern
```csharp
public sealed class LoopPattern : IBulletPattern
{
    public IBulletPattern Pattern { get; }
    public float Duration => float.PositiveInfinity;
    public bool IsLooping => true;
}
```

#### RepeatPattern
```csharp
public sealed class RepeatPattern : IBulletPattern
{
    public IBulletPattern Pattern { get; }
    public int RepeatCount { get; }
    public float Delay { get; }
    public float Duration { get; }
    public bool IsLooping => false;
}
```

### Modifier Patterns

#### RotatingPattern
```csharp
public sealed class RotatingPattern : IBulletPattern
{
    public IBulletPattern Pattern { get; }
    public float DegreesPerSecond { get; }
    public float Duration { get; }
    public bool IsLooping { get; }
}
```

---

## Exceptions

The library may throw the following exceptions:

- **`ArgumentNullException`** - When required parameters are null
- **`ArgumentException`** - When parameters are invalid (e.g., negative counts, speeds)

---

## Namespaces

- `BenedictBulletHell.Patterns` - Main namespace, contains `Pattern` factory class
- `BenedictBulletHell.Patterns.Core` - Core interfaces and types (`IBulletPattern`, `BulletSpawn`, `PatternContext`)
- `BenedictBulletHell.Patterns.Extensions` - Framework integration extensions
- `BenedictBulletHell.Patterns.Patterns.Basic` - Basic pattern implementations
- `BenedictBulletHell.Patterns.Patterns.Advanced` - Advanced pattern implementations
- `BenedictBulletHell.Patterns.Patterns.Composite` - Composite pattern implementations
- `BenedictBulletHell.Patterns.Patterns.Modifiers` - Modifier pattern implementations

---

## See Also

- **[Patterns](Patterns)** - Detailed guide to all pattern types
- **[Examples](Examples)** - Usage examples
- **[Getting Started](Getting-Started)** - Quick start guide

