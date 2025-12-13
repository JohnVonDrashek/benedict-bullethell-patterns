# Advanced Topics

Advanced techniques and extensibility for BenedictBulletHell.Patterns.

## Custom Patterns

You can create custom patterns by implementing `IBulletPattern`:

```csharp
public class CustomPattern : IBulletPattern
{
    private readonly int _bulletCount;
    private readonly float _speed;
    private readonly float _delay;

    public CustomPattern(int bulletCount, float speed, float delay)
    {
        _bulletCount = bulletCount;
        _speed = speed;
        _delay = delay;
    }

    public float Duration => (_bulletCount - 1) * _delay;
    public bool IsLooping => false;

    public IEnumerable<BulletSpawn> GetSpawns(
        float lastTime,
        float currentTime,
        PatternContext context)
    {
        // Generate spawns in time range [lastTime, currentTime)
        for (int i = 0; i < _bulletCount; i++)
        {
            float spawnTime = i * _delay;
            
            // Only spawn if in the requested time range
            if (spawnTime >= lastTime && spawnTime < currentTime)
            {
                yield return new BulletSpawn
                {
                    Position = context.Origin,
                    Direction = new Vector2(1, 0), // Example direction
                    Speed = _speed,
                    Angle = 0f
                };
            }
        }
    }
}
```

### Custom Pattern Factory

Add your custom pattern to the factory:

```csharp
public static class Pattern
{
    // ... existing methods ...

    public static IBulletPattern Custom(int bulletCount, float speed, float delay)
    {
        return new CustomPattern(bulletCount, speed, delay);
    }
}
```

## Performance Optimization

### Allocation-Free Queries

The library uses `IEnumerable<BulletSpawn>` which may allocate. For high-performance scenarios:

1. **Materialize early**: Convert to array/list if you need to iterate multiple times
2. **Process immediately**: Don't store spawns, process them as they're generated
3. **Batch processing**: Query multiple patterns together

```csharp
// ✅ Good: Process immediately
foreach (var spawn in pattern.GetSpawns(...))
{
    CreateBullet(spawn);
}

// ⚠️ Caution: Materializing allocates
var spawns = pattern.GetSpawns(...).ToArray(); // Only if needed
```

### Pattern Caching

Cache patterns to avoid recreation:

```csharp
public class PatternCache
{
    private static readonly Dictionary<string, IBulletPattern> _cache = new();

    public static IBulletPattern GetOrCreate(string key, Func<IBulletPattern> factory)
    {
        if (!_cache.TryGetValue(key, out var pattern))
        {
            pattern = factory();
            _cache[key] = pattern;
        }
        return pattern;
    }
}

// Usage
var pattern = PatternCache.GetOrCreate("ring-8", () => Pattern.Ring(8, 150f));
```

### Query Optimization

Minimize query overhead:

```csharp
// ✅ Good: Query once per frame
var spawns = pattern.GetSpawns(lastTime, currentTime, context);
ProcessSpawns(spawns);

// ❌ Bad: Query multiple times
var spawns1 = pattern.GetSpawns(0f, currentTime, context);
var spawns2 = pattern.GetSpawns(0f, currentTime, context); // Duplicate work
```

## Pattern Metadata

Use `PatternContext.Metadata` for runtime customization:

```csharp
// Set metadata
var context = new PatternContext
{
    Origin = position,
    Metadata = new Dictionary<string, object>
    {
        ["SpeedMultiplier"] = 1.5f,
        ["AngleOffset"] = 15f,
        ["BulletType"] = "fire"
    }
};

// Custom pattern reads metadata
public class MetadataAwarePattern : IBulletPattern
{
    public IEnumerable<BulletSpawn> GetSpawns(
        float lastTime,
        float currentTime,
        PatternContext context)
    {
        float speedMultiplier = context.GetMetadataValue<float>("SpeedMultiplier", 1.0f);
        float angleOffset = context.GetMetadataValue<float>("AngleOffset", 0f);
        
        // Use metadata to customize spawns
        yield return new BulletSpawn
        {
            Position = context.Origin,
            Direction = Vector2.UnitY,
            Speed = 200f * speedMultiplier,
            Angle = angleOffset
        };
    }
}
```

## Pattern Age

Use `PatternContext.PatternAge` for time-based effects:

```csharp
public class EscalatingPattern : IBulletPattern
{
    public IEnumerable<BulletSpawn> GetSpawns(
        float lastTime,
        float currentTime,
        PatternContext context)
    {
        // Increase speed over time
        float ageMultiplier = 1.0f + (context.PatternAge * 0.1f);
        float speed = 200f * ageMultiplier;
        
        yield return new BulletSpawn
        {
            Position = context.Origin,
            Direction = Vector2.UnitY,
            Speed = speed
        };
    }
}
```

## Complex Compositions

Build sophisticated patterns through composition:

### Layered Attacks

```csharp
// Multiple patterns firing simultaneously
var layered = Pattern.Parallel(
    Pattern.Ring(16, 150f),
    Pattern.Spread(8, 60f, 200f),
    Pattern.Aimed(3, 180f, 10f)
);
```

### Phased Attacks

```csharp
// Sequential phases with transitions
var phased = Pattern.Sequence(
    Pattern.Ring(16, 150f),
    Pattern.Spiral(12, 2f, 150f, 180f),
    Pattern.Spread(8, 90f, 200f),
    Pattern.Burst(10, Vector2.UnitY, 200f, 0.1f)
);
```

### Nested Compositions

```csharp
// Patterns within patterns
var inner = Pattern.Sequence(
    Pattern.Ring(8, 150f),
    Pattern.Spread(5, 45f, 200f)
);

var outer = Pattern.Repeat(inner, count: 3, delay: 1f);
var rotating = Pattern.Rotating(outer, 30f);
```

## Pattern State Management

While patterns are stateless, you may need to track state:

### Pattern Instance State

```csharp
public class PatternInstance
{
    private readonly IBulletPattern _pattern;
    private float _startTime;
    private float _lastQueryTime;
    private bool _isActive;

    public PatternInstance(IBulletPattern pattern)
    {
        _pattern = pattern;
    }

    public void Start(float gameTime)
    {
        _startTime = gameTime;
        _lastQueryTime = 0f;
        _isActive = true;
    }

    public IEnumerable<BulletSpawn> Query(float currentGameTime, PatternContext context)
    {
        if (!_isActive) yield break;

        float patternTime = currentGameTime - _startTime;
        
        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            yield return spawn;
        }

        _lastQueryTime = patternTime;

        if (patternTime >= _pattern.Duration && !_pattern.IsLooping)
        {
            _isActive = false;
        }
    }
}
```

## Testing Patterns

Test patterns in isolation:

### Unit Testing

```csharp
[Test]
public void RingPattern_SpawnsCorrectCount()
{
    var pattern = Pattern.Ring(8, 150f);
    var context = new PatternContext { Origin = Vector2.Zero };
    
    var spawns = pattern.GetSpawns(0f, 0f, context).ToList();
    
    Assert.AreEqual(8, spawns.Count);
}

[Test]
public void BurstPattern_SpawnsOverTime()
{
    var pattern = Pattern.Burst(3, Vector2.UnitY, 200f, delay: 0.1f);
    var context = new PatternContext { Origin = Vector2.Zero };
    
    // First bullet at t=0
    var spawns1 = pattern.GetSpawns(0f, 0.05f, context).ToList();
    Assert.AreEqual(1, spawns1.Count);
    
    // Second bullet at t=0.1
    var spawns2 = pattern.GetSpawns(0.05f, 0.15f, context).ToList();
    Assert.AreEqual(1, spawns2.Count);
    
    // Third bullet at t=0.2
    var spawns3 = pattern.GetSpawns(0.15f, 0.25f, context).ToList();
    Assert.AreEqual(1, spawns3.Count);
}
```

### Integration Testing

```csharp
[Test]
public void SequencePattern_ExecutesInOrder()
{
    var pattern = Pattern.Sequence(
        Pattern.Ring(8, 150f),
        Pattern.Spread(5, 45f, 200f)
    );
    
    var context = new PatternContext { Origin = Vector2.Zero };
    
    // First phase (ring)
    var spawns1 = pattern.GetSpawns(0f, 0f, context).ToList();
    Assert.AreEqual(8, spawns1.Count);
    
    // Second phase (spread) - after ring completes
    var spawns2 = pattern.GetSpawns(0f, 0.1f, context).ToList();
    Assert.AreEqual(13, spawns2.Count); // 8 ring + 5 spread
}
```

## Debugging Patterns

### Pattern Inspection

```csharp
public static class PatternDebugger
{
    public static void Inspect(IBulletPattern pattern)
    {
        Console.WriteLine($"Duration: {pattern.Duration}");
        Console.WriteLine($"IsLooping: {pattern.IsLooping}");
        
        var context = new PatternContext { Origin = Vector2.Zero };
        var spawns = pattern.GetSpawns(0f, pattern.Duration, context).ToList();
        
        Console.WriteLine($"Total Spawns: {spawns.Count}");
        foreach (var spawn in spawns)
        {
            Console.WriteLine($"  Position: {spawn.Position}, Direction: {spawn.Direction}, Speed: {spawn.Speed}");
        }
    }
}
```

### Timing Verification

```csharp
public static void VerifyTiming(IBulletPattern pattern, float step = 0.1f)
{
    var context = new PatternContext { Origin = Vector2.Zero };
    var allSpawns = new List<BulletSpawn>();
    
    float lastTime = 0f;
    for (float t = step; t <= pattern.Duration; t += step)
    {
        var spawns = pattern.GetSpawns(lastTime, t, context).ToList();
        allSpawns.AddRange(spawns);
        lastTime = t;
    }
    
    Console.WriteLine($"Total spawns: {allSpawns.Count}");
    Console.WriteLine($"Expected duration: {pattern.Duration}");
}
```

## Extensibility Points

### Custom Pattern Types

Create domain-specific patterns:

```csharp
public class HomingPattern : IBulletPattern
{
    // Custom homing logic
}

public class CurvedPattern : IBulletPattern
{
    // Custom curved trajectory
}
```

### Pattern Decorators

Wrap patterns with additional behavior:

```csharp
public class DelayedPattern : IBulletPattern
{
    private readonly IBulletPattern _pattern;
    private readonly float _delay;

    public DelayedPattern(IBulletPattern pattern, float delay)
    {
        _pattern = pattern;
        _delay = delay;
    }

    public float Duration => _pattern.Duration + _delay;
    public bool IsLooping => _pattern.IsLooping;

    public IEnumerable<BulletSpawn> GetSpawns(
        float lastTime,
        float currentTime,
        PatternContext context)
    {
        // Offset time by delay
        float adjustedLastTime = Math.Max(0f, lastTime - _delay);
        float adjustedCurrentTime = Math.Max(0f, currentTime - _delay);
        
        if (adjustedCurrentTime > 0f)
        {
            foreach (var spawn in _pattern.GetSpawns(adjustedLastTime, adjustedCurrentTime, context))
            {
                yield return spawn;
            }
        }
    }
}
```

## Pattern Serialization

The library provides comprehensive JSON serialization support for all pattern types, enabling save/load functionality, pattern sharing, and runtime configuration.

### Basic Serialization

```csharp
using BenedictBulletHell.Patterns.Serialization;

var serializer = new JsonPatternSerializer();
var pattern = Pattern.Ring(8, 150f);

// Serialize to string
string json = serializer.Serialize(pattern);

// Deserialize from string
var deserialized = serializer.Deserialize(json);

// Verify round-trip
var context = new PatternContext { Origin = Vector2.Zero };
var originalSpawns = pattern.GetSpawns(0f, 0f, context).ToList();
var deserializedSpawns = deserialized.GetSpawns(0f, 0f, context).ToList();
// Both produce identical spawns
```

### File I/O

```csharp
// Save pattern to file
using (var stream = File.Create("boss_attack.json"))
{
    serializer.Serialize(pattern, stream);
}

// Load pattern from file
using (var stream = File.OpenRead("boss_attack.json"))
{
    var loaded = serializer.Deserialize(stream);
}
```

### JSON Format

Patterns use a polymorphic JSON structure with type discriminators:

**Basic Pattern:**
```json
{
  "type": "RingPattern",
  "bulletCount": 8,
  "speed": 150.0,
  "startAngle": 0.0
}
```

**Composite Pattern:**
```json
{
  "type": "SequencePattern",
  "looping": false,
  "patterns": [
    { "type": "RingPattern", "bulletCount": 8, "speed": 150.0 },
    { "type": "SpreadPattern", "bulletCount": 5, "angleSpread": 45.0, "speed": 200.0 }
  ]
}
```

**Nested Pattern with Modifier:**
```json
{
  "type": "RotatingPattern",
  "rotationSpeed": 90.0,
  "pattern": {
    "type": "SequencePattern",
    "looping": false,
    "patterns": [
      { "type": "RingPattern", "bulletCount": 8, "speed": 150.0 },
      { "type": "SpreadPattern", "bulletCount": 5, "angleSpread": 45.0, "speed": 200.0 }
    ]
  }
}
```

### Pattern Caching with Serialization

Combine serialization with caching for efficient pattern management:

```csharp
public class SerializedPatternCache
{
    private readonly IPatternSerializer _serializer = new JsonPatternSerializer();
    private readonly Dictionary<string, IBulletPattern> _cache = new();
    private readonly string _cacheDirectory;

    public SerializedPatternCache(string cacheDirectory)
    {
        _cacheDirectory = cacheDirectory;
        Directory.CreateDirectory(cacheDirectory);
    }

    public IBulletPattern GetOrLoad(string patternId)
    {
        if (_cache.TryGetValue(patternId, out var cached))
            return cached;

        string filePath = Path.Combine(_cacheDirectory, $"{patternId}.json");
        
        if (File.Exists(filePath))
        {
            using (var stream = File.OpenRead(filePath))
            {
                var pattern = _serializer.Deserialize(stream);
                _cache[patternId] = pattern;
                return pattern;
            }
        }

        throw new FileNotFoundException($"Pattern file not found: {filePath}");
    }

    public void Save(string patternId, IBulletPattern pattern)
    {
        _cache[patternId] = pattern;
        
        string filePath = Path.Combine(_cacheDirectory, $"{patternId}.json");
        using (var stream = File.Create(filePath))
        {
            _serializer.Serialize(pattern, stream);
        }
    }
}
```

### Runtime Pattern Configuration

Load patterns from configuration files for easy difficulty adjustment:

```csharp
public class ConfigurableBoss
{
    private readonly IPatternSerializer _serializer = new JsonPatternSerializer();
    private IBulletPattern _attackPattern;

    public void LoadPatternFromConfig(string configPath)
    {
        string json = File.ReadAllText(configPath);
        _attackPattern = _serializer.Deserialize(json);
    }

    public void AdjustDifficulty(float speedMultiplier)
    {
        // Note: Patterns are immutable, so you'd need to deserialize,
        // modify the JSON, and re-serialize, or use PatternContext.Metadata
        // for runtime adjustments instead
    }
}
```

### Error Handling

Serialization methods throw descriptive exceptions:

```csharp
try
{
    var pattern = serializer.Deserialize(json);
}
catch (ArgumentException ex)
{
    // Missing required fields, invalid values, etc.
    Console.WriteLine($"Invalid pattern: {ex.Message}");
}
catch (JsonException ex)
{
    // Malformed JSON
    Console.WriteLine($"JSON parse error: {ex.Message}");
}
```

### Performance Considerations

- Serialization is fast enough for runtime use (< 10ms for typical patterns)
- For large pattern libraries, consider caching deserialized patterns
- File I/O is the main bottleneck; use async I/O for large files
- JSON output is human-readable and well-formatted (indented)

### Round-Trip Guarantee

The serializer guarantees that serializing and deserializing a pattern produces identical behavior:

```csharp
var original = Pattern.Ring(8, 150f, 45f);
var json = serializer.Serialize(original);
var deserialized = serializer.Deserialize(json);

// Both patterns produce identical spawns
var context = new PatternContext { Origin = Vector2.Zero };
var originalSpawns = original.GetSpawns(0f, 0f, context).ToList();
var deserializedSpawns = deserialized.GetSpawns(0f, 0f, context).ToList();

Assert.Equal(originalSpawns.Count, deserializedSpawns.Count);
for (int i = 0; i < originalSpawns.Count; i++)
{
    Assert.Equal(originalSpawns[i].Position, deserializedSpawns[i].Position);
    Assert.Equal(originalSpawns[i].Direction, deserializedSpawns[i].Direction);
    Assert.Equal(originalSpawns[i].Speed, deserializedSpawns[i].Speed);
}
```

## Next Steps

- **[Best Practices](Best-Practices)** - Apply these techniques effectively
- **[Examples](Examples)** - See advanced examples including serialization
- **[API Reference](API-Reference)** - Detailed API documentation including serialization

