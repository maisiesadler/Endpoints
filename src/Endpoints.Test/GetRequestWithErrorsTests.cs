using System.Threading.Tasks;
using System.Net;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using System;
using Moq;
using Endpoints.Extensions;
using Endpoints.Api.Domain;

namespace Endpoints.Test
{
    public class GetRequestWithErrorsTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;

        public GetRequestWithErrorsTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        public class TestGetRetriever : IRetriever<ModelRequest, ModelResponse>
        {
            private readonly IDbThing _dbThing;

            public TestGetRetriever(IDbThing dbThing)
            {
                _dbThing = dbThing;
            }

            public async Task<PipelineResponse<ModelResponse>> Retrieve(ModelRequest input)
            {
                try
                {
                    var response = await _dbThing.GetModel(input);
                    return PipelineResponse.Ok<ModelResponse>(response);
                }
                catch (Exception ex)
                {
                    return PipelineResponse.Fail<ModelResponse>(ex, "Unhandled exception");
                }
            }

            public static ModelRequest ParseModel(HttpContext context)
            {
                return new ModelRequest
                {
                    Id = context.Request.RouteValues["id"]?.ToString(),
                };
            }

            public static async Task ParseResponse(HttpContext context, PipelineResponse<ModelResponse> response)
            {
                if (response.Success)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.WriteAsync(response.Result.Name);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync(response.Error.ErrorMessage);
                }
            }
        }

        [Fact]
        public async Task GetWithNoErrorsReturnsOk()
        {
            // Arrange
            var dbThing = new Mock<IDbThing>();
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing>(dbThing.Object);
                services.AddTransient<TestGetRetriever>();
                services.AddPipeline<ModelRequest, ModelResponse>(
                    TestGetRetriever.ParseModel,
                    TestGetRetriever.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<TestGetRetriever, ModelRequest, ModelResponse>());
            }));
            var client = server.CreateClient();

            dbThing.Setup(t => t.GetModel(It.IsAny<ModelRequest>()))
                .Returns(Task.FromResult(new ModelResponse { Name = "new model name" }));

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("new model name", content);
        }

        [Fact]
        public async Task GetWithErrorsReturnsErrorResponse()
        {
            // Arrange
            var dbThing = new Mock<IDbThing>();
            using var server = _fixture.CreateServer(services =>
            {
                services.AddSingleton<IDbThing>(dbThing.Object);
                services.AddTransient<TestGetRetriever>();
                services.AddPipeline<ModelRequest, ModelResponse>(
                    TestGetRetriever.ParseModel,
                    TestGetRetriever.ParseResponse
                );
            },
            app => app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/test", endpoints.ServiceProvider.Get<TestGetRetriever, ModelRequest, ModelResponse>());
            }));
            var client = server.CreateClient();

            dbThing.Setup(t => t.GetModel(It.IsAny<ModelRequest>()))
                .ThrowsAsync(new InvalidOperationException());

            // Act
            var response = await client.GetAsync("/test");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Unhandled exception", content);
        }
    }
}
