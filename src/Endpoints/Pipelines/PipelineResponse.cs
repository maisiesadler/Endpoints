using System;

namespace Endpoints.Pipelines
{
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
