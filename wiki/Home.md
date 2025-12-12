# BenedictBulletHell.Patterns Wiki

Welcome to the BenedictBulletHell.Patterns wiki! This library helps you create complex bullet hell patterns for your games with a clean, composable API.

> **Note:** This wiki is automatically synced from the main repository. See the [GitHub repository](https://github.com/JohnVonDrashek/benedict-bullethell-patterns) for the source.

## What This Library Does

BenedictBulletHell.Patterns generates:
- **Bullet spawn instructions** - When and where bullets should appear
- **Pattern definitions** - Composable, reusable pattern configurations
- **Time-based execution** - Deterministic, frame-rate independent pattern timing
- **Framework-agnostic design** - Works with MonoGame, FNA, Unity, or any .NET framework

All patterns are **stateless and immutable** - same pattern + same time = identical output.

## Quick Links

### Getting Started
- **[Getting Started](Getting-Started)** - Step-by-step tutorial to create your first pattern
- **[Core Concepts](Core-Concepts)** - Understanding the fundamental ideas

### Main Topics
- **[Patterns](Patterns)** - Complete guide to all pattern types (Basic, Advanced, Composite, Modifiers)
- **[API Reference](API-Reference)** - Quick reference for main classes and methods
- **[Integration](Integration)** - Framework-specific integration guides
- **[Examples](Examples)** - Complete, runnable examples for common scenarios
- **[Best Practices](Best-Practices)** - Tips and patterns for effective pattern creation
- **[Troubleshooting](Troubleshooting)** - Common issues and solutions

### Advanced
- **[Advanced Topics](Advanced-Topics)** - Custom patterns, performance optimization, extensibility
- **[Design Document](../DESIGN.md)** - Complete technical documentation

## Installation

```bash
dotnet add package BenedictBulletHell.Patterns
```

Or add to your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="BenedictBulletHell.Patterns" Version="1.0.0" />
</ItemGroup>
```

## Quick Example

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using System.Numerics;

// 1. Create a pattern
var ringPattern = Pattern.Ring(count: 8, speed: 150f);

// 2. Set up timing
float lastTime = 0f;
float currentTime = 0.1f; // 100ms elapsed

// 3. Create context
var context = new PatternContext
{
    Origin = new Vector2(400, 300), // Boss position
    Target = new Vector2(200, 100)  // Optional: player position
};

// 4. Get spawns and create bullets
foreach (var spawn in ringPattern.GetSpawns(lastTime, currentTime, context))
{
    CreateBullet(spawn.Position, spawn.Direction, spawn.Speed);
}
```

## What This Library Does NOT Do

- ❌ Bullet rendering (you handle graphics)
- ❌ Bullet physics/movement (you handle updates)
- ❌ Collision detection (you handle collisions)
- ❌ Bullet lifetime management (you manage bullet lifecycle)

This library **only** defines when and how bullets spawn. Everything else is your responsibility.

## Next Steps

1. **[Getting Started](Getting-Started)** - Follow the tutorial to create your first pattern
2. **[Patterns](Patterns)** - Learn about all available pattern types
3. **[Examples](Examples)** - See complete examples for your use case
4. **[Integration](Integration)** - Integrate with your game framework

## Resources

- [GitHub Repository](https://github.com/JohnVonDrashek/benedict-bullethell-patterns)
- [NuGet Package](https://www.nuget.org/packages/BenedictBulletHell.Patterns)
- [Design Document](../DESIGN.md) - Complete technical documentation

## License

MIT License - see [LICENSE](../LICENSE) file for details.

