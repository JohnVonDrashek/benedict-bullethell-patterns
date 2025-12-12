# Patterns

Complete guide to all pattern types available in BenedictBulletHell.Patterns.

## Pattern Categories

- **[Basic Patterns](#basic-patterns)** - Simple, fundamental patterns
- **[Advanced Patterns](#advanced-patterns)** - Complex, dynamic patterns
- **[Composite Patterns](#composite-patterns)** - Combine multiple patterns
- **[Modifier Patterns](#modifier-patterns)** - Transform existing patterns

## Basic Patterns

### Single Shot

Fires a single bullet in a specified direction.

```csharp
var single = Pattern.Single(
    direction: new Vector2(1, 0), // Right
    speed: 200f
);
```

**Use Cases:**
- Basic enemy attacks
- Player weapon firing
- Building blocks for composite patterns

**Parameters:**
- `direction` - Direction vector (will be normalized)
- `speed` - Speed in units per second

**Duration:** 0 seconds (instantaneous)

---

### Burst

Fires multiple bullets in quick succession.

```csharp
var burst = Pattern.Burst(
    count: 5,
    direction: new Vector2(0, -1), // Up
    speed: 180f,
    delay: 0.1f // 100ms between shots
);
```

**Use Cases:**
- Rapid-fire attacks
- Machine gun patterns
- Quick volleys

**Parameters:**
- `count` - Number of bullets to fire
- `direction` - Direction vector (will be normalized)
- `speed` - Speed in units per second
- `delay` - Delay between each shot in seconds

**Duration:** `(count - 1) * delay` seconds

---

### Spread

Fires bullets in a fan formation.

```csharp
var spread = Pattern.Spread(
    count: 5,
    angleSpread: 45f, // 45-degree fan
    speed: 200f,
    baseAngle: 90f // Pointing up
);
```

**Use Cases:**
- Shotgun-style attacks
- Wide-area coverage
- Cone attacks

**Parameters:**
- `count` - Number of bullets to fire
- `angleSpread` - Total angle spread in degrees
- `speed` - Speed in units per second
- `baseAngle` - Center angle in degrees (0 = right, 90 = up, default: 0)

**Duration:** 0 seconds (all bullets fire simultaneously)

---

### Ring

Fires bullets in a circle formation.

```csharp
var ring = Pattern.Ring(
    count: 8,
    speed: 150f,
    startAngle: 0f // Start at 0 degrees (right)
);
```

**Use Cases:**
- 360-degree attacks
- Circular bullet patterns
- Defensive patterns

**Parameters:**
- `count` - Number of bullets to fire
- `speed` - Speed in units per second
- `startAngle` - Starting angle in degrees (0 = right, 90 = up, default: 0)

**Duration:** 0 seconds (all bullets fire simultaneously)

---

## Advanced Patterns

### Spiral

Fires bullets in a rotating spiral pattern.

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

**Use Cases:**
- Complex boss attacks
- Rotating bullet patterns
- Dynamic spiral formations

**Parameters:**
- `bulletsPerRevolution` - Number of bullets per full revolution
- `totalRevolutions` - Total number of revolutions
- `speed` - Speed in units per second
- `rotationSpeed` - Rotation speed in degrees per second (default: 360)
- `startAngle` - Starting angle in degrees (default: 0)
- `looping` - Whether the spiral loops indefinitely (default: false)

**Duration:** `(totalRevolutions * 360) / rotationSpeed` seconds (or infinite if looping)

---

### Aimed

Fires bullet(s) toward a target position.

```csharp
var aimed = Pattern.Aimed(
    count: 1,
    speed: 200f,
    spreadAngle: 5f // Optional spread for multiple bullets
);
```

**Use Cases:**
- Player-targeting attacks
- Homing-style patterns
- Predictive aiming

**Parameters:**
- `count` - Number of bullets to fire
- `speed` - Speed in units per second
- `spreadAngle` - Optional spread angle in degrees for multiple bullets (default: 0)

**Note:** Requires `PatternContext.Target` to be set.

**Duration:** 0 seconds (all bullets fire simultaneously)

---

### Wave

Fires bullets in a sine-wave formation.

```csharp
var wave = Pattern.Wave(
    bulletCount: 10,
    baseDirection: 90f, // Pointing up
    waveAmplitude: 30f, // 30-degree side-to-side swing
    waveFrequency: 2f,  // 2 complete cycles across all bullets
    speed: 180f
);
```

**Use Cases:**
- Wavy bullet patterns
- S-curve formations
- Dynamic wave attacks

**Parameters:**
- `bulletCount` - Number of bullets to fire
- `baseDirection` - Base direction of wave travel in degrees
- `waveAmplitude` - Side-to-side amplitude in degrees
- `waveFrequency` - Frequency of the wave (complete cycles)
- `speed` - Speed in units per second

**Duration:** 0 seconds (all bullets fire simultaneously)

---

## Composite Patterns

### Sequence

Executes patterns one after another.

```csharp
var phase1 = Pattern.Ring(8, 150f);
var phase2 = Pattern.Spread(5, 45f, 180f);
var phase3 = Pattern.Burst(10, new Vector2(1, 0), 200f, 0.05f);

var sequence = Pattern.Sequence(phase1, phase2, phase3);
```

**Use Cases:**
- Multi-phase boss attacks
- Attack sequences
- Pattern transitions

**Parameters:**
- `patterns` - Patterns to execute in sequence

**Duration:** Sum of all pattern durations

---

### Looping Sequence

Executes patterns in sequence, then loops.

```csharp
var looped = Pattern.Sequence(looping: true, 
    Pattern.Ring(8, 150f),
    Pattern.Spread(5, 45f, 180f)
);
```

**Parameters:**
- `looping` - Whether to loop the sequence
- `patterns` - Patterns to execute in sequence

**Duration:** Infinite (if looping), otherwise sum of all pattern durations

---

### Parallel

Executes multiple patterns simultaneously.

```csharp
var ring = Pattern.Ring(8, 150f);
var spread = Pattern.Spread(5, 30f, 180f);

var layered = Pattern.Parallel(ring, spread);
```

**Use Cases:**
- Layered attacks
- Simultaneous pattern execution
- Complex multi-pattern attacks

**Parameters:**
- `patterns` - Patterns to execute in parallel

**Duration:** Maximum duration of all patterns

---

### Loop

Repeats a pattern indefinitely.

```csharp
var ring = Pattern.Ring(8, 150f);
var loopingRing = Pattern.Loop(ring); // Loops forever
```

**Use Cases:**
- Continuous attacks
- Repeating patterns
- Infinite loops

**Parameters:**
- `pattern` - The pattern to loop

**Duration:** Infinite

---

### Repeat

Repeats a pattern a specific number of times.

```csharp
var burst = Pattern.Burst(5, new Vector2(0, -1), 200f, 0.1f);
var repeatedBurst = Pattern.Repeat(burst, count: 3, delay: 0.5f);
```

**Use Cases:**
- Repeating attack cycles
- Multi-wave attacks
- Pattern repetition with delays

**Parameters:**
- `pattern` - The pattern to repeat
- `count` - Number of times to repeat
- `delay` - Delay between each repeat in seconds (default: 0)

**Duration:** `(pattern.Duration + delay) * count - delay` seconds

---

## Modifier Patterns

### Rotating

Rotates any pattern around the origin over time.

```csharp
var spread = Pattern.Spread(5, 30f, 180f);
var rotatingSpread = Pattern.Rotating(spread, degreesPerSecond: 90f);
```

**Use Cases:**
- Rotating attack patterns
- Dynamic pattern rotation
- Time-based pattern transformation

**Parameters:**
- `pattern` - The pattern to rotate
- `degreesPerSecond` - Rotation speed in degrees per second

**Duration:** Same as the base pattern

**Note:** Rotation is applied to the pattern's spawn directions, not the pattern itself.

---

## Pattern Properties

All patterns implement `IBulletPattern` and have:

- **`Duration`** - Total duration in seconds (or `float.PositiveInfinity` if looping)
- **`IsLooping`** - Whether the pattern loops indefinitely
- **`GetSpawns()`** - Method to query for bullet spawns between two time points

## Combining Patterns

Patterns are composable - you can combine them in any way:

```csharp
// Complex boss attack
var bossAttack = Pattern.Sequence(
    Pattern.Ring(16, 150f),
    Pattern.Parallel(
        Pattern.Spread(8, 60f, 200f),
        Pattern.Aimed(3, 180f, 10f)
    ),
    Pattern.Spiral(12, 2f, 150f, 180f),
    Pattern.Repeat(
        Pattern.Burst(5, new Vector2(0, -1), 200f, 0.1f),
        count: 3,
        delay: 0.5f
    )
);
```

## Next Steps

- **[Examples](Examples)** - See complete examples using these patterns
- **[Best Practices](Best-Practices)** - Tips for effective pattern creation
- **[API Reference](API-Reference)** - Detailed API documentation

