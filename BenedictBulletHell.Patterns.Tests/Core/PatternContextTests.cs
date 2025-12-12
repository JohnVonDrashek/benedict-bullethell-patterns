using System.Collections.Generic;
using System.Numerics;
using BenedictBulletHell.Patterns.Core;
using Xunit;

namespace BenedictBulletHell.Patterns.Tests.Core
{
    public class PatternContextTests
    {
        [Fact]
        public void Origin_CanBeSetAndRetrieved()
        {
            var origin = new Vector2(100, 200);
            var context = new PatternContext { Origin = origin };

            Assert.Equal(origin, context.Origin);
        }

        [Fact]
        public void Target_CanBeSetAndRetrieved()
        {
            var target = new Vector2(300, 400);
            var context = new PatternContext { Target = target };

            Assert.Equal(target, context.Target);
        }

        [Fact]
        public void Target_CanBeNull()
        {
            var context = new PatternContext { Target = null };

            Assert.Null(context.Target);
        }

        [Fact]
        public void PatternAge_CanBeSetAndRetrieved()
        {
            var age = 5.5f;
            var context = new PatternContext { PatternAge = age };

            Assert.Equal(age, context.PatternAge);
        }

        [Fact]
        public void Metadata_CanBeSetAndRetrieved()
        {
            var metadata = new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 42
            };
            var context = new PatternContext { Metadata = metadata };

            Assert.Equal(metadata, context.Metadata);
        }

        [Fact]
        public void Metadata_CanBeNull()
        {
            var context = new PatternContext { Metadata = null };

            Assert.Null(context.Metadata);
        }

        [Fact]
        public void GetMetadata_ReturnsValueWhenKeyExists()
        {
            var metadata = new Dictionary<string, object> { ["test"] = "value" };
            var context = new PatternContext { Metadata = metadata };

            var result = context.GetMetadata<string>("test");

            Assert.Equal("value", result);
        }

        [Fact]
        public void GetMetadata_ReturnsNullWhenKeyDoesNotExist()
        {
            var metadata = new Dictionary<string, object> { ["other"] = "value" };
            var context = new PatternContext { Metadata = metadata };

            var result = context.GetMetadata<string>("test");

            Assert.Null(result);
        }

        [Fact]
        public void GetMetadata_ReturnsNullWhenMetadataIsNull()
        {
            var context = new PatternContext { Metadata = null };

            var result = context.GetMetadata<string>("test");

            Assert.Null(result);
        }

        [Fact]
        public void GetMetadataValue_ReturnsValueWhenKeyExists()
        {
            var metadata = new Dictionary<string, object> { ["speed"] = 150f };
            var context = new PatternContext { Metadata = metadata };

            var result = context.GetMetadataValue<float>("speed");

            Assert.Equal(150f, result);
        }

        [Fact]
        public void GetMetadataValue_ReturnsDefaultWhenKeyDoesNotExist()
        {
            var metadata = new Dictionary<string, object> { ["other"] = 42 };
            var context = new PatternContext { Metadata = metadata };

            var result = context.GetMetadataValue<float>("speed");

            Assert.Equal(0f, result);
        }

        [Fact]
        public void GetMetadataValue_ReturnsDefaultWhenMetadataIsNull()
        {
            var context = new PatternContext { Metadata = null };

            var result = context.GetMetadataValue<float>("speed");

            Assert.Equal(0f, result);
        }

        [Fact]
        public void GetMetadataValue_ReturnsProvidedDefaultWhenKeyDoesNotExist()
        {
            var metadata = new Dictionary<string, object>();
            var context = new PatternContext { Metadata = metadata };

            var result = context.GetMetadataValue<float>("speed", 200f);

            Assert.Equal(200f, result);
        }

        [Fact]
        public void GetMetadataValue_HandlesTypeMismatch()
        {
            var metadata = new Dictionary<string, object> { ["value"] = "not a float" };
            var context = new PatternContext { Metadata = metadata };

            var result = context.GetMetadataValue<float>("value", 100f);

            Assert.Equal(100f, result); // Should return default on type mismatch
        }
    }
}


