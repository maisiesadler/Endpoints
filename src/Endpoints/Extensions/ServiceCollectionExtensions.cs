using System;
using System.Net;
using System.Threading.Tasks;
using Endpoints.Instructions;
using Endpoints.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoints.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPipelines(this IServiceCollection services)
        {
            services.AddSingleton<PipelineRegistry>();

            return services;
        }

        public static IServiceCollection RegisterRetrievePipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, Task<TIn>> parseModel,
           Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse)
        {
            var instructions = new RetrievePipelineInstructions<TIn, TOut>(
                parseModel, parseResponse
            );

            services.AddSingleton(instructions);

            return services;
        }

        public static IServiceCollection RegisterRetrievePipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, Task<TIn>> parseModel,
           Func<HttpContext, TOut, Task> parseResponse)
        {
            var instructions = new RetrievePipelineInstructions<TIn, TOut>(
                parseModel, WrapParseResponse(parseResponse)
            );

            services.AddSingleton(instructions);

            return services;
        }

        public static IServiceCollection RegisterRetrievePipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, TIn> parseModel,
           Func<HttpContext, TOut, Task> parseResponse)
        {
            var instructions = new RetrievePipelineInstructions<TIn, TOut>(
                parseModel, WrapParseResponse(parseResponse)
            );

            services.AddSingleton(instructions);

            return services;
        }

        // todo: default error wrapper?
        private static Func<HttpContext, PipelineResponse<TOut>, Task> WrapParseResponse<TOut>(Func<HttpContext, TOut, Task> func)
        {
            return async (ctx, response) =>
            {
                if (response.Success)
                {
                    await func(ctx, response.Result);
                }
                else
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            };
        }

        public static IServiceCollection RegisterRetrievePipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, TIn> parseModel,
           Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse,
           Action<RetrievePipelineInstructions<TIn, TOut>> builder = null)
        {
            var instructions = new RetrievePipelineInstructions<TIn, TOut>(
                parseModel, parseResponse
            );

            if (builder != null)
                builder(instructions);

            services.AddSingleton(instructions);

            return services;
        }
    }
}
