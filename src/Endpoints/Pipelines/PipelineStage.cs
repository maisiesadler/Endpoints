using System.Threading;
using System.Threading.Tasks;

namespace Endpoints.Pipelines
{
    public abstract class PipelineStage<TContext, TOut>
    {
        protected readonly PipelineStage<TContext, TOut> _next;

        public PipelineStage(PipelineStage<TContext, TOut> next = null)
        {
            _next = next;
        }

        public abstract Task<TOut> RunAsync(TContext context, CancellationToken stoppingToken);
    }
}
