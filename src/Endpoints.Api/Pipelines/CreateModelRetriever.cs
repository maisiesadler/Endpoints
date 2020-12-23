using System.Net;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Api.Pipelines
{
    public class CreateModelRetriever : IRetriever<ModelRequest, PipelineResponse<CreateModelRetriever.Response>>
    {
        public Task<PipelineResponse<CreateModelRetriever.Response>> Retrieve(ModelRequest input)
        {
            // var response = PipelineResponse<CreateModelPipeline.Response, string>.Fail("error-response");
            var response = PipelineResponse.Ok<CreateModelRetriever.Response>(new CreateModelRetriever.Response("new-model-id"));
            return Task.FromResult(response);
        }

        public static ModelRequest ParseModel(HttpContext context)
        {
            return new ModelRequest
            {
                Id = context.Request.RouteValues["id"]?.ToString(),
            };
        }

        public static Task ParseResponse(HttpContext context, PipelineResponse<CreateModelRetriever.Response> response)
        {
            if (response.Success)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.WriteAsync(response.Result.Id);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.WriteAsync(response.Error.ErrorMessage);
            }

            return Task.CompletedTask;
        }

        public record Response
        {
            public string Id { get; }

            public Response(string id) => (Id) = (id);
        }
    }
}
