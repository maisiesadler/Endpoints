using System;
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

    public class RunnablePipelineStage<TIn, TOut> : PipelineStage<TIn, TOut>
    {
        private Func<TIn, Task<TOut>> _run;

        public void SetRunFunction(Func<TIn, Task<TOut>> run)
        {
            _run = run;
        }

        public async override Task<TOut> RunAsync(TIn input, CancellationToken stoppingToken)
        {
            if (_run == null) throw new InvalidOperationException("Run function has not been set");

            return await _run(input);
        }
    }
}
