using System.Numerics;
using BenedictBulletHell.Patterns.Core;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Core
{
    public class BulletSpawnTests
    {
        [Fact]
        public void Position_CanBeSetAndRetrieved()
        {
            var position = new Vector2(100, 200);
            var spawn = new BulletSpawn { Position = position };

            Assert.Equal(position, spawn.Position);
        }

        [Fact]
        public void Direction_CanBeSetAndRetrieved()
        {
            var direction = new Vector2(1, 0);
            var spawn = new BulletSpawn { Direction = direction };

            Assert.Equal(direction, spawn.Direction);
        }

        [Fact]
        public void Speed_CanBeSetAndRetrieved()
        {
            var speed = 150f;
            var spawn = new BulletSpawn { Speed = speed };

            Assert.Equal(speed, spawn.Speed);
        }

        [Fact]
        public void Angle_CanBeSetAndRetrieved()
        {
            var angle = 45f;
            var spawn = new BulletSpawn { Angle = angle };

            Assert.Equal(angle, spawn.Angle);
        }

        [Fact]
        public void BulletData_CanBeSetAndRetrieved()
        {
            var data = "test data";
            var spawn = new BulletSpawn { BulletData = data };

            Assert.Equal(data, spawn.BulletData);
        }

        [Fact]
        public void BulletData_CanBeNull()
        {
            var spawn = new BulletSpawn { BulletData = null };

            Assert.Null(spawn.BulletData);
        }

        [Fact]
        public void BulletData_CanStoreComplexObjects()
        {
            var data = new { Type = "PlayerBullet", Damage = 10 };
            var spawn = new BulletSpawn { BulletData = data };

            Assert.NotNull(spawn.BulletData);
            Assert.Equal(data, spawn.BulletData);
        }
    }
}


