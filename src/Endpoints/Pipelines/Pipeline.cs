using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Pipelines
{
    public class Pipeline<TIn, TOut>
    {
        private readonly Func<HttpContext, Task<TIn>> _parseModel;
        private readonly Func<HttpContext, PipelineResponse<TOut>, Task> _parseResponse;
        private readonly Middleware<TOut> _middleware;

        public Pipeline(
            Func<HttpContext, Task<TIn>> parseModel,
            Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse,
            Middleware<TOut> middleware)
        {
            _parseModel = parseModel;
            _parseResponse = parseResponse;
            _middleware = middleware;
        }

        public async Task Run(IRetriever<TIn, TOut> retriever, HttpContext context)
        {
            var input = await _parseModel(context);

            var response = await _middleware.Run(() => retriever.Retrieve(input));

            await _parseResponse(context, response);
        }
    }
}
