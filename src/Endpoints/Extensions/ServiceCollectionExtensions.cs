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
        public static IServiceCollection AddPipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, Task<TIn>> parseModel,
           Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse)
        {
            var instructions = new PipelineInstructions<TIn, TOut>(
                parseModel, parseResponse
            );

            services.AddSingleton(instructions);

            return services;
        }

        public static IServiceCollection AddPipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, Task<TIn>> parseModel,
           Func<HttpContext, TOut, Task> parseResponse)
        {
            var instructions = new PipelineInstructions<TIn, TOut>(
                parseModel, WrapParseResponse(parseResponse)
            );

            services.AddSingleton(instructions);

            return services;
        }

        public static IServiceCollection AddPipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, TIn> parseModel,
           Func<HttpContext, TOut, Task> parseResponse)
        {
            var instructions = new PipelineInstructions<TIn, TOut>(
                parseModel, WrapParseResponse(parseResponse)
            );

            services.AddSingleton(instructions);

            return services;
        }

        public static IServiceCollection AddPipeline<TOut>(
           this IServiceCollection services,
           Func<HttpContext, TOut, Task> parseResponse)
        {
            var instructions = new PipelineInstructions<TOut>(
                WrapParseResponse(parseResponse)
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

        public static IServiceCollection AddPipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, TIn> parseModel,
           Func<HttpContext, PipelineResponse<TOut>, Task> parseResponse,
           Action<PipelineInstructions<TIn, TOut>> builder = null)
        {
            var instructions = new PipelineInstructions<TIn, TOut>(
                parseModel, parseResponse
            );

            if (builder != null)
                builder(instructions);

            services.AddSingleton(instructions);

            return services;
        }
    }
}
