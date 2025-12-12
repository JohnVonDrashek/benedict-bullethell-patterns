# BenedictBulletHell.Patterns

A framework-agnostic C# library for defining and executing bullet hell pattern generation. Create complex bullet patterns with a clean, composable API while remaining completely decoupled from rendering, physics, and game engine concerns.

## Features

- **Framework Agnostic**: Works with MonoGame, FNA, Unity, or any .NET game framework
- **Composable**: Simple patterns combine into complex behaviors
- **Time-Based**: All patterns are driven by absolute time, preventing duplicate spawns
- **Stateless & Immutable**: Patterns are predictable, thread-safe, and cacheable
- **Zero Dependencies**: Uses only .NET Standard 2.1 libraries

## Installation

```bash
dotnet add package BenedictBulletHell.Patterns
```

## Quick Start

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using System.Numerics;

// Create a simple ring pattern
var ringPattern = Pattern.Ring(count: 8, speed: 150f);

// Use it in your game loop
float lastTime = 0f;
float currentTime = 0.1f; // 100ms elapsed

var context = new PatternContext
{
    Origin = new Vector2(400, 300), // Boss position
    Target = new Vector2(200, 100)  // Optional: player position
};

foreach (var spawn in ringPattern.GetSpawns(lastTime, currentTime, context))
{
    // Create your bullet entity
    CreateBullet(spawn.Position, spawn.Direction, spawn.Speed);
}
```

## Basic Patterns

### Single Shot
```csharp
var single = Pattern.Single(
    direction: new Vector2(1, 0), 
    speed: 200f
);
```

### Burst
```csharp
var burst = Pattern.Burst(
    count: 5,
    direction: new Vector2(0, -1),
    speed: 180f,
    delay: 0.1f // 100ms between shots
);
```

### Spread
```csharp
var spread = Pattern.Spread(
    count: 5,
    angleSpread: 45f, // 45-degree fan
    speed: 200f,
    baseAngle: 90f // Pointing up
);
```

### Ring
```csharp
var ring = Pattern.Ring(
    count: 8,
    speed: 150f,
    startAngle: 0f // Start at 0 degrees (right)
);
```

## Advanced Patterns

### Spiral
```csharp
var spiral = Pattern.Spiral(
    bulletsPerRevolution: 12,
    totalRevolutions: 3,
    speed: 150f,
    rotationSpeed: 360f, // degrees per second
    startAngle: 0f,
    looping: false
);
```

### Aimed
```csharp
var aimed = Pattern.Aimed(
    count: 1,
    speed: 200f,
    spreadAngle: 5f // Optional spread for multiple bullets
);
```

### Wave
```csharp
var wave = Pattern.Wave(
    bulletCount: 10,
    baseDirection: 90f, // Pointing up
    waveAmplitude: 30f, // 30-degree side-to-side swing
    waveFrequency: 2f,  // 2 complete cycles across all bullets
    speed: 180f
);
```

## Composite Patterns

### Sequence
Execute patterns one after another:

```csharp
var phase1 = Pattern.Ring(8, 150f);
var phase2 = Pattern.Spread(5, 45f, 180f);
var phase3 = Pattern.Burst(10, new Vector2(1, 0), 200f, 0.05f);

var sequence = Pattern.Sequence(phase1, phase2, phase3);
```

### Parallel
Execute patterns simultaneously:

```csharp
var ring = Pattern.Ring(8, 150f);
var spread = Pattern.Spread(5, 30f, 180f);

var layered = Pattern.Parallel(ring, spread);
```

### Looping Sequences
```csharp
var looped = Pattern.Sequence(looping: true, 
    Pattern.Ring(8, 150f),
    Pattern.Spread(5, 45f, 180f)
);
```

### Loop (Individual Pattern)
```csharp
var ring = Pattern.Ring(8, 150f);
var loopingRing = Pattern.Loop(ring); // Loops forever
```

### Repeat
```csharp
var burst = Pattern.Burst(5, new Vector2(0, -1), 200f, 0.1f);
var repeatedBurst = Pattern.Repeat(burst, count: 3, delay: 0.5f);
```

## Modifier Patterns

### Rotating
Rotate any pattern around the origin over time:

```csharp
var spread = Pattern.Spread(5, 30f, 180f);
var rotatingSpread = Pattern.Rotating(spread, degreesPerSecond: 90f);
```

## Integration Example

Here's a complete example of integrating with a game:

```csharp
public class Enemy
{
    private IBulletPattern _attackPattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private bool _isAttacking;
    
    public void Initialize()
    {
        // Create a complex boss attack pattern
        var phase1 = Pattern.Ring(8, speed: 150f);
        var phase2 = Pattern.Spread(5, angleSpread: 45f, speed: 180f);
        
        _attackPattern = Pattern.Sequence(phase1, phase2);
        _isAttacking = false;
    }
    
    public void Update(float currentGameTime)
    {
        if (!_isAttacking) return;
        
        float patternTime = currentGameTime - _patternStartTime;
        
        // Get spawns since last query
        var spawns = _attackPattern.GetSpawns(
            _lastQueryTime, 
            patternTime, 
            new PatternContext 
            { 
                Origin = Position,
                Target = _player?.Position 
            }
        );
        
        // Create bullets
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
        
        // Check if pattern is complete
        if (patternTime >= _attackPattern.Duration && !_attackPattern.IsLooping)
        {
            _isAttacking = false;
        }
    }
    
    public void StartAttack(float gameTime)
    {
        _patternStartTime = gameTime;
        _lastQueryTime = 0f;
        _isAttacking = true;
    }
}
```

## Pattern Context

The `PatternContext` struct provides runtime information to patterns:

```csharp
var context = new PatternContext
{
    Origin = new Vector2(400, 300),      // Where pattern originates
    Target = new Vector2(200, 100),      // Optional target for aimed patterns
    PatternAge = 2.5f,                    // How long pattern has been running
    Metadata = new Dictionary<string, object>
    {
        ["SpeedMultiplier"] = 1.5f,      // Custom runtime parameters
        ["AngleOffset"] = 15f
    }
};
```

## Bullet Spawn

Each spawn instruction contains:

```csharp
public struct BulletSpawn
{
    public Vector2 Position;      // Where to spawn
    public Vector2 Direction;      // Initial direction (normalized)
    public float Speed;            // Initial speed
    public float Angle;            // Initial rotation (degrees)
    public object? BulletData;     // Optional custom data
}
```

## Framework Integration

### MonoGame / FNA
```csharp
using BenedictBulletHell.Patterns.Extensions;

// Convert vectors
var xnaVector = spawn.Position.ToXna();
var numericsVector = VectorExtensions.FromXna(xnaVector);
```

### Unity
```csharp
using BenedictBulletHell.Patterns.Extensions;

// Convert vectors
var unityVector = spawn.Position.ToUnity();
var numericsVector = VectorExtensions.FromUnity(unityVector);
```

## Performance Tips

1. **Reuse Patterns**: Create patterns once and reuse them across multiple enemies
2. **Pool Pattern Instances**: Patterns are immutable, so cache them
3. **Batch Queries**: Query multiple pattern instances together when possible
4. **Time Management**: Track `lastTime` carefully to avoid duplicate spawns

## Architecture

This library is **pattern definition only** - it doesn't manage bullets, collisions, or rendering. Your game code handles:
- Bullet entity creation
- Movement/physics updates
- Collision detection
- Rendering
- Lifetime management

The library only defines **when** and **how** bullets should spawn.

## License

[To be determined - MIT recommended]

## Contributing

This is a work in progress. See [DESIGN.md](DESIGN.md) for the full design document.

