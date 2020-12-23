using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoints.Instructions
{
    public class RetrievePipelineInstructions<TIn, TOut>
    {
        public Func<HttpContext, Task<TIn>> ParseModel { get; }
        public Func<HttpContext, TOut, Task> ParseResponse { get; }
        public List<Func<IServiceProvider, IMiddleware<TOut>>> MiddlewareFunction { get; } = new List<Func<IServiceProvider, IMiddleware<TOut>>>();

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
            where TMiddleware : IMiddleware<TOut>
        {
            MiddlewareFunction.Add(sp => sp.GetRequiredService<TMiddleware>());
            return this;
        }

        public RetrievePipelineInstructions<TIn, TOut> WithMiddleware(IMiddleware<TOut> middleware)
        {
            MiddlewareFunction.Add(_ => middleware);
            return this;
        }
    }
}
