using System.Net;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Api.Pipelines
{
    public class MyModelRetriever : IRetriever<ModelRequest, ModelResponse>
    {
        private readonly IDbThing _dbThing;

        public MyModelRetriever(IDbThing dbThing)
        {
            _dbThing = dbThing;
        }

        public async Task<PipelineResponse<ModelResponse>> Retrieve(ModelRequest input)
        {
            var result = await _dbThing.GetModel(input);
            return PipelineResponse.Ok(result);
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
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
