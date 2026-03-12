using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Providers
{
    public class EntityPropertyKeyValueProviderTests
    {
        private const string GuidEntityDisplayName = "GuidEntity";
        private const string LongEntityDisplayName = "LongEntity";

        [Fact]
        public void GetNextValue_ShouldReturnGuid_WhenKeyTypeIsGuid()
        {
            // Arrange
            var configuration = new EntityConfiguration();
            configuration.SetKeyPropertyType(typeof(Guid));
            var provider = new EntityPropertyKeyValueProvider<GuidEntity>();

            // Act
            var value = provider.GetNextValue(configuration);

            // Assert
            Assert.IsType<Guid>(value);
            Assert.NotEqual(Guid.Empty, (Guid)value);
        }

        [Fact]
        public void GetNextValue_ShouldThrowConfigurationException_WhenKeyTypeIsLong()
        {
            // Arrange
            var configuration = new EntityConfiguration();
            configuration.SetKeyPropertyType(typeof(long));
            var provider = new EntityPropertyKeyValueProvider<LongEntity>();

            // Act + Assert
            Assert.Throws<ConfigurationException>(() => provider.GetNextValue(configuration));
        }

        private class GuidEntity : ISlothfulEntity
        {
            public Guid Id { get; init; }
            public string DisplayName => GuidEntityDisplayName;
        }

        private class LongEntity : ISlothfulEntity
        {
            public long Id { get; init; }
            public string DisplayName => LongEntityDisplayName;
        }
    }
}
