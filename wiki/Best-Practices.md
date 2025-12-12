# Best Practices

Tips and patterns for effective bullet pattern creation and integration.

## Pattern Creation

### Reuse Patterns

Patterns are immutable and stateless - create them once and reuse:

```csharp
// ✅ Good: Create once, reuse
private static readonly IBulletPattern RingPattern = Pattern.Ring(8, 150f);

public void Attack()
{
    // Use the same pattern instance
    var spawns = RingPattern.GetSpawns(...);
}

// ❌ Bad: Creating new pattern every time
public void Attack()
{
    var pattern = Pattern.Ring(8, 150f); // Unnecessary allocation
    var spawns = pattern.GetSpawns(...);
}
```

### Cache Complex Patterns

For complex composite patterns, create them during initialization:

```csharp
public class Boss
{
    private readonly IBulletPattern _attackPattern;

    public Boss()
    {
        // Create complex pattern once during initialization
        _attackPattern = Pattern.Sequence(
            Pattern.Ring(16, 150f),
            Pattern.Spread(8, 60f, 200f),
            Pattern.Spiral(12, 2f, 150f, 180f)
        );
    }
}
```

### Use Pattern Composition

Build complex patterns from simple ones:

```csharp
// ✅ Good: Composable
var basePattern = Pattern.Spread(5, 30f, 200f);
var rotating = Pattern.Rotating(basePattern, 90f);
var repeated = Pattern.Repeat(rotating, 3, 0.5f);

// ❌ Bad: Trying to do everything in one pattern
// (Not applicable here, but avoid over-complicating single patterns)
```

## Time Management

### Track Time Correctly

Always use absolute time, not frame deltas:

```csharp
// ✅ Good: Absolute time
private float _patternStartTime;
private float _lastQueryTime;

public void Update(float currentGameTime)
{
    float patternTime = currentGameTime - _patternStartTime;
    var spawns = _pattern.GetSpawns(_lastQueryTime, patternTime, context);
    _lastQueryTime = patternTime;
}

// ❌ Bad: Using frame deltas
private float _accumulatedTime;

public void Update(float deltaTime)
{
    _accumulatedTime += deltaTime;
    var spawns = _pattern.GetSpawns(0f, _accumulatedTime, context);
    // This will cause duplicate spawns!
}
```

### Never Reset lastTime

`lastTime` should only move forward:

```csharp
// ✅ Good: lastTime only increases
_lastQueryTime = patternTime;

// ❌ Bad: Resetting lastTime
_lastQueryTime = 0f; // This causes duplicate spawns!
```

### Handle Frame Rate Independence

Patterns are frame-rate independent by design:

```csharp
// ✅ Good: Works at any frame rate
float patternTime = currentGameTime - _patternStartTime;
var spawns = _pattern.GetSpawns(_lastQueryTime, patternTime, context);

// The pattern will automatically spawn the correct bullets
// regardless of whether you're running at 30fps or 300fps
```

## Performance

### Batch Pattern Queries

Query multiple patterns together when possible:

```csharp
// ✅ Good: Batch queries
foreach (var enemy in _enemies)
{
    var spawns = enemy.Pattern.GetSpawns(
        enemy.LastQueryTime,
        enemy.PatternTime,
        context
    );
    // Process spawns
}

// This is efficient because patterns are stateless
```

### Avoid Unnecessary Allocations

Patterns themselves don't allocate, but be mindful of spawn collections:

```csharp
// ✅ Good: Process spawns immediately
foreach (var spawn in pattern.GetSpawns(...))
{
    CreateBullet(spawn);
}

// ⚠️ Caution: Materializing to list allocates
var spawnList = pattern.GetSpawns(...).ToList(); // Only if needed
```

### Pool Pattern Instances

If you need multiple instances of the same pattern, reuse them:

```csharp
public class PatternPool
{
    private readonly Dictionary<string, IBulletPattern> _patterns = new();

    public IBulletPattern GetPattern(string key, Func<IBulletPattern> factory)
    {
        if (!_patterns.TryGetValue(key, out var pattern))
        {
            pattern = factory();
            _patterns[key] = pattern;
        }
        return pattern;
    }
}
```

## Pattern Design

### Start Simple

Begin with basic patterns and compose:

```csharp
// Start simple
var ring = Pattern.Ring(8, 150f);

// Add complexity gradually
var rotatingRing = Pattern.Rotating(ring, 90f);
var repeatedRing = Pattern.Repeat(rotatingRing, 3, 1f);
```

### Use Meaningful Parameters

Make parameters clear and well-documented:

```csharp
// ✅ Good: Clear parameters
var spread = Pattern.Spread(
    count: 5,
    angleSpread: 45f,  // Clear: 45-degree fan
    speed: 200f,
    baseAngle: 90f     // Clear: pointing up
);

// ❌ Bad: Magic numbers
var spread = Pattern.Spread(5, 45f, 200f, 90f);
```

### Test Pattern Timing

Verify pattern durations and timing:

```csharp
// Check pattern duration
float duration = pattern.Duration;
bool isLooping = pattern.IsLooping;

// Test pattern at different times
var spawns1 = pattern.GetSpawns(0f, 0.1f, context);
var spawns2 = pattern.GetSpawns(0.1f, 0.2f, context);
// Verify no duplicates
```

## Context Usage

### Set Origin Correctly

Always set the pattern origin:

```csharp
// ✅ Good: Origin set to enemy position
var context = new PatternContext
{
    Origin = enemy.Position
};

// ❌ Bad: Missing origin
var context = new PatternContext(); // Bullets spawn at (0, 0)!
```

### Use Target for Aimed Patterns

Set target for patterns that need it:

```csharp
// ✅ Good: Target set for aimed patterns
var context = new PatternContext
{
    Origin = enemy.Position,
    Target = player.Position  // Required for Aimed patterns
};

var aimedPattern = Pattern.Aimed(1, 200f);
var spawns = aimedPattern.GetSpawns(0f, 0f, context);
```

### Leverage Metadata

Use metadata for runtime customization:

```csharp
// ✅ Good: Runtime parameters via metadata
var context = new PatternContext
{
    Origin = position,
    Metadata = new Dictionary<string, object>
    {
        ["SpeedMultiplier"] = difficultyMultiplier,
        ["AngleOffset"] = rotationOffset
    }
};

// Patterns can read metadata if needed
```

## Error Handling

### Validate Pattern Completion

Check if patterns are complete:

```csharp
if (patternTime >= _pattern.Duration && !_pattern.IsLooping)
{
    // Pattern is complete
    OnPatternComplete();
}
```

### Handle Looping Patterns

Be aware of infinite patterns:

```csharp
if (_pattern.IsLooping)
{
    // Pattern never completes naturally
    // You need to manually stop it
}
```

### Check for Null Context

Always provide a valid context:

```csharp
// ✅ Good: Always provide context
var context = new PatternContext { Origin = position };
var spawns = pattern.GetSpawns(lastTime, currentTime, context);

// ❌ Bad: Null context (won't compile, but be explicit)
```

## Testing

### Test Pattern Isolation

Test patterns independently:

```csharp
[Test]
public void RingPattern_SpawnsCorrectCount()
{
    var pattern = Pattern.Ring(8, 150f);
    var context = new PatternContext { Origin = Vector2.Zero };
    var spawns = pattern.GetSpawns(0f, 0f, context).ToList();
    
    Assert.AreEqual(8, spawns.Count);
}
```

### Test Time Progression

Verify spawns at different times:

```csharp
[Test]
public void BurstPattern_SpawnsOverTime()
{
    var pattern = Pattern.Burst(3, Vector2.UnitY, 200f, delay: 0.1f);
    var context = new PatternContext { Origin = Vector2.Zero };
    
    var spawns1 = pattern.GetSpawns(0f, 0.05f, context).ToList();
    Assert.AreEqual(1, spawns1.Count); // First bullet
    
    var spawns2 = pattern.GetSpawns(0.05f, 0.15f, context).ToList();
    Assert.AreEqual(1, spawns2.Count); // Second bullet
}
```

## Common Pitfalls

### Duplicate Spawns

**Problem:** Bullets spawn multiple times

**Solution:** Never reset `lastTime`, always track it correctly

```csharp
// ✅ Correct
_lastQueryTime = patternTime;

// ❌ Wrong
_lastQueryTime = 0f; // Causes duplicates!
```

### Missing Spawns

**Problem:** Some bullets don't spawn

**Solution:** Ensure `currentTime > lastTime` and check pattern duration

```csharp
// ✅ Correct
if (currentTime > lastTime)
{
    var spawns = pattern.GetSpawns(lastTime, currentTime, context);
}
```

### Wrong Origin

**Problem:** Bullets spawn at wrong position

**Solution:** Always set `PatternContext.Origin`

```csharp
// ✅ Correct
var context = new PatternContext { Origin = enemy.Position };
```

## Next Steps

- **[Examples](Examples)** - See complete examples
- **[Troubleshooting](Troubleshooting)** - Solutions to common problems
- **[Integration](Integration)** - Framework-specific guides

