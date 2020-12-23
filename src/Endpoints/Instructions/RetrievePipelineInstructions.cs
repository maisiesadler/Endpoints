using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Endpoints.Instructions
{
    public class RetrievePipelineInstructions<TIn, TOut>
    {
        public Func<HttpContext, Task<TIn>> ParseModel { get; }
        public Func<HttpContext, TOut, Task> ParseResponse { get; }

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
    }

    public static class RetrievePipelineBuilder
    {
        public static (RetrievePipeline<TIn, TOut>, bool) TryGetPipeline<TRetriever, TIn, TOut>(
            this RetrievePipelineInstructions<TIn, TOut> instructions,
            IServiceProvider sp)
            where TRetriever : IRetriever<TIn, TOut>
        {
            if (!instructions.Validate())
            {
                return (null, false);
            }

            var retriever = sp.GetRequiredService<TRetriever>();
            var pipeline = new RetrievePipeline<TIn, TOut>(instructions.ParseModel, instructions.ParseResponse, retriever);

            return (pipeline, true);
        }
    }
}
