using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Configurations
{
    public class SlothEntityBuilderTests
    {
        private const string PatchMethodName = "Patch";
        private const string PolicyA = "Policy.A";
        private const string PolicyB = "Policy.B";
        private const string WhitespaceValue = "   ";
        private const string DisplayNameSuffix = "_suffix";
        private const string GuidEntityDisplayName = "GuidEntity";
        private const string LongEntityDisplayName = "LongEntity";

        [Fact]
        public void Build_ShouldReturnDefaultConfiguration_WhenNoCustomizationIsApplied()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder.Build();

            // Assert - entity defaults
            Assert.Equal("Name", configuration.Entity.SortProperty);
            Assert.Equal("Name", configuration.Entity.FilterProperty);
            Assert.Equal("Update", configuration.Entity.UpdateMethod);
            Assert.Equal("Id", configuration.Entity.KeyProperty);
            Assert.Equal(typeof(Guid), configuration.Entity.KeyPropertyType);
            Assert.True(configuration.Entity.HasValidation);
            Assert.False(configuration.Entity.ExposeAllNestedProperties);
            Assert.False(configuration.Entity.IsAuthorizationEnable);
            Assert.Empty(configuration.Entity.PolicyNames);

            // Assert - endpoint defaults
            AssertEndpointDefaults(configuration.Get);
            AssertEndpointDefaults(configuration.Browse);
            AssertEndpointDefaults(configuration.BrowseSelectable);
            AssertEndpointDefaults(configuration.Create);
            AssertEndpointDefaults(configuration.Update);
            AssertEndpointDefaults(configuration.Delete);
        }

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

        [Fact]
        public void SetSortProperty_ShouldSetSortProperty_WhenValidPropertyExpressionIsProvided()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder
                .SetSortProperty(x => x.DisplayName)
                .Build();

            // Assert
            Assert.Equal(nameof(GuidEntity.DisplayName), configuration.Entity.SortProperty);
        }

        [Fact]
        public void SetFilterProperty_ShouldSetFilterProperty_WhenValidPropertyExpressionIsProvided()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder
                .SetFilterProperty(x => x.DisplayName)
                .Build();

            // Assert
            Assert.Equal(nameof(GuidEntity.DisplayName), configuration.Entity.FilterProperty);
        }

        [Fact]
        public void SetUpdateMethodName_ShouldSetUpdateMethod_WhenMethodNameIsProvided()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder
                .SetUpdateMethodName(PatchMethodName)
                .Build();

            // Assert
            Assert.Equal(PatchMethodName, configuration.Entity.UpdateMethod);
        }

        [Fact]
        public void SetUpdateMethodName_ShouldThrowArgumentException_WhenMethodNameIsNull()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act + Assert
            Assert.Throws<ArgumentException>(() => builder.SetUpdateMethodName(null));
        }

        [Fact]
        public void SetUpdateMethodName_ShouldThrowArgumentException_WhenMethodNameIsWhitespace()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act + Assert
            Assert.Throws<ArgumentException>(() => builder.SetUpdateMethodName(WhitespaceValue));
        }

        [Fact]
        public void HasValidation_ShouldDisableValidation_WhenSetToFalse()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder
                .HasValidation(false)
                .Build();

            // Assert
            Assert.False(configuration.Entity.HasValidation);
        }

        [Fact]
        public void RequireAuthorization_ShouldEnableAuthorizationAndSetPolicies_OnEntityConfiguration()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder
                .RequireAuthorization(PolicyA, PolicyB)
                .Build();

            // Assert
            Assert.True(configuration.Entity.IsAuthorizationEnable);
            Assert.Equal(new[] { PolicyA, PolicyB }, configuration.Entity.PolicyNames);
        }

        [Fact]
        public void AllowAnonymous_ShouldDisableAuthorization_OnEntityConfiguration()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder
                .RequireAuthorization(PolicyA)
                .AllowAnonymous()
                .Build();

            // Assert
            Assert.False(configuration.Entity.IsAuthorizationEnable);
        }

        [Fact]
        public void ExposeAllNestedProperties_ShouldSetFlag_WhenEnabled()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act
            var configuration = builder
                .ExposeAllNestedProperties(true)
                .Build();

            // Assert
            Assert.True(configuration.Entity.ExposeAllNestedProperties);
        }

        [Fact]
        public void SetSortProperty_ShouldThrowArgumentException_WhenExpressionIsNotPropertyAccess()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act + Assert
            Assert.Throws<ArgumentException>(() => builder.SetSortProperty(x => x.DisplayName + DisplayNameSuffix));
        }

        [Fact]
        public void SetFilterProperty_ShouldThrowArgumentException_WhenExpressionIsNotPropertyAccess()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act + Assert
            Assert.Throws<ArgumentException>(() => builder.SetFilterProperty(x => x.DisplayName + DisplayNameSuffix));
        }

        [Fact]
        public void SetKeyProperty_ShouldThrowArgumentException_WhenExpressionIsNotPropertyAccess()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act + Assert
            Assert.Throws<ArgumentException>(() => builder.SetKeyProperty(x => x.Id.ToString()));
        }

        [Fact]
        public void SetKeyPropertyType_ShouldThrowArgumentException_WhenExpressionIsNotPropertyAccess()
        {
            // Arrange
            var builder = new SlothEntityBuilder<GuidEntity>();

            // Act + Assert
            Assert.Throws<ArgumentException>(() => builder.SetKeyPropertyType(x => x.Id.ToString()));
        }

        private static void AssertEndpointDefaults(EndpointConfiguration endpointConfiguration)
        {
            Assert.True(endpointConfiguration.IsEnable);
            Assert.False(endpointConfiguration.ExposeAllNestedProperties);
            Assert.False(endpointConfiguration.IsAuthorizationEnable);
            Assert.Empty(endpointConfiguration.PolicyNames);
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
