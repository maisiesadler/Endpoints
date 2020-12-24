using System;
using System.Threading.Tasks;

namespace Endpoints.Pipelines
{
    public interface Middleware<TOut>
    {
        Task<TOut> Run(Func<Task<TOut>> func);
    }

    public class DelegateMiddleware<TOut> : Middleware<TOut>
    {
        public async Task<TOut> Run(Func<Task<TOut>> func) => await func();
    }

    public interface IMiddleware<TOut>
    {
        Task<TOut> Run(Func<Task<TOut>> func);
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

        public async Task<TOut> Run(Func<Task<TOut>> func)
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

        private async Task<TOut> RunInner(Func<Task<TOut>> func)
        {
            return await _middleware.Run(func);
        }
    }
}
