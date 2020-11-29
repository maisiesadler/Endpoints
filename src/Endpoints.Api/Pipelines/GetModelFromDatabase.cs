using System.Threading;
using System.Threading.Tasks;
using Endpoints.Pipelines;

namespace Endpoints.Api.Pipelines
{
    public class GetModelFromDatabase : PipelineStage<ModelRequest, ModelResponse>
    {
        private readonly IDbThing _dbThing;

        public GetModelFromDatabase(IDbThing dbThing)
        {
            _dbThing = dbThing;
        }

        public override async Task<ModelResponse> RunAsync(ModelRequest input, CancellationToken stoppingToken)
        {
            return await _dbThing.GetModel(input);
        }
    }
}
