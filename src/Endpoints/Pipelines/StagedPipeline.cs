using System.Threading;
using System.Threading.Tasks;

namespace Endpoints.Pipelines
{
    public abstract class StagedPipeline<TIn, TOut> : Pipeline<TIn, TOut>
    {
        private readonly PipelineStage<TIn, TOut> _stages;

        public StagedPipeline(PipelineStage<TIn, TOut> stages)
        {
            _stages = stages;
        }

        protected override async Task<TOut> GetResponse(TIn input)
        {
            return await _stages.RunAsync(input, CancellationToken.None);
        }
    }
}
