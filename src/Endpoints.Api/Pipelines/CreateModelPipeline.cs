using System.Net;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Api.Pipelines
{
    public class CreateModelPipeline : Pipeline<ModelRequest, PipelineResponse<CreateModelPipeline.Response>>
    {
        protected override Task<PipelineResponse<CreateModelPipeline.Response>> GetResponse(ModelRequest input)
        {
            // var response = PipelineResponse<CreateModelPipeline.Response, string>.Fail("error-response");
            var response = PipelineResponse.Ok<CreateModelPipeline.Response>(new CreateModelPipeline.Response("new-model-id"));
            return Task.FromResult(response);
        }

        protected override ModelRequest ParseModel(HttpContext context)
        {
            return new ModelRequest
            {
                Id = context.Request.RouteValues["id"]?.ToString(),
            };
        }

        protected override Task ParseResponse(HttpContext context, PipelineResponse<CreateModelPipeline.Response> response)
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
