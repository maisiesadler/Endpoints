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
    }
}
