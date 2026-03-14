using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Tests.Api.EF;

namespace SlothfulCrud.Tests.Unit.Integration
{
    public class CrudEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private const string SlothsBaseUrl = "/sloths";
        private const string WildKoalasBaseUrl = "/wild-koalas";
        private const string SlothName = "TestSloth";
        private const string UpdatedSlothName = "UpdatedSloth";
        private const int SlothAge = 5;
        private const int UpdatedSlothAge = 10;
        private const string KoalaName = "TestKoala";
        private const string UpdatedKoalaName = "UpdatedKoala";
        private const int KoalaAge = 3;
        private const int UpdatedKoalaAge = 7;
        private const string ProblemJsonMediaType = "application/problem+json";
        private const string ProblemCodeProperty = "Code";
        private const string ProblemStatusProperty = "status";
        private const string ProblemIdProperty = "ProblemId";
        private const string ValidationErrorsProperty = "validationErrors";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CrudEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SlothfulDbContext>();
            db.Koalas.RemoveRange(db.Koalas);
            db.Sloths.RemoveRange(db.Sloths);
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task CreateSloth_ShouldReturn201_WhenCommandIsValid()
        {
            var command = new { name = SlothName, age = SlothAge };

            var response = await _client.PostAsJsonAsync(SlothsBaseUrl, command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var id = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        public async Task GetWildKoala_ShouldReturn200_WhenEntityExists()
        {
            var koalaId = await CreateWildKoalaAsync();

            var response = await _client.GetAsync($"{WildKoalasBaseUrl}/{koalaId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(KoalaName, body);
        }

        [Fact]
        public async Task GetWildKoala_ShouldReturn404_WhenEntityDoesNotExist()
        {
            var response = await _client.GetAsync($"{WildKoalasBaseUrl}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task BrowseWildKoalas_ShouldReturnPagedResults()
        {
            await CreateWildKoalaAsync();

            var response = await _client.GetAsync($"{WildKoalasBaseUrl}/list/1?Rows=10&SortBy=Name&SortDirection=asc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(KoalaName, body);
        }

        [Fact]
        public async Task BrowseSelectableSloth_ShouldReturnPagedResults()
        {
            await CreateSlothAsync();

            var response = await _client.GetAsync($"{SlothsBaseUrl}/selectable-list/1?Rows=10&SortBy=Name&SortDirection=asc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(SlothName, body);
        }

        [Fact]
        public async Task UpdateSloth_ShouldReturn204_WhenCommandIsValid()
        {
            var slothId = await CreateSlothAsync();
            var updateCommand = new { name = UpdatedSlothName, age = UpdatedSlothAge };

            var response = await _client.PutAsJsonAsync($"{SlothsBaseUrl}/{slothId}", updateCommand);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task UpdateWildKoala_ShouldModifyEntity()
        {
            var koalaId = await CreateWildKoalaAsync();
            var updateCommand = new { name = UpdatedKoalaName, age = UpdatedKoalaAge, cuisineId = await GetOrCreateCuisineIdAsync(), neighbourId = (Guid?)null };

            var response = await _client.PutAsJsonAsync($"{WildKoalasBaseUrl}/{koalaId}", updateCommand);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await _client.GetAsync($"{WildKoalasBaseUrl}/{koalaId}");
            var body = await getResponse.Content.ReadAsStringAsync();
            Assert.Contains(UpdatedKoalaName, body);
        }

        [Fact]
        public async Task DeleteSloth_ShouldReturn204_WhenEntityExists()
        {
            var slothId = await CreateSlothAsync();

            var response = await _client.DeleteAsync($"{SlothsBaseUrl}/{slothId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteWildKoala_ShouldReturn204_ThenGetReturns404()
        {
            var koalaId = await CreateWildKoalaAsync();

            var deleteResponse = await _client.DeleteAsync($"{WildKoalasBaseUrl}/{koalaId}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync($"{WildKoalasBaseUrl}/{koalaId}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task CreateSloth_ShouldReturn400_WhenValidationFails()
        {
            var command = new { name = "", age = SlothAge };

            var response = await _client.PostAsJsonAsync(SlothsBaseUrl, command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetSlothDetails_ShouldNotReturn200_WhenEndpointIsDisabled()
        {
            var slothId = await CreateSlothAsync();

            var response = await _client.GetAsync($"{SlothsBaseUrl}/{slothId}");

            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task BrowseSloths_ShouldReturn404_WhenEndpointIsDisabled()
        {
            var response = await _client.GetAsync($"{SlothsBaseUrl}/list/1?Rows=10&SortBy=Name&SortDirection=asc");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateSloth_ShouldReturnProblemJsonContract_WhenValidationFails()
        {
            var command = new { name = "", age = SlothAge };

            var response = await _client.PostAsJsonAsync(SlothsBaseUrl, command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ProblemJsonMediaType, response.Content.Headers.ContentType?.MediaType);

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.True(root.TryGetProperty(ProblemCodeProperty, out _));
            Assert.Equal(StatusCodes.Status400BadRequest, root.GetProperty(ProblemStatusProperty).GetInt32());
            Assert.True(root.TryGetProperty(ProblemIdProperty, out var problemId));
            Assert.NotEqual(Guid.Empty, problemId.GetGuid());
            Assert.True(root.TryGetProperty(ValidationErrorsProperty, out _));
        }

        [Fact]
        public async Task GetWildKoala_ShouldReturnProblemJsonContract_WhenEntityDoesNotExist()
        {
            var response = await _client.GetAsync($"{WildKoalasBaseUrl}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ProblemJsonMediaType, response.Content.Headers.ContentType?.MediaType);

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.True(root.TryGetProperty(ProblemCodeProperty, out _));
            Assert.Equal(StatusCodes.Status404NotFound, root.GetProperty(ProblemStatusProperty).GetInt32());
            Assert.True(root.TryGetProperty(ProblemIdProperty, out _));
        }

        [Fact]
        public async Task UpdateWildKoala_ShouldReturn404_WhenEntityDoesNotExist()
        {
            var cuisineId = await GetOrCreateCuisineIdAsync();
            var updateCommand = new { name = UpdatedKoalaName, age = UpdatedKoalaAge, cuisineId, neighbourId = (Guid?)null };

            var response = await _client.PutAsJsonAsync($"{WildKoalasBaseUrl}/{Guid.NewGuid()}", updateCommand);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteWildKoala_ShouldReturn404_WhenEntityDoesNotExist()
        {
            var response = await _client.DeleteAsync($"{WildKoalasBaseUrl}/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateSloth_ShouldReturnLocationHeader_WhenCreated()
        {
            var command = new { name = SlothName, age = SlothAge };

            var response = await _client.PostAsJsonAsync(SlothsBaseUrl, command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains(SlothsBaseUrl, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task BrowseWildKoalas_ShouldReturnCorrectPaginationMetadata()
        {
            for (int i = 0; i < 5; i++)
            {
                await CreateWildKoalaAsync();
            }

            var response = await _client.GetAsync($"{WildKoalasBaseUrl}/list/1?Rows=2&SortBy=Name&SortDirection=asc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.True(TryGetPropertyCaseInsensitive(root, "rows", out var rowsProp));
            Assert.Equal(2, rowsProp.GetInt32());
            Assert.True(TryGetPropertyCaseInsensitive(root, "total", out var totalProp));
            Assert.True(totalProp.GetInt32() >= 5);
            Assert.True(TryGetPropertyCaseInsensitive(root, "data", out var dataProp));
            Assert.Equal(2, dataProp.GetArrayLength());
        }

        [Fact]
        public async Task BrowseSelectableSloth_ShouldReturnSortedResults_Desc()
        {
            for (int i = 0; i < 3; i++)
            {
                var cmd = new { name = $"SortSloth{i}", age = SlothAge + i };
                await _client.PostAsJsonAsync(SlothsBaseUrl, cmd);
            }

            var response = await _client.GetAsync($"{SlothsBaseUrl}/selectable-list/1?Rows=10&SortBy=Name&SortDirection=desc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            Assert.True(TryGetPropertyCaseInsensitive(root, "data", out var data));
            Assert.True(data.GetArrayLength() >= 3);

            var names = data.EnumerateArray()
                .Select(e =>
                {
                    TryGetPropertyCaseInsensitive(e, "displayName", out var dn);
                    return dn.GetString();
                })
                .ToList();
            var sortedDesc = names.OrderByDescending(n => n).ToList();
            Assert.Equal(sortedDesc, names);
        }

        private async Task<Guid> CreateSlothAsync()
        {
            var command = new { name = SlothName, age = SlothAge };
            var response = await _client.PostAsJsonAsync(SlothsBaseUrl, command);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        }

        private async Task<Guid> GetOrCreateCuisineIdAsync()
        {
            var command = new { name = "CuisineSloth", age = 2 };
            var response = await _client.PostAsJsonAsync(SlothsBaseUrl, command);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        }

        private async Task<Guid> CreateWildKoalaAsync()
        {
            var cuisineId = await GetOrCreateCuisineIdAsync();
            var command = new { name = KoalaName, age = KoalaAge, cuisineId = cuisineId };
            var response = await _client.PostAsJsonAsync(WildKoalasBaseUrl, command);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        }

        private static bool TryGetPropertyCaseInsensitive(JsonElement element, string name, out JsonElement value)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}