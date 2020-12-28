using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoints.Instructions
{
    public class PipelineInstructions<TIn, TOut>
    {
        public Func<HttpContext, Task<TIn>> ParseModel { get; }
        public Func<HttpContext, PipelineResponse<TOut>, Task> ParseResponse { get; }
        public List<Func<IServiceProvider, IMiddleware<TOut>>> MiddlewareFunction { get; } = new List<Func<IServiceProvider, IMiddleware<TOut>>>();

        public PipelineInstructions(
            Func<HttpContext, Task<TIn>> parseModel,
            Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse)
            => (ParseModel, ParseResponse) = (parseModel, parseResponse);

        public PipelineInstructions(
            Func<HttpContext, TIn> parseModel,
            Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse)
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

        public PipelineInstructions<TIn, TOut> WithMiddleware<TMiddleware>()
            where TMiddleware : IMiddleware<TOut>
        {
            MiddlewareFunction.Add(sp => sp.GetRequiredService<TMiddleware>());
            return this;
        }

        public PipelineInstructions<TIn, TOut> WithMiddleware(IMiddleware<TOut> middleware)
        {
            MiddlewareFunction.Add(_ => middleware);
            return this;
        }
    }

    public class PipelineInstructions<TOut> : PipelineInstructions<NoType, TOut>
    {
        public PipelineInstructions(
            Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse)
            : base(IgnoreNoType, parseResponse) { }

        protected static Task<NoType> IgnoreNoType(HttpContext _) => Task.FromResult(default(NoType));
    }
}
