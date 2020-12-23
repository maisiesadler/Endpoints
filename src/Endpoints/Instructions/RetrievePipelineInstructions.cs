using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Instructions
{
    public class RetrievePipelineInstructions<TIn, TOut>
    {
        public Func<HttpContext, Task<TIn>> ParseModel { get; }
        public Func<HttpContext, TOut, Task> ParseResponse { get; }
        public List<Type> Middleware { get; } = new List<Type>();

        public RetrievePipelineInstructions(
            Func<HttpContext, Task<TIn>> parseModel,
            Func<HttpContext, TOut, Task> parseResponse)
            => (ParseModel, ParseResponse) = (parseModel, parseResponse);

        public RetrievePipelineInstructions(
            Func<HttpContext, TIn> parseModel,
            Func<HttpContext, TOut, Task> parseResponse)
            => (ParseModel, ParseResponse) = (WrapParseModel(parseModel), parseResponse);

        private Func<HttpContext, Task<TIn>> WrapParseModel(Func<HttpContext, TIn> parseModel)
        {
            return ctx => Task.FromResult(parseModel(ctx));
        }

        public bool Validate()
        {
            return ParseModel != null
                && ParseResponse != null;
        }

        public RetrievePipelineInstructions<TIn, TOut> WithMiddleware<TMiddleware>()
            where TMiddleware : Middleware<TOut>
        {
            Middleware.Add(typeof(TMiddleware));
            return this;
        }
    }
}
