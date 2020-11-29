using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Pipelines
{
    public abstract class Pipeline<TIn, TOut>
    {
        protected abstract TIn ParseModel(HttpContext context);
        protected abstract Task<TOut> GetResponse(TIn input);
        protected abstract Task ParseResponse(HttpContext context, TOut response);

        public async Task Run(HttpContext context)
        {
            var input = ParseModel(context);
            var response = await GetResponse(input);

            await ParseResponse(context, response);
        }
    }
}
