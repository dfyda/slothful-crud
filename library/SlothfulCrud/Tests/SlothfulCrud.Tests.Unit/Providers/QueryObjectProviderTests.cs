using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Tests.Unit.Providers
{
    public class QueryObjectProviderTests
    {
        private const string RowsQueryKey = "Rows";
        private const string SortByQueryKey = "SortBy";
        private const string IsActiveQueryKey = "IsActive";
        private const string PageQueryKey = "Page";
        private const string CustomQueryKey = "Custom";
        private const int ExpectedRows = 25;
        private const string ExpectedSortBy = "Name";
        private const bool ExpectedIsActive = true;
        private const int ExpectedPage = 2;

        [Fact]
        public void PrepareQueryObject_ShouldParsePrimitiveAndNullableTypes_WhenValuesAreProvided()
        {
            // Arrange
            var context = CreateHttpContext(new Dictionary<string, StringValues>
            {
                [RowsQueryKey] = ExpectedRows.ToString(),
                [SortByQueryKey] = ExpectedSortBy,
                [IsActiveQueryKey] = ExpectedIsActive.ToString().ToLowerInvariant(),
                [PageQueryKey] = ExpectedPage.ToString()
            });

            // Act
            var query = QueryObjectProvider.PrepareQueryObject<QueryModel>(context);

            // Assert
            Assert.Equal(ExpectedRows, query.Rows);
            Assert.Equal(ExpectedSortBy, query.SortBy);
            Assert.Equal(ExpectedIsActive, query.IsActive);
            Assert.Equal(ExpectedPage, query.Page);
        }

        [Fact]
        public void PrepareQueryObject_ShouldKeepDefaults_WhenValueIsMissing()
        {
            // Arrange
            var context = CreateHttpContext(new Dictionary<string, StringValues>());

            // Act
            var query = QueryObjectProvider.PrepareQueryObject<QueryModel>(context);

            // Assert
            Assert.Equal(0, query.Rows);
            Assert.Null(query.SortBy);
            Assert.False(query.IsActive);
            Assert.Null(query.Page);
        }

        [Fact]
        public void PrepareQueryObject_ShouldSetNullableValueToNull_WhenEmptyStringProvided()
        {
            // Arrange
            var context = CreateHttpContext(new Dictionary<string, StringValues>
            {
                [PageQueryKey] = string.Empty
            });

            // Act
            var query = QueryObjectProvider.PrepareQueryObject<QueryModel>(context);

            // Assert
            Assert.Null(query.Page);
        }

        [Fact]
        public void PrepareQueryObject_ShouldThrowTargetInvocationException_WhenValueCannotBeParsed()
        {
            // Arrange
            var context = CreateHttpContext(new Dictionary<string, StringValues>
            {
                [RowsQueryKey] = "not-a-number"
            });

            // Act + Assert
            var exception = Assert.Throws<TargetInvocationException>(() =>
                QueryObjectProvider.PrepareQueryObject<QueryModel>(context));
            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void PrepareQueryObject_ShouldThrowConfigurationException_WhenTypeHasNoParseMethod()
        {
            // Arrange
            var context = CreateHttpContext(new Dictionary<string, StringValues>
            {
                [CustomQueryKey] = "x"
            });

            // Act + Assert
            Assert.Throws<ConfigurationException>(() =>
                QueryObjectProvider.PrepareQueryObject<QueryWithoutParse>(context));
        }

        private static HttpContext CreateHttpContext(Dictionary<string, StringValues> query)
        {
            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(query);
            return context;
        }

        private class QueryModel
        {
            public int Rows { get; set; }
            public string SortBy { get; set; }
            public bool IsActive { get; set; }
            public int? Page { get; set; }
        }

        private class QueryWithoutParse
        {
            public NoParseType Custom { get; set; }
        }

        private class NoParseType
        {
        }
    }
}
