# Getting Started

This guide will walk you through creating your first bullet pattern in 5 simple steps.

## Step 1: Install the Package

Add the package to your project:

```bash
dotnet add package BenedictBulletHell.Patterns
```

Or add to your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="BenedictBulletHell.Patterns" Version="1.0.0" />
</ItemGroup>
```

## Step 2: Add Using Statements

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using System.Numerics;
```

## Step 3: Create a Pattern

Start with a simple pattern. Let's create a ring pattern that fires 8 bullets in all directions:

```csharp
var ringPattern = Pattern.Ring(count: 8, speed: 150f);
```

**Key points:**
- `count` - Number of bullets in the ring
- `speed` - Bullet speed in units per second
- Patterns are immutable - create once, reuse everywhere

See [Patterns](Patterns) for all available pattern types.

## Step 4: Set Up Timing

Patterns are time-based. You need to track:
- `lastTime` - When you last queried the pattern (0 for first call)
- `currentTime` - Current time relative to pattern start

```csharp
float lastTime = 0f;
float currentTime = 0.1f; // 100ms elapsed since pattern started
```

**Important:** Use absolute time, not frame deltas. This prevents duplicate spawns if you query multiple times.

## Step 5: Query and Create Bullets

Create a `PatternContext` and query for spawns:

```csharp
var context = new PatternContext
{
    Origin = new Vector2(400, 300), // Where pattern originates (boss position)
    Target = new Vector2(200, 100)  // Optional: target for aimed patterns
};

foreach (var spawn in ringPattern.GetSpawns(lastTime, currentTime, context))
{
    // Create your bullet entity
    CreateBullet(spawn.Position, spawn.Direction, spawn.Speed);
}

// Update lastTime for next frame
lastTime = currentTime;
```

## Complete Example

Here's a complete, runnable example:

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using System.Numerics;

class Program
{
    static void Main()
    {
        // Create pattern
        var ringPattern = Pattern.Ring(count: 8, speed: 150f);
        
        // Timing
        float lastTime = 0f;
        float currentTime = 0.1f;
        
        // Context
        var context = new PatternContext
        {
            Origin = new Vector2(400, 300),
            Target = new Vector2(200, 100)
        };
        
        // Get spawns
        foreach (var spawn in ringPattern.GetSpawns(lastTime, currentTime, context))
        {
            Console.WriteLine($"Spawn at ({spawn.Position.X}, {spawn.Position.Y}) " +
                            $"direction ({spawn.Direction.X}, {spawn.Direction.Y}) " +
                            $"speed {spawn.Speed}");
        }
    }
}
```

## Game Loop Integration

Here's how to integrate into a typical game loop:

```csharp
public class Enemy
{
    private IBulletPattern _attackPattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private bool _isAttacking;
    
    public void Initialize()
    {
        // Create pattern once
        _attackPattern = Pattern.Ring(8, speed: 150f);
        _isAttacking = false;
    }
    
    public void Update(float currentGameTime)
    {
        if (!_isAttacking) return;
        
        // Calculate pattern-relative time
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
        
        // Update tracking
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

## Common Patterns

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

## Next Steps

- **[Patterns](Patterns)** - Learn about all available pattern types
- **[Examples](Examples)** - See more complete examples
- **[Integration](Integration)** - Framework-specific integration guides
- **[Best Practices](Best-Practices)** - Tips for effective pattern creation

## Common Issues

If you get errors, check:
1. **Missing spawns**: Make sure `lastTime` is correctly tracked (see [Troubleshooting](Troubleshooting))
2. **Duplicate spawns**: Don't reset `lastTime` between frames
3. **No bullets appearing**: Check that `currentTime > lastTime`

See [Troubleshooting](Troubleshooting) for solutions to common problems.

