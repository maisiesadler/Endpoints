using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Pipelines
{
    public abstract class Pipeline<TIn, TOut>
    {
        protected abstract TIn ParseModel(HttpContext context);
        protected abstract Task ParseResponse(HttpContext context, TOut response);

        private readonly PipelineStage<TIn, TOut> _stages;

        public Pipeline(PipelineStage<TIn, TOut> stages)
        {
            _stages = stages;
        }

        protected virtual Task<TIn> ParseModelAsync(HttpContext context)
        {
            return Task.FromResult(ParseModel(context));
        }

        public async Task Run(HttpContext context)
        {
            var input = await ParseModelAsync(context);
            var response = await GetResponse(input);

            await ParseResponse(context, response);
        }

        protected virtual async Task<TOut> GetResponse(TIn input)
        {
            return await _stages.RunAsync(input, CancellationToken.None);
        }
    }
}
