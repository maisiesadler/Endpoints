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

    public abstract class MiddlewareBase<TOut> : Middleware<TOut>
    {
        private MiddlewareBase<TOut> _next;

        public MiddlewareBase(MiddlewareBase<TOut> next)
        {
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

        protected abstract Task<TOut> RunInner(Func<Task<TOut>> func);
    }
}
