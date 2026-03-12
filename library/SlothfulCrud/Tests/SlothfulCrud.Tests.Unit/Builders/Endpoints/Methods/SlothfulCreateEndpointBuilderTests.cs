using SlothfulCrud.Builders.Endpoints.Methods;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Tests.Unit.Builders.Endpoints.Methods
{
    public class SlothfulCreateEndpointBuilderTests
    {
        [Fact]
        public void BuildCreatedLocation_ShouldBuildRelativePath_WhenSegmentHasNoLeadingSlash()
        {
            // Arrange
            var key = Guid.NewGuid();

            // Act
            var result = SlothfulCreateEndpointBuilder<Sloth>.BuildCreatedLocation("sloths", key);

            // Assert
            Assert.Equal($"/sloths/{key}", result);
        }

        [Fact]
        public void BuildCreatedLocation_ShouldBuildRelativePath_ForCustomSegmentProviderStyle()
        {
            // Act
            var result = SlothfulCreateEndpointBuilder<Sloth>.BuildCreatedLocation("custom-segment", 123);

            // Assert
            Assert.Equal("/custom-segment/123", result);
        }

        [Fact]
        public void BuildCreatedLocation_ShouldNotDuplicateSlash_WhenSegmentAlreadyHasLeadingSlash()
        {
            // Act
            var result = SlothfulCreateEndpointBuilder<Sloth>.BuildCreatedLocation("/sloths", 10);

            // Assert
            Assert.Equal("/sloths/10", result);
        }

        [Fact]
        public void BuildCreatedLocation_ShouldThrowArgumentException_WhenSegmentIsWhitespace()
        {
            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
                SlothfulCreateEndpointBuilder<Sloth>.BuildCreatedLocation(" ", 1));
        }

        [Fact]
        public void BuildCreatedLocation_ShouldThrowArgumentNullException_WhenKeyIsNull()
        {
            // Act + Assert
            Assert.Throws<ArgumentNullException>(() =>
                SlothfulCreateEndpointBuilder<Sloth>.BuildCreatedLocation("sloths", null));
        }
    }
}
