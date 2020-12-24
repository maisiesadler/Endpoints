using System;
using System.Threading.Tasks;

namespace Endpoints.Pipelines
{
    public interface Middleware<TOut>
    {
        Task<PipelineResponse<TOut>> Run(Func<Task<PipelineResponse<TOut>>> func);
    }

    public class DelegateMiddleware<TOut> : Middleware<TOut>
    {
        public async Task<PipelineResponse<TOut>> Run(Func<Task<PipelineResponse<TOut>>> func) => await func();
    }

    public interface IMiddleware<TOut>
    {
        Task<PipelineResponse<TOut>> Run(Func<Task<PipelineResponse<TOut>>> func);
    }

    public sealed class MiddlewareRunner<TOut> : Middleware<TOut>
    {
        private MiddlewareRunner<TOut> _next;
        private readonly IMiddleware<TOut> _middleware;

        public MiddlewareRunner(IMiddleware<TOut> middleware, MiddlewareRunner<TOut> next)
        {
            _middleware = middleware ?? throw new ArgumentNullException("middleware");
            _next = next;
        }

        public async Task<PipelineResponse<TOut>> Run(Func<Task<PipelineResponse<TOut>>> func)
        {
            return await RunInner(
                async () =>
                {
                    if (_next == null)
                    {
                        return await func().ConfigureAwait(false);
                    }
                    else
                    {
                        return await _next.Run(func);
                    }
                });
        }

        private async Task<PipelineResponse<TOut>> RunInner(Func<Task<PipelineResponse<TOut>>> func)
        {
            return await _middleware.Run(func);
        }
    }
}
