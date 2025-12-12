# Core Concepts

Understanding the fundamental ideas behind BenedictBulletHell.Patterns.

## Time-Based Execution

Patterns are **time-based**, not frame-based. This means:

- Patterns use absolute time, not frame deltas
- Same time input = same output (deterministic)
- Frame-rate independent
- Prevents duplicate spawns

### How It Works

```csharp
// Pattern is queried with time range
var spawns = pattern.GetSpawns(lastTime, currentTime, context);

// Pattern returns all spawns that should occur in that time range
// If you query again with the same range, you get the same spawns
// If you query with a new range, you get new spawns
```

### Example

```csharp
float lastTime = 0f;
float currentTime = 0.1f; // 100ms

// First query: gets spawns from 0 to 0.1 seconds
var spawns1 = pattern.GetSpawns(0f, 0.1f, context);

// Next frame: currentTime = 0.2f
// Query: gets spawns from 0.1 to 0.2 seconds (no duplicates!)
var spawns2 = pattern.GetSpawns(0.1f, 0.2f, context);
```

## Stateless Patterns

Patterns are **stateless** - they don't store any execution state:

- Same pattern instance can be used by multiple enemies
- Patterns are thread-safe
- No side effects from querying
- Completely predictable

### Benefits

```csharp
// Create pattern once
var ringPattern = Pattern.Ring(8, 150f);

// Use it for multiple enemies
var enemy1 = new Enemy(ringPattern);
var enemy2 = new Enemy(ringPattern);
var enemy3 = new Enemy(ringPattern);

// All enemies share the same pattern instance
// No conflicts, no state issues
```

## Immutability

Patterns are **immutable** - once created, they can't be changed:

- Safe to cache and reuse
- No need to clone patterns
- Thread-safe by default
- Predictable behavior

```csharp
// Pattern is created and never changes
var pattern = Pattern.Ring(8, 150f);

// You can't modify it (no setters)
// You can only query it
var spawns = pattern.GetSpawns(...);
```

## Pattern Context

The `PatternContext` provides **runtime information** to patterns:

- `Origin` - Where the pattern originates (required)
- `Target` - Optional target for aimed patterns
- `PatternAge` - How long the pattern has been running
- `Metadata` - Custom runtime parameters

### Why Context?

Patterns are immutable, but they need runtime data (like enemy position). Context provides this without mutating the pattern:

```csharp
// Pattern is created once (immutable)
var pattern = Pattern.Ring(8, 150f);

// Context provides runtime data each query
var context1 = new PatternContext { Origin = enemy1.Position };
var context2 = new PatternContext { Origin = enemy2.Position };

// Same pattern, different origins
var spawns1 = pattern.GetSpawns(0f, 0f, context1);
var spawns2 = pattern.GetSpawns(0f, 0f, context2);
```

## Bullet Spawn Instructions

Patterns don't create bullets - they generate **spawn instructions**:

- `Position` - Where to spawn
- `Direction` - Initial direction (normalized)
- `Speed` - Initial speed
- `Angle` - Initial rotation
- `BulletData` - Optional custom data

### Separation of Concerns

```csharp
// Pattern generates instructions
var spawns = pattern.GetSpawns(...);

// Your game code creates actual bullets
foreach (var spawn in spawns)
{
    var bullet = new Bullet();
    bullet.Position = spawn.Position;
    bullet.Velocity = spawn.Direction * spawn.Speed;
    bullet.Rotation = spawn.Angle;
    // Add to game world
}
```

## Pattern Composition

Patterns are **composable** - simple patterns combine into complex ones:

### Basic Composition

```csharp
// Simple pattern
var ring = Pattern.Ring(8, 150f);

// Compose into complex pattern
var complex = Pattern.Sequence(
    ring,
    Pattern.Spread(5, 45f, 200f),
    Pattern.Spiral(12, 2f, 150f)
);
```

### Modifier Composition

```csharp
// Base pattern
var spread = Pattern.Spread(5, 30f, 200f);

// Add modifier
var rotating = Pattern.Rotating(spread, 90f);

// Compose further
var repeated = Pattern.Repeat(rotating, 3, 0.5f);
```

## Pattern Duration

Every pattern has a **duration**:

- Non-looping patterns: finite duration
- Looping patterns: `float.PositiveInfinity`
- Composite patterns: calculated from children

### Using Duration

```csharp
var pattern = Pattern.Burst(5, Vector2.UnitY, 200f, delay: 0.1f);

// Duration is (5 - 1) * 0.1 = 0.4 seconds
float duration = pattern.Duration;

// Check if pattern is complete
if (patternTime >= pattern.Duration && !pattern.IsLooping)
{
    // Pattern finished
}
```

## Pattern Looping

Some patterns can **loop indefinitely**:

- `Loop` pattern: wraps any pattern in infinite loop
- `Sequence` with `looping: true`: loops the sequence
- `IsLooping` property: indicates if pattern loops

### Looping Behavior

```csharp
var ring = Pattern.Ring(8, 150f);
var looping = Pattern.Loop(ring);

// Looping pattern never completes naturally
bool isLooping = looping.IsLooping; // true
float duration = looping.Duration; // float.PositiveInfinity

// You must manually stop it
if (shouldStop)
{
    // Stop the pattern
}
```

## Query Semantics

Understanding how `GetSpawns()` works:

### Time Range

```csharp
// Query spawns in time range [lastTime, currentTime)
var spawns = pattern.GetSpawns(lastTime, currentTime, context);

// Returns all spawns that should occur in this range
// If lastTime == currentTime, returns empty (no time passed)
// If lastTime > currentTime, behavior is undefined (don't do this!)
```

### First Query

```csharp
// First query: lastTime = 0, currentTime = 0.1
var spawns = pattern.GetSpawns(0f, 0.1f, context);
// Gets spawns from start to 0.1 seconds
```

### Subsequent Queries

```csharp
// Next query: lastTime = 0.1, currentTime = 0.2
var spawns = pattern.GetSpawns(0.1f, 0.2f, context);
// Gets spawns from 0.1 to 0.2 seconds (no duplicates!)
```

### Querying Past Duration

```csharp
// If pattern duration is 1.0 seconds
var spawns = pattern.GetSpawns(1.5f, 2.0f, context);
// Returns empty (pattern already finished)
```

## Framework Agnostic Design

The library is **framework-agnostic**:

- Uses `System.Numerics.Vector2` (standard .NET)
- No rendering, physics, or game engine dependencies
- Extension methods for framework conversion
- Works with any .NET game framework

### Why Framework Agnostic?

- **Flexibility**: Use with MonoGame, Unity, FNA, or custom engines
- **Separation**: Pattern logic separate from game logic
- **Testability**: Easy to test without game engine
- **Reusability**: Same patterns work across projects

## Pattern Lifecycle

Understanding pattern lifecycle:

### 1. Creation

```csharp
// Pattern is created (immutable, stateless)
var pattern = Pattern.Ring(8, 150f);
```

### 2. Querying

```csharp
// Pattern is queried repeatedly during execution
var spawns = pattern.GetSpawns(lastTime, currentTime, context);
```

### 3. Completion

```csharp
// Pattern completes when time >= duration (if not looping)
if (patternTime >= pattern.Duration && !pattern.IsLooping)
{
    // Pattern finished
}
```

### 4. Reuse

```csharp
// Pattern can be reused for new attack
patternStartTime = currentGameTime;
lastQueryTime = 0f;
// Query again
```

## Next Steps

- **[Getting Started](Getting-Started)** - Create your first pattern
- **[Patterns](Patterns)** - Learn about all pattern types
- **[Best Practices](Best-Practices)** - Apply these concepts effectively

