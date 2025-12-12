# Integration

Framework-specific integration guides for MonoGame, FNA, Unity, and other .NET frameworks.

## MonoGame / FNA

### Vector Conversion

The library uses `System.Numerics.Vector2`, but MonoGame/FNA use `Microsoft.Xna.Framework.Vector2`. Use the extension methods:

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Extensions;
using Microsoft.Xna.Framework;
using System.Numerics;

public class MonoGameEnemy
{
    private IBulletPattern _pattern;
    private Vector2 _position; // MonoGame Vector2

    public void Update(GameTime gameTime)
    {
        float currentTime = (float)gameTime.TotalGameTime.TotalSeconds;
        
        // Convert MonoGame Vector2 to System.Numerics.Vector2
        var origin = new System.Numerics.Vector2(_position.X, _position.Y);
        
        var context = new PatternContext
        {
            Origin = origin
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, currentTime, context))
        {
            // Convert back to MonoGame Vector2
            var bulletPosition = spawn.Position.ToXna();
            var bulletDirection = spawn.Direction.ToXna();
            
            CreateBullet(bulletPosition, bulletDirection, spawn.Speed);
        }
    }
}
```

### Complete MonoGame Example

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Extensions;
using System.Numerics;

public class BulletHellGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _bulletTexture;
    private List<Bullet> _bullets = new List<Bullet>();
    private Enemy _enemy;

    public BulletHellGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _bulletTexture = Content.Load<Texture2D>("bullet");
        
        var enemyPos = new System.Numerics.Vector2(400, 300);
        _enemy = new Enemy(enemyPos);
    }

    protected override void Update(GameTime gameTime)
    {
        float currentTime = (float)gameTime.TotalGameTime.TotalSeconds;
        
        _enemy.Update(currentTime, (pos, dir, speed) =>
        {
            var bullet = new Bullet
            {
                Position = pos.ToXna(),
                Direction = dir.ToXna(),
                Speed = speed
            };
            _bullets.Add(bullet);
        });

        // Update bullets
        foreach (var bullet in _bullets)
        {
            bullet.Position += bullet.Direction * bullet.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();
        foreach (var bullet in _bullets)
        {
            _spriteBatch.Draw(_bulletTexture, bullet.Position, Color.White);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

public class Enemy
{
    private IBulletPattern _pattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private System.Numerics.Vector2 _position;
    private bool _isAttacking;

    public Enemy(System.Numerics.Vector2 position)
    {
        _position = position;
        _pattern = Pattern.Ring(8, speed: 150f);
    }

    public void Update(float currentTime, Action<System.Numerics.Vector2, System.Numerics.Vector2, float> createBullet)
    {
        if (!_isAttacking)
        {
            _patternStartTime = currentTime;
            _lastQueryTime = 0f;
            _isAttacking = true;
        }

        float patternTime = currentTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
        }

        _lastQueryTime = patternTime;
    }
}

public class Bullet
{
    public Microsoft.Xna.Framework.Vector2 Position;
    public Microsoft.Xna.Framework.Vector2 Direction;
    public float Speed;
}
```

---

## Unity

### Vector Conversion

Unity uses `UnityEngine.Vector2`, but the library uses `System.Numerics.Vector2`. Use the extension methods:

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Extensions;
using UnityEngine;
using System.Numerics;

public class UnityEnemy : MonoBehaviour
{
    private IBulletPattern _pattern;
    private float _patternStartTime;
    private float _lastQueryTime;

    void Start()
    {
        _pattern = Pattern.Ring(8, speed: 150f);
    }

    void Update()
    {
        float currentTime = Time.time;
        
        // Convert Unity Vector2 to System.Numerics.Vector2
        var origin = new System.Numerics.Vector2(transform.position.x, transform.position.y);
        
        var context = new PatternContext
        {
            Origin = origin
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, currentTime, context))
        {
            // Convert back to Unity Vector2
            var bulletPosition = spawn.Position.ToUnity();
            var bulletDirection = spawn.Direction.ToUnity();
            
            CreateBullet(bulletPosition, bulletDirection, spawn.Speed);
        }

        _lastQueryTime = currentTime;
    }
}
```

### Complete Unity Example

```csharp
using UnityEngine;
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using BenedictBulletHell.Patterns.Extensions;
using System.Collections.Generic;
using System.Numerics;

public class BulletHellManager : MonoBehaviour
{
    public GameObject bulletPrefab;
    private List<Bullet> _bullets = new List<Bullet>();
    private Enemy _enemy;

    void Start()
    {
        var enemyPos = new System.Numerics.Vector2(0, 0);
        _enemy = new Enemy(enemyPos);
    }

    void Update()
    {
        float currentTime = Time.time;
        
        _enemy.Update(currentTime, (pos, dir, speed) =>
        {
            var bulletObj = Instantiate(bulletPrefab);
            var bullet = bulletObj.GetComponent<Bullet>();
            bullet.Initialize(pos.ToUnity(), dir.ToUnity(), speed);
            _bullets.Add(bullet);
        });

        // Update bullets
        foreach (var bullet in _bullets)
        {
            bullet.Update(Time.deltaTime);
        }
    }
}

public class Enemy
{
    private IBulletPattern _pattern;
    private float _patternStartTime;
    private float _lastQueryTime;
    private System.Numerics.Vector2 _position;
    private bool _isAttacking;

    public Enemy(System.Numerics.Vector2 position)
    {
        _position = position;
        _pattern = Pattern.Ring(8, speed: 150f);
    }

    public void Update(float currentTime, System.Action<System.Numerics.Vector2, System.Numerics.Vector2, float> createBullet)
    {
        if (!_isAttacking)
        {
            _patternStartTime = currentTime;
            _lastQueryTime = 0f;
            _isAttacking = true;
        }

        float patternTime = currentTime - _patternStartTime;
        
        var context = new PatternContext
        {
            Origin = _position
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, patternTime, context))
        {
            createBullet(spawn.Position, spawn.Direction, spawn.Speed);
        }

        _lastQueryTime = patternTime;
    }
}

public class Bullet : MonoBehaviour
{
    private UnityEngine.Vector2 _direction;
    private float _speed;

    public void Initialize(UnityEngine.Vector2 direction, UnityEngine.Vector2 normalizedDirection, float speed)
    {
        _direction = normalizedDirection;
        _speed = speed;
    }

    public void Update(float deltaTime)
    {
        transform.position += (UnityEngine.Vector3)(_direction * _speed * deltaTime);
    }
}
```

---

## Generic .NET

### Using System.Numerics Directly

If you're using `System.Numerics.Vector2` directly (or another framework that uses it), no conversion is needed:

```csharp
using BenedictBulletHell.Patterns;
using BenedictBulletHell.Patterns.Core;
using System.Numerics;

public class GenericEnemy
{
    private IBulletPattern _pattern;
    private Vector2 _position; // System.Numerics.Vector2

    public void Update(float currentTime)
    {
        var context = new PatternContext
        {
            Origin = _position // Direct use, no conversion needed
        };

        foreach (var spawn in _pattern.GetSpawns(_lastQueryTime, currentTime, context))
        {
            // Direct use of spawn.Position and spawn.Direction
            CreateBullet(spawn.Position, spawn.Direction, spawn.Speed);
        }
    }
}
```

---

## Custom Vector Types

If you're using a custom vector type, you'll need to convert manually:

```csharp
// Custom vector type
public struct MyVector2
{
    public float X, Y;
    public MyVector2(float x, float y) { X = x; Y = y; }
}

// Conversion helpers
public static class VectorConversions
{
    public static System.Numerics.Vector2 ToNumerics(this MyVector2 v)
    {
        return new System.Numerics.Vector2(v.X, v.Y);
    }

    public static MyVector2 FromNumerics(System.Numerics.Vector2 v)
    {
        return new MyVector2(v.X, v.Y);
    }
}

// Usage
var myPos = new MyVector2(100, 200);
var numericsPos = myPos.ToNumerics();

var context = new PatternContext
{
    Origin = numericsPos
};

foreach (var spawn in pattern.GetSpawns(...))
{
    var mySpawnPos = VectorConversions.FromNumerics(spawn.Position);
    // Use mySpawnPos
}
```

---

## Performance Considerations

### Vector Conversion Overhead

Vector conversions are lightweight (just copying two floats), but if you're creating many bullets per frame, consider:

1. **Batch conversions** - Convert once per pattern query, not per spawn
2. **Cache conversions** - If positions don't change often, cache converted vectors
3. **Direct use** - Use `System.Numerics.Vector2` in your game code if possible

### Example: Batch Conversion

```csharp
// Convert origin once
var origin = enemyPosition.ToXna(); // or ToUnity()
var numericsOrigin = new System.Numerics.Vector2(origin.X, origin.Y);

var context = new PatternContext { Origin = numericsOrigin };

// Process spawns
foreach (var spawn in pattern.GetSpawns(...))
{
    // Convert spawn position
    var bulletPos = spawn.Position.ToXna();
    // ...
}
```

---

## Common Integration Patterns

### Pattern Manager

Centralize pattern management:

```csharp
public class PatternManager
{
    private Dictionary<string, IBulletPattern> _patterns = new();

    public void RegisterPattern(string name, IBulletPattern pattern)
    {
        _patterns[name] = pattern;
    }

    public IBulletPattern GetPattern(string name)
    {
        return _patterns[name];
    }

    public IEnumerable<BulletSpawn> QueryPattern(
        string name,
        float lastTime,
        float currentTime,
        PatternContext context)
    {
        var pattern = GetPattern(name);
        return pattern.GetSpawns(lastTime, currentTime, context);
    }
}
```

### Bullet Factory

Separate bullet creation from pattern logic:

```csharp
public interface IBulletFactory
{
    void CreateBullet(Vector2 position, Vector2 direction, float speed);
}

public class GameBulletFactory : IBulletFactory
{
    public void CreateBullet(Vector2 position, Vector2 direction, float speed)
    {
        // Framework-specific bullet creation
        var bullet = new Bullet
        {
            Position = position.ToXna(), // or ToUnity()
            Direction = direction.ToXna(),
            Speed = speed
        };
        // Add to game world
    }
}
```

---

## Next Steps

- **[Examples](Examples)** - See complete integration examples
- **[Best Practices](Best-Practices)** - Performance and design tips
- **[Troubleshooting](Troubleshooting)** - Common integration issues

