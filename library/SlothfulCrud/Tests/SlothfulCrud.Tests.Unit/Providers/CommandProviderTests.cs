using SlothfulCrud.Providers.Types;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Exceptions;

namespace SlothfulCrud.Tests.Unit.Providers
{
    public class CommandProviderTests
    {
        [Fact]
        public void PrepareCreateCommand_ShouldExcludeKeyProperty_FromCreatePayload()
        {
            // Arrange
            var constructor = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) })!;

            // Act
            var commandType = CommandProvider.PrepareCreateCommand(constructor, typeof(Sloth));
            var propertyNames = commandType.GetProperties().Select(x => x.Name).ToList();

            // Assert
            Assert.DoesNotContain(propertyNames, x => x.Equals("id", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(propertyNames, x => x.Equals("name", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(propertyNames, x => x.Equals("age", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void PrepareCreateCommand_ShouldKeepNestedEntityIds_AndExcludeRootKey()
        {
            // Arrange
            var constructor = typeof(WildKoala).GetConstructor(
                [typeof(Guid), typeof(string), typeof(int), typeof(Sloth), typeof(Sloth)])!;

            // Act
            var commandType = CommandProvider.PrepareCreateCommand(constructor, typeof(WildKoala));
            var propertyNames = commandType.GetProperties().Select(x => x.Name).ToList();

            // Assert
            Assert.DoesNotContain(propertyNames, x => x.Equals("id", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(propertyNames, x => x.Equals("name", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(propertyNames, x => x.Equals("age", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(propertyNames, x => x.Equals("cuisineId", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(propertyNames, x => x.Equals("neighbourId", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void PrepareCreateCommand_ShouldThrowConfigurationException_WhenConstructorHasNoParameters()
        {
            // Arrange
            var constructor = typeof(SlothWithoutCreateParams).GetConstructor(Type.EmptyTypes)!;

            // Act + Assert
            var exception = Assert.Throws<ConfigurationException>(() =>
                CommandProvider.PrepareCreateCommand(constructor, typeof(SlothWithoutCreateParams)));
            Assert.Contains("must contain generated key as first parameter", exception.Message);
        }

        private sealed class SlothWithoutCreateParams
        {
            public SlothWithoutCreateParams()
            {
            }
        }
    }
}
