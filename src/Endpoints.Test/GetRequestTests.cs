using System.Threading.Tasks;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace Endpoints.Test
{
    public class GetRequestTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public GetRequestTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetWithNoParameters()
        {
            // Arrange
            using var server = _fixture.CreateServer();
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello!", content);
        }

        [Theory]
        [InlineData("test1")]
        [InlineData("anothername")]
        public async Task GetWithStringParameter(string @param)
        {
            // Arrange
            using var server = _fixture.CreateServer();
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync($"/test/{@param}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello! " + @param, content);
        }

        [Fact]
        public async Task GetWithMultipleStringParameters()
        {
            // Arrange
            using var server = _fixture.CreateServer();
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("/test/test/one/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Params are 'test' and 'one'", content);
        }
    }
}
