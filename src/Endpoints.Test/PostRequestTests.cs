// using System.Threading.Tasks;
// using System.Net;
// using Xunit;
// using Xunit.Abstractions;

// namespace Endpoints.Test
// {
//     public class PostRequestTests : IClassFixture<TestFixture>
//     {
//         private readonly TestFixture _fixture;

//         public PostRequestTests(TestFixture fixture)
//         {
//             _fixture = fixture;
//         }

//         [Fact]
//         public async Task PostWithNoParameters()
//         {
//             // Arrange
//             using var server = _fixture.CreateServer();
//             var client = server.CreateClient();

//             // Act
//             var response = await client.PostAsync("/test", null);

//             // Assert
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//             var content = await response.Content.ReadAsStringAsync();
//             Assert.Equal("Hello!", content);
//         }

//         [Theory]
//         [InlineData("test1")]
//         [InlineData("anothername")]
//         public async Task PostWithStringParameter(string @param)
//         {
//             // Arrange
//             using var server = _fixture.CreateServer();
//             var client = server.CreateClient();

//             // Act
//             var response = await client.PostAsync($"/test/{@param}", null);

//             // Assert
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//             var content = await response.Content.ReadAsStringAsync();
//             Assert.Equal("Hello! " + @param, content);
//         }
//     }
// }
