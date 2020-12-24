using System.Net;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Api.Pipelines
{
    public static class ModelParser
    {
        public static ModelRequest ParseModel(HttpContext context)
        {
            return new ModelRequest
            {
                Id = context.Request.RouteValues["id"]?.ToString(),
            };
        }

        public static Task ParseCreateModelResponse(HttpContext context, PipelineResponse<CreateModelRetriever.Response> response)
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

        public static async Task ParseResponse(HttpContext context, PipelineResponse<ModelResponse> response)
        {
            if (response.Success)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsync(response.Result.Name);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
