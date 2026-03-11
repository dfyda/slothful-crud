using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Tests.Unit.Configurations
{
    public class SlothEntityBuilderTests
    {
        [Fact]
        public void SetKeyPropertyType_ShouldSetGuidType_WhenGuidKeyExpressionIsProvided()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder
                .SetKeyPropertyType(x => x.Id)
                .Build();

            // Assert
            Assert.Equal(typeof(Guid), configuration.Entity.KeyPropertyType);
        }

        [Fact]
        public void SetKeyPropertyType_ShouldSetLongType_WhenLongKeyExpressionIsProvided()
        {
            // Arrange
            var builder = new SlothEntityBuilder<LongEntity>();

            // Act
            var configuration = builder
                .SetKeyPropertyType(x => x.Id)
                .Build();

            // Assert
            Assert.Equal(typeof(long), configuration.Entity.KeyPropertyType);
        }

        [Fact]
        public void SetKeyProperty_ShouldSetKeyNameAndType_WhenLongKeyExpressionIsProvided()
        {
            // Arrange
            var builder = new SlothEntityBuilder<LongEntity>();

            // Act
            var configuration = builder
                .SetKeyProperty(x => x.Id)
                .Build();

            // Assert
            Assert.Equal(nameof(LongEntity.Id), configuration.Entity.KeyProperty);
            Assert.Equal(typeof(long), configuration.Entity.KeyPropertyType);
        }

        private class GuidEntity : ISlothfulEntity
        {
            public Guid Id { get; init; }
            public string DisplayName => "GuidEntity";
        }

        private class LongEntity : ISlothfulEntity
        {
            public long Id { get; init; }
            public string DisplayName => "LongEntity";
        }
    }
}
