// using System.Threading.Tasks;
// using System.Net;
// using Xunit;
// using Microsoft.Extensions.DependencyInjection;
// using Endpoints.Api.Pipelines;
// using Microsoft.AspNetCore.Builder;
// using Endpoints.Instructions;
// using Endpoints.Pipelines;
// using Microsoft.AspNetCore.Http;
// using System.Threading;
// using Endpoints.Extensions;

// namespace Endpoints.Test
// {
//     public class GetRequestTests : IClassFixture<TestFixture>
//     {
//         private readonly TestFixture _fixture;

//         public GetRequestTests(TestFixture fixture)
//         {
//             _fixture = fixture;
//         }

//         public class TestGetPipeline : Pipeline<ModelRequest, ModelResponse>
//         {
//             private readonly IDbThing _dbThing;

//             public TestGetPipeline(IDbThing dbThing)
//             {
//                 _dbThing = dbThing;
//             }

//             protected override async Task<ModelResponse> GetResponse(ModelRequest input)
//             {
//                 return await _dbThing.GetModel(input);
//             }

//             protected override ModelRequest ParseModel(HttpContext context)
//             {
//                 return new ModelRequest
//                 {
//                     Id = context.Request.RouteValues["id"]?.ToString(),
//                 };
//             }

//             protected override async Task ParseResponse(HttpContext context, ModelResponse response)
//             {
//                 context.Response.StatusCode = (int)HttpStatusCode.OK;
//                 await context.Response.WriteAsync(response.Name);
//             }
//         }

//         [Fact]
//         public async Task GetWithNoParameters()
//         {
//             // Arrange
//             using var server = _fixture.CreateServer(services =>
//             {
//                 services.AddSingleton<IDbThing, DbThing>();
//                 services.AddTransient<Pipeline<ModelRequest, ModelResponse>, TestGetPipeline>();
//                     // new PipelineInstructions<ModelRequest, ModelResponse>(stages => new MyModelPipeline(stages))
//                     //     .Register<GetModelFromDatabase>()
//                     //     .GetPipeline(sp));
//             },
//             app => app.UseEndpoints(endpoints =>
//             {
//                 endpoints.MapGet("/test", async ctx => await endpoints.ServiceProvider.GetRequiredService<Pipeline<ModelRequest, ModelResponse>>().Run(ctx));
//             }));
//             var client = server.CreateClient();

//             // Act
//             var response = await client.GetAsync("/test");

//             // Assert
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//             var content = await response.Content.ReadAsStringAsync();
//             Assert.Equal("0=", content);
//         }

//         [Theory]
//         [InlineData("test1")]
//         [InlineData("anothername")]
//         public async Task GetWithStringParameter(string @param)
//         {
//             // Arrange
//             using var server = _fixture.CreateServer(services =>
//             {
//                 services.AddSingleton<IDbThing, DbThing>();
//                 services.AddTransient<Pipeline<ModelRequest, ModelResponse>>(sp =>
//                     new PipelineInstructions<MyModelPipeline, ModelRequest, ModelResponse>()
//                         .WithStage<GetModelFromDatabase>()
//                         .GetPipeline(sp));
//             },
//             app => app.UseEndpoints(endpoints =>
//             {
//                 endpoints.MapGet("/test/{id}", async ctx => await endpoints.ServiceProvider.GetRequiredService<Pipeline<ModelRequest, ModelResponse>>().Run(ctx));
//             }));
//             var client = server.CreateClient();

//             // Act
//             var response = await client.GetAsync("/test/" + @param);

//             // Assert
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//             var content = await response.Content.ReadAsStringAsync();
//             Assert.Equal("0=" + @param, content);
//         }

//         public class TwoIdsModelRequest
//         {
//             public string Id { get; set; }
//             public string Id2 { get; set; }
//         }

//         public class TwoIdsPipeline : StagedPipeline<TwoIdsModelRequest, ModelResponse>
//         {
//             public TwoIdsPipeline(PipelineStage<TwoIdsModelRequest, ModelResponse> stages)
//                 : base(stages)
//             {
//             }

//             protected override TwoIdsModelRequest ParseModel(HttpContext context)
//             {
//                 return new TwoIdsModelRequest
//                 {
//                     Id = context.Request.RouteValues["id"]?.ToString(),
//                     Id2 = context.Request.RouteValues["id2"]?.ToString(),
//                 };
//             }

//             protected override async Task ParseResponse(HttpContext context, ModelResponse response)
//             {
//                 context.Response.StatusCode = (int)HttpStatusCode.OK;
//                 await context.Response.WriteAsync(response.Name);
//             }
//         }

//         public class TestPipelineStage : PipelineStage<TwoIdsModelRequest, ModelResponse>
//         {
//             public override Task<ModelResponse> RunAsync(TwoIdsModelRequest input, CancellationToken stoppingToken)
//             {
//                 var response = new ModelResponse
//                 {
//                     Name = $"Id = {input.Id}, Id2 = {input.Id2}",
//                 };

//                 return Task.FromResult(response);
//             }
//         }

//         [Fact]
//         public async Task GetWithMultipleStringParameters()
//         {
//             // Arrange
//             using var server = _fixture.CreateServer(services =>
//             {
//                 services.AddSingleton<IDbThing, DbThing>();
//                 services.AddPipelines()
//                     .RegisterPipeline<TwoIdsPipeline, TwoIdsModelRequest, ModelResponse>(
//                         b => b.WithStage<TestPipelineStage>());
//             },
//             app => app.UseEndpoints(endpoints =>
//             {
//                 var pipelineRegistry = endpoints.ServiceProvider.GetRequiredService<PipelineRegistry>();
//                 endpoints.MapGet("/test/{id}/{id2}", pipelineRegistry.Get<TwoIdsPipeline, TwoIdsModelRequest, ModelResponse>());
//             }));
//             var client = server.CreateClient();

//             // Act
//             var response = await client.GetAsync("/test/one/two");

//             // Assert
//             Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//             var content = await response.Content.ReadAsStringAsync();
//             Assert.Equal("Id = one, Id2 = two", content);
//         }
//     }
// }
