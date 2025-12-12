# Examples

Complete, runnable examples for common scenarios.

## Table of Contents

- [Basic Enemy Attack](#basic-enemy-attack)
- [Boss Multi-Phase Attack](#boss-multi-phase-attack)
- [Player Weapon Patterns](#player-weapon-patterns)
- [Rotating Pattern](#rotating-pattern)
- [Complex Composite Pattern](#complex-composite-pattern)
- [Aimed Pattern with Target](#aimed-pattern-with-target)
- [Looping Continuous Attack](#looping-continuous-attack)
- [Game Loop Integration](#game-loop-integration)

## Basic Enemy Attack

Simple enemy that fires a burst pattern:

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using System.Numerics;

public class BasicEnemy
{
    private IBulletPattern _attackPattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private bool _isAttacking;
    private Vector2 _position;

    public BasicEnemy(Vector2 position)
    {
        _position = position;
        _attackPattern = Pattern.Burst(
            count: 3,
            direction: new Vector2(0, -1), // Up
            speed: 200f,
            delay: 0.2f
        );
    }

    public void StartAttack(float gameTime)
    {
        _patternStartTime = gameTime;
        _lastQueryTime = 0f;
        _isAttacking = true;
    }

    public void Update(float currentGameTime, Action<Vector2, Vector2, float> createBullet)
    {
        if (!_isAttacking) return;

        float patternTime = currentGameTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position
        };

        foreach (var spawn in _attackPattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
        }

        _lastQueryTime = patternTime;

        if (patternTime >= _attackPattern.Duration && !_attackPattern.IsLooping)
        {
            _isAttacking = false;
        }
    }
}
```

## Boss Multi-Phase Attack

Boss with multiple attack phases:

```csharp
public class Boss
{
    private IBulletPattern _phase1Pattern;
    private IBulletPattern _phase2Pattern;
    private IBulletPattern _phase3Pattern;
    private IBulletPattern _currentPattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private int _currentPhase;
    private Vector2 _position;
    private Vector2? _playerPosition;

    public Boss(Vector2 position)
    {
        _position = position;
        
        // Phase 1: Ring attack
        _phase1Pattern = Pattern.Ring(16, speed: 150f);
        
        // Phase 2: Spiral attack
        _phase2Pattern = Pattern.Spiral(
            bulletsPerRevolution: 12,
            totalRevolutions: 2,
            speed: 180f,
            rotationSpeed: 180f
        );
        
        // Phase 3: Aimed spread
        _phase3Pattern = Pattern.Aimed(count: 5, speed: 200f, spreadAngle: 30f);
        
        _currentPattern = _phase1Pattern;
        _currentPhase = 1;
    }

    public void Update(float currentGameTime, Vector2? playerPosition, Action<Vector2, Vector2, float> createBullet)
    {
        _playerPosition = playerPosition;

        float patternTime = currentGameTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position,
            Target = _playerPosition
        };

        foreach (var spawn in _currentPattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
        }

        _lastQueryTime = patternTime;

        // Check for phase transition
        if (patternTime >= _currentPattern.Duration && !_currentPattern.IsLooping)
        {
            TransitionToNextPhase(currentGameTime);
        }
    }

    private void TransitionToNextPhase(float gameTime)
    {
        _currentPhase++;
        
        switch (_currentPhase)
        {
            case 2:
                _currentPattern = _phase2Pattern;
                break;
            case 3:
                _currentPattern = _phase3Pattern;
                break;
            default:
                // Loop back to phase 1
                _currentPhase = 1;
                _currentPattern = _phase1Pattern;
                break;
        }

        _patternStartTime = gameTime;
        _lastQueryTime = 0f;
    }

    public void StartAttack(float gameTime)
    {
        _patternStartTime = gameTime;
        _lastQueryTime = 0f;
        _currentPhase = 1;
        _currentPattern = _phase1Pattern;
    }
}
```

## Player Weapon Patterns

Different weapon types for the player:

```csharp
public class PlayerWeapon
{
    public enum WeaponType
    {
        Single,
        Burst,
        Spread,
        Rapid
    }

    private IBulletPattern _pattern;
    private float _lastFireTime;
    private float _cooldown;

    public PlayerWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Single:
                _pattern = Pattern.Single(new Vector2(0, -1), speed: 300f);
                _cooldown = 0.2f;
                break;
            case WeaponType.Burst:
                _pattern = Pattern.Burst(3, new Vector2(0, -1), speed: 300f, delay: 0.1f);
                _cooldown = 0.5f;
                break;
            case WeaponType.Spread:
                _pattern = Pattern.Spread(5, angleSpread: 30f, speed: 300f, baseAngle: 90f);
                _cooldown = 0.3f;
                break;
            case WeaponType.Rapid:
                _pattern = Pattern.Burst(1, new Vector2(0, -1), speed: 300f, delay: 0.05f);
                _cooldown = 0.1f;
                break;
        }
    }

    public void Fire(float currentTime, Vector2 position, Action<Vector2, Vector2, float> createBullet)
    {
        if (currentTime - _lastFireTime < _cooldown) return;

        var context = new PatternContext
        {
            Origin = position
        };

        foreach (var spawn in _pattern.GetSpawns(0f, 0f, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
        }

        _lastFireTime = currentTime;
    }
}
```

## Rotating Pattern

Pattern that rotates over time:

```csharp
public class RotatingEnemy
{
    private IBulletPattern _pattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private Vector2 _position;

    public RotatingEnemy(Vector2 position)
    {
        _position = position;
        
        // Create a spread pattern and rotate it
        var spread = Pattern.Spread(8, angleSpread: 60f, speed: 200f);
        _pattern = Pattern.Rotating(spread, degreesPerSecond: 90f);
    }

    public void Update(float currentGameTime, Action<Vector2, Vector2, float> createBullet)
    {
        float patternTime = currentGameTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position,
            PatternAge = patternTime
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
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

## Complex Composite Pattern

Combining multiple patterns into a complex attack:

```csharp
public class ComplexBossAttack
{
    private IBulletPattern _attackPattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private Vector2 _position;
    private Vector2? _playerPosition;

    public ComplexBossAttack(Vector2 position)
    {
        _position = position;
        
        // Create a complex multi-phase attack
        var phase1 = Pattern.Ring(16, speed: 150f);
        
        var phase2 = Pattern.Parallel(
            Pattern.Spread(8, 60f, 200f, baseAngle: 90f),
            Pattern.Aimed(3, 180f, spreadAngle: 10f)
        );
        
        var phase3 = Pattern.Spiral(
            bulletsPerRevolution: 12,
            totalRevolutions: 2,
            speed: 150f,
            rotationSpeed: 180f
        );
        
        var phase4 = Pattern.Repeat(
            Pattern.Burst(5, new Vector2(0, -1), 200f, delay: 0.1f),
            count: 3,
            delay: 0.5f
        );
        
        _attackPattern = Pattern.Sequence(phase1, phase2, phase3, phase4);
    }

    public void Update(float currentGameTime, Vector2? playerPosition, Action<Vector2, Vector2, float> createBullet)
    {
        _playerPosition = playerPosition;

        float patternTime = currentGameTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position,
            Target = _playerPosition
        };

        foreach (var spawn in _attackPattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
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

## Aimed Pattern with Target

Using aimed patterns that track the player:

```csharp
public class AimingEnemy
{
    private IBulletPattern _pattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private Vector2 _position;
    private Vector2? _targetPosition;

    public AimingEnemy(Vector2 position)
    {
        _position = position;
        
        // Create an aimed pattern with spread
        _pattern = Pattern.Aimed(count: 3, speed: 250f, spreadAngle: 15f);
    }

    public void Update(float currentGameTime, Vector2? targetPosition, Action<Vector2, Vector2, float> createBullet)
    {
        _targetPosition = targetPosition;

        if (_targetPosition == null) return;

        float patternTime = currentGameTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position,
            Target = _targetPosition
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
        }

        _lastQueryTime = patternTime;
    }

    public void Fire(float gameTime)
    {
        _patternStartTime = gameTime;
        _lastQueryTime = 0f;
    }
}
```

## Looping Continuous Attack

Pattern that loops indefinitely:

```csharp
public class ContinuousAttacker
{
    private IBulletPattern _pattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private Vector2 _position;

    public ContinuousAttacker(Vector2 position)
    {
        _position = position;
        
        // Create a looping ring pattern
        var ring = Pattern.Ring(8, speed: 150f);
        _pattern = Pattern.Loop(ring);
    }

    public void Update(float currentGameTime, Action<Vector2, Vector2, float> createBullet)
    {
        float patternTime = currentGameTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position,
            PatternAge = patternTime
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
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

## Game Loop Integration

Complete integration example:

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using System.Numerics;
using System.Collections.Generic;

public class Game
{
    private List<Enemy> _enemies = new List<Enemy>();
    private float _gameTime = 0f;

    public void Initialize()
    {
        // Create enemies with different patterns
        _enemies.Add(new Enemy(new Vector2(400, 100), CreateBurstPattern()));
        _enemies.Add(new Enemy(new Vector2(200, 200), CreateRingPattern()));
        _enemies.Add(new Enemy(new Vector2(600, 200), CreateSpiralPattern()));
    }

    private IBulletPattern CreateBurstPattern()
    {
        return Pattern.Burst(5, new Vector2(0, 1), speed: 200f, delay: 0.1f);
    }

    private IBulletPattern CreateRingPattern()
    {
        return Pattern.Ring(12, speed: 150f);
    }

    private IBulletPattern CreateSpiralPattern()
    {
        return Pattern.Spiral(8, 2f, speed: 180f, rotationSpeed: 90f);
    }

    public void Update(float deltaTime)
    {
        _gameTime += deltaTime;

        foreach (var enemy in _enemies)
        {
            enemy.Update(_gameTime, CreateBullet);
        }
    }

    private void CreateBullet(Vector2 position, Vector2 direction, float speed)
    {
        // Your bullet creation logic here
        Console.WriteLine($"Bullet at ({position.X}, {position.Y}) " +
                         $"direction ({direction.X}, {direction.Y}) " +
                         $"speed {speed}");
    }
}

public class Enemy
{
    private IBulletPattern _pattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private Vector2 _position;
    private bool _isAttacking;

    public Enemy(Vector2 position, IBulletPattern pattern)
    {
        _position = position;
        _pattern = pattern;
    }

    public void Update(float currentGameTime, Action<Vector2, Vector2, float> createBullet)
    {
        if (!_isAttacking)
        {
            StartAttack(currentGameTime);
            return;
        }

        float patternTime = currentGameTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
        }

        _lastQueryTime = patternTime;

        if (patternTime >= _pattern.Duration && !_pattern.IsLooping)
        {
            _isAttacking = false;
        }
    }

    private void StartAttack(float gameTime)
    {
        _patternStartTime = gameTime;
        _lastQueryTime = 0f;
        _isAttacking = true;
    }
}
```

## Next Steps

- **[Patterns](Patterns)** - Learn about all available pattern types
- **[Best Practices](Best-Practices)** - Tips for effective pattern creation
- **[Integration](Integration)** - Framework-specific integration guides

