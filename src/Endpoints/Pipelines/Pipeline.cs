using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Pipelines
{
    public class Pipeline<TIn, TOut>
    {
        private readonly Func<HttpContext, Task<TIn>> _parseModel;
        private readonly Func<HttpContext, PipelineResponse<TOut>, Task> _parseResponse;
        private readonly IRetriever<TIn, TOut> _retriever;
        private readonly Middleware<TOut> _middleware;

        public Pipeline(
            Func<HttpContext, Task<TIn>> parseModel,
            Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse,
            IRetriever<TIn, TOut> retriever,
            Middleware<TOut> middleware)
        {
            _parseModel = parseModel;
            _parseResponse = parseResponse;
            _retriever = retriever;
            _middleware = middleware;
        }

        public async Task Run(HttpContext context)
        {
            var input = await _parseModel(context);

            var response = await _middleware.Run(() => _retriever.Retrieve(input));

            await _parseResponse(context, response);
        }
    }
}
