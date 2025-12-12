# Troubleshooting

Common issues and solutions when using BenedictBulletHell.Patterns.

## No Bullets Spawning

### Problem: Pattern doesn't spawn any bullets

**Possible Causes:**

1. **Time range is zero or negative**
   ```csharp
   // ❌ Bad: No time passed
   var spawns = pattern.GetSpawns(0f, 0f, context);
   
   // ✅ Good: Time has passed
   var spawns = pattern.GetSpawns(0f, 0.1f, context);
   ```

2. **Querying before pattern starts**
   ```csharp
   // ❌ Bad: Querying at negative time
   float patternTime = -0.1f;
   var spawns = pattern.GetSpawns(0f, patternTime, context);
   
   // ✅ Good: Querying at positive time
   float patternTime = 0.1f;
   var spawns = pattern.GetSpawns(0f, patternTime, context);
   ```

3. **Pattern already completed**
   ```csharp
   // Pattern duration is 1.0 seconds
   // ❌ Bad: Querying after completion
   var spawns = pattern.GetSpawns(1.5f, 2.0f, context);
   
   // ✅ Good: Querying during pattern
   var spawns = pattern.GetSpawns(0f, 0.5f, context);
   ```

**Solution:** Ensure `currentTime > lastTime` and pattern hasn't completed.

---

## Duplicate Bullets

### Problem: Same bullets spawn multiple times

**Possible Causes:**

1. **Resetting lastTime**
   ```csharp
   // ❌ Bad: Resetting lastTime
   _lastQueryTime = 0f; // Causes duplicates!
   
   // ✅ Good: Only move forward
   _lastQueryTime = patternTime;
   ```

2. **Querying same time range multiple times**
   ```csharp
   // ❌ Bad: Querying same range twice
   var spawns1 = pattern.GetSpawns(0f, 0.1f, context);
   var spawns2 = pattern.GetSpawns(0f, 0.1f, context); // Duplicates!
   
   // ✅ Good: Querying different ranges
   var spawns1 = pattern.GetSpawns(0f, 0.1f, context);
   var spawns2 = pattern.GetSpawns(0.1f, 0.2f, context);
   ```

3. **Using frame deltas instead of absolute time**
   ```csharp
   // ❌ Bad: Accumulating deltas
   _accumulatedTime += deltaTime;
   var spawns = pattern.GetSpawns(0f, _accumulatedTime, context);
   
   // ✅ Good: Using absolute time
   float patternTime = currentGameTime - _patternStartTime;
   var spawns = pattern.GetSpawns(_lastQueryTime, patternTime, context);
   ```

**Solution:** Always track `lastTime` correctly and never reset it.

---

## Bullets Spawning at Wrong Position

### Problem: Bullets appear at (0, 0) or wrong location

**Possible Causes:**

1. **Missing Origin in context**
   ```csharp
   // ❌ Bad: No origin set
   var context = new PatternContext();
   
   // ✅ Good: Origin set
   var context = new PatternContext
   {
       Origin = enemy.Position
   };
   ```

2. **Wrong coordinate system**
   ```csharp
   // Check if your game uses different coordinate system
   // (e.g., Y-up vs Y-down)
   ```

**Solution:** Always set `PatternContext.Origin` to the correct position.

---

## Aimed Patterns Not Working

### Problem: Aimed patterns don't aim at target

**Possible Causes:**

1. **Target not set in context**
   ```csharp
   // ❌ Bad: No target
   var context = new PatternContext
   {
       Origin = enemy.Position
   };
   
   // ✅ Good: Target set
   var context = new PatternContext
   {
       Origin = enemy.Position,
       Target = player.Position
   };
   ```

2. **Target is null**
   ```csharp
   // ❌ Bad: Null target
   var context = new PatternContext
   {
       Origin = position,
       Target = null
   };
   
   // ✅ Good: Valid target
   if (player != null)
   {
       var context = new PatternContext
       {
           Origin = position,
           Target = player.Position
       };
   }
   ```

**Solution:** Always set `PatternContext.Target` for aimed patterns.

---

## Pattern Completing Too Early/Late

### Problem: Pattern duration doesn't match expectations

**Possible Causes:**

1. **Misunderstanding duration calculation**
   ```csharp
   // Burst pattern: 5 bullets, 0.1s delay
   // Duration = (5 - 1) * 0.1 = 0.4 seconds
   var pattern = Pattern.Burst(5, Vector2.UnitY, 200f, 0.1f);
   float duration = pattern.Duration; // 0.4f
   ```

2. **Composite pattern duration**
   ```csharp
   // Sequence duration = sum of all patterns
   var sequence = Pattern.Sequence(
       Pattern.Ring(8, 150f),      // Duration: 0
       Pattern.Burst(5, ..., 0.1f)  // Duration: 0.4
   );
   // Total duration: 0.4 seconds
   ```

**Solution:** Check pattern `Duration` property and understand how it's calculated.

---

## Performance Issues

### Problem: Game runs slowly with many patterns

**Possible Causes:**

1. **Creating patterns every frame**
   ```csharp
   // ❌ Bad: Creating pattern every frame
   void Update()
   {
       var pattern = Pattern.Ring(8, 150f); // Unnecessary allocation
   }
   
   // ✅ Good: Create once
   private readonly IBulletPattern _pattern = Pattern.Ring(8, 150f);
   ```

2. **Materializing spawns unnecessarily**
   ```csharp
   // ❌ Bad: Materializing to list
   var spawns = pattern.GetSpawns(...).ToList();
   // Process spawns
   
   // ✅ Good: Process immediately
   foreach (var spawn in pattern.GetSpawns(...))
   {
       CreateBullet(spawn);
   }
   ```

3. **Too many pattern queries**
   ```csharp
   // ❌ Bad: Querying multiple times
   var spawns1 = pattern.GetSpawns(0f, time, context);
   var spawns2 = pattern.GetSpawns(0f, time, context); // Duplicate work
   
   // ✅ Good: Query once
   var spawns = pattern.GetSpawns(lastTime, currentTime, context);
   ```

**Solution:** Cache patterns, process spawns immediately, query once per frame.

---

## Looping Patterns Never Stop

### Problem: Looping pattern continues indefinitely

**Possible Causes:**

1. **Pattern is designed to loop**
   ```csharp
   var pattern = Pattern.Loop(Pattern.Ring(8, 150f));
   bool isLooping = pattern.IsLooping; // true
   ```

2. **Not manually stopping**
   ```csharp
   // Looping patterns never complete naturally
   // You must manually stop them
   if (shouldStop)
   {
       _isAttacking = false;
   }
   ```

**Solution:** Check `IsLooping` property and manually stop looping patterns when needed.

---

## Vector Conversion Issues

### Problem: Bullets spawn at wrong position after conversion

**Possible Causes:**

1. **Wrong conversion method**
   ```csharp
   // ❌ Bad: Using wrong framework conversion
   var monoGamePos = spawn.Position.ToUnity(); // Wrong!
   
   // ✅ Good: Using correct conversion
   var monoGamePos = spawn.Position.ToXna();
   ```

2. **Coordinate system mismatch**
   ```csharp
   // Some frameworks use Y-up, others Y-down
   // Check your framework's coordinate system
   ```

**Solution:** Use correct extension method for your framework.

---

## Pattern Not Respecting Metadata

### Problem: Custom metadata not affecting pattern

**Possible Causes:**

1. **Pattern doesn't read metadata**
   ```csharp
   // Built-in patterns don't use metadata
   // Only custom patterns can use it
   ```

2. **Metadata key mismatch**
   ```csharp
   // ❌ Bad: Wrong key
   context.GetMetadataValue<float>("SpeedMult", 1.0f);
   
   // ✅ Good: Correct key
   context.GetMetadataValue<float>("SpeedMultiplier", 1.0f);
   ```

**Solution:** Built-in patterns don't use metadata. Create custom patterns if you need metadata support.

---

## Testing Issues

### Problem: Tests fail or behave unexpectedly

**Possible Causes:**

1. **Time range issues in tests**
   ```csharp
   // ❌ Bad: Zero time range
   var spawns = pattern.GetSpawns(0f, 0f, context);
   
   // ✅ Good: Valid time range
   var spawns = pattern.GetSpawns(0f, 0.1f, context);
   ```

2. **Missing context**
   ```csharp
   // ❌ Bad: No context
   var spawns = pattern.GetSpawns(0f, 0.1f, null);
   
   // ✅ Good: Valid context
   var context = new PatternContext { Origin = Vector2.Zero };
   var spawns = pattern.GetSpawns(0f, 0.1f, context);
   ```

**Solution:** Always provide valid time ranges and context in tests.

---

## Common Error Messages

### "ArgumentNullException"

**Cause:** Null parameter passed to pattern factory

**Solution:** Ensure all parameters are non-null

```csharp
// ❌ Bad
var pattern = Pattern.Single(null, 200f);

// ✅ Good
var pattern = Pattern.Single(Vector2.UnitY, 200f);
```

### "ArgumentException"

**Cause:** Invalid parameter value (e.g., negative count, speed)

**Solution:** Validate parameters before creating pattern

```csharp
// ❌ Bad
var pattern = Pattern.Ring(-5, 150f);

// ✅ Good
var pattern = Pattern.Ring(8, 150f);
```

---

## Debugging Tips

### 1. Log Pattern State

```csharp
Console.WriteLine($"Pattern Duration: {pattern.Duration}");
Console.WriteLine($"Pattern IsLooping: {pattern.IsLooping}");
Console.WriteLine($"Last Query Time: {_lastQueryTime}");
Console.WriteLine($"Current Time: {patternTime}");
```

### 2. Inspect Spawns

```csharp
var spawns = pattern.GetSpawns(lastTime, currentTime, context).ToList();
Console.WriteLine($"Spawn Count: {spawns.Count}");
foreach (var spawn in spawns)
{
    Console.WriteLine($"  Position: {spawn.Position}, Direction: {spawn.Direction}");
}
```

### 3. Verify Context

```csharp
Console.WriteLine($"Context Origin: {context.Origin}");
Console.WriteLine($"Context Target: {context.Target}");
Console.WriteLine($"Context PatternAge: {context.PatternAge}");
```

### 4. Check Timing

```csharp
Console.WriteLine($"Pattern Start Time: {_patternStartTime}");
Console.WriteLine($"Current Game Time: {currentGameTime}");
Console.WriteLine($"Pattern Time: {patternTime}");
Console.WriteLine($"Time Delta: {patternTime - _lastQueryTime}");
```

---

## Getting Help

If you're still having issues:

1. **Check the examples** - [Examples](Examples)
2. **Review best practices** - [Best Practices](Best-Practices)
3. **Read the API reference** - [API Reference](API-Reference)
4. **Check the design document** - [DESIGN.md](../DESIGN.md)

---

## Next Steps

- **[Best Practices](Best-Practices)** - Prevent common issues
- **[Examples](Examples)** - See working code
- **[Core Concepts](Core-Concepts)** - Understand the fundamentals

