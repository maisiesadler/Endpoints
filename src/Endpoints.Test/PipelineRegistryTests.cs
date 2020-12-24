using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Extensions;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Endpoints.Api.Domain;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Test
{
    public class PipelineRegistryTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public PipelineRegistryTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task TransientDependenciesAreResolvedForEachRequest()
        {
            // Arrange
            using var server = _fixture.CreateServer(services =>
            {
                services.AddTransient<TransientRetriever>();
                services.AddPipeline<ModelRequest, ModelResponse>(
                    ModelParser.ParseModel,
                    ModelParser.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<TransientRetriever, ModelRequest, ModelResponse>());
            }));
            var client = server.CreateClient();

            var response = await client.GetAsync("/test");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("1", content);

            // Act
            response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            content = await response.Content.ReadAsStringAsync();
            Assert.Equal("1", content);
        }

        public class TransientRetriever : IRetriever<ModelRequest, ModelResponse>
        {
            private int _times = 0;

            public Task<PipelineResponse<ModelResponse>> Retrieve(ModelRequest input)
            {
                _times++;

                return Task.FromResult(PipelineResponse.Ok(new ModelResponse{ Name = _times.ToString() }));
            }
        }

        public static class ModelParser
        {
            public static ModelRequest ParseModel(HttpContext context)
            {
                return new ModelRequest
                {
                    Id = context.Request.RouteValues["id"]?.ToString(),
                };
            }

            public static async Task ParseResponse(HttpContext context, ModelResponse response)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(response.Name);
            }
        }
    }
}
