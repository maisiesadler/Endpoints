using System.Threading;
using System.Threading.Tasks;

namespace Endpoints.Pipelines
{
    public abstract class PipelineStage<TIn, TOut>
    {
        protected readonly PipelineStage<TIn, TOut> _next;

        public PipelineStage(PipelineStage<TIn, TOut> next = null)
        {
            _next = next;
        }

        public abstract Task<TOut> RunAsync(TIn input, CancellationToken stoppingToken);
    }
}
