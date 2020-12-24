using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Pipelines
{
    public abstract class Pipeline<TIn, TOut>
    {
        protected abstract TIn ParseModel(HttpContext context);
        protected abstract Task ParseResponse(HttpContext context, TOut response);
        protected abstract Task<TOut> GetResponse(TIn input);

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
    }

    public class RetrievePipeline<TIn, TOut>
    {
        private readonly Func<HttpContext, Task<TIn>> _parseModel;
        private readonly Func<HttpContext, PipelineResponse<TOut>, Task> _parseResponse;
        private readonly IRetriever<TIn, TOut> _retriever;
        private readonly Middleware<TOut> _middleware;

        public RetrievePipeline(
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

    public interface IRetriever<TIn, TOut>
    {
        Task<PipelineResponse<TOut>> Retrieve(TIn input);
    }

    public class PipelineResponse<TResult> : PipelineResponse<TResult, PipelineResponseError>
    {
        internal PipelineResponse(bool success, TResult result, PipelineResponseError error)
            : base(success, result, error)
        {
        }
    }

    public static class PipelineResponse
    {
        public static PipelineResponse<TResult> Ok<TResult>(TResult result)
        {
            return new PipelineResponse<TResult>(true, result, default);
        }

        public static PipelineResponse<TResult, TError> Ok<TResult, TError>(TResult result)
        {
            return new PipelineResponse<TResult, TError>(true, result, default);
        }

        public static PipelineResponse<TResult, TError> Fail<TResult, TError>(TError error)
        {
            return new PipelineResponse<TResult, TError>(false, default, error);
        }

        public static PipelineResponse<TResult> Fail<TResult>(Exception exception, string errorMessage)
        {
            return new PipelineResponse<TResult>(false, default, new PipelineResponseError(exception, errorMessage));
        }
    }

    public class PipelineResponse<TResult, TError>
    {
        public bool Success { get; }
        public TResult Result { get; }
        public TError Error { get; }

        internal PipelineResponse(bool success, TResult result, TError error)
            => (Success, Result, Error) = (success, result, error);
    }

    public record PipelineResponseError
    {
        public Exception Exception { get; }
        public string ErrorMessage { get; }

        internal PipelineResponseError(Exception exception, string errorMessage)
            => (Exception, ErrorMessage) = (exception, errorMessage);
    }
}
