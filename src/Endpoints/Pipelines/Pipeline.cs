using System;
using System.Threading;
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

    public abstract class Pipeline<TIn, TOut, TError> : Pipeline<TIn, PipelineResponse<TOut, TError>>
    {
        protected abstract Task<TError> ParseErrorResponse(TIn input, Exception exception);
        protected abstract Task<TOut> TryGetResponse(TIn input);
        protected async override Task<PipelineResponse<TOut, TError>> GetResponse(TIn input)
        {
            try
            {
                var response = await TryGetResponse(input);
                return PipelineResponse<TOut, TError>.Ok(response);
            }
            catch (Exception ex)
            {
                var error = await ParseErrorResponse(input, ex);
                return PipelineResponse<TOut, TError>.Fail(error);
            }
        }
    }

    public class PipelineResponse<TResult, TError>
    {
        public bool Success { get; }
        public TResult Result { get; }
        public TError Error { get; }

        private PipelineResponse(bool success, TResult result, TError error)
        {
            Success = success;
            Result = result;
            Error = error;
        }

        public static PipelineResponse<TResult, TError> Ok(TResult result)
        {
            return new PipelineResponse<TResult, TError>(true, result, default);
        }

        public static PipelineResponse<TResult, TError> Fail(TError error)
        {
            return new PipelineResponse<TResult, TError>(false, default, error);
        }
    }
}
