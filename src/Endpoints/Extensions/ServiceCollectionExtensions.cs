using System;
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

        public static IServiceCollection RegisterPipeline<TPipeline, TIn, TOut>(
            this IServiceCollection services,
            Action<IPipelineBuilder<TPipeline, TIn, TOut>> builder = null)
            where TPipeline : Pipeline<TIn, TOut>
        {
            var instructions = new PipelineInstructions<TPipeline, TIn, TOut>();
            if (builder != null)
                builder(instructions);

            services.AddSingleton(instructions);

            return services;
        }

        public static IServiceCollection RegisterRetrievePipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, Task<TIn>> parseModel,
           Func<HttpContext, TOut, Task> parseResponse)
        {
            var instructions = new RetrievePipelineInstructions<TIn, TOut>(
                parseModel, parseResponse
            );

            services.AddSingleton(instructions);

            return services;
        }

        public static IServiceCollection RegisterRetrievePipeline<TIn, TOut>(
           this IServiceCollection services,
           Func<HttpContext, TIn> parseModel,
           Func<HttpContext, TOut, Task> parseResponse,
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

    public class PipelineRegistry
    {
        private readonly IServiceProvider _serviceProvider;

        public PipelineRegistry(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public RequestDelegate Get<TPipeline, TIn, TOut>()
            where TPipeline : Pipeline<TIn, TOut>
        {
            var instructions = _serviceProvider.GetRequiredService<PipelineInstructions<TPipeline, TIn, TOut>>();
            var pipeline = instructions.GetPipeline(_serviceProvider);

            return pipeline.Run;
        }

        public RequestDelegate GetRetrieve<TRetriever, TIn, TOut>()
            where TRetriever : IRetriever<TIn, TOut>
        {
            var instructions = _serviceProvider.GetRequiredService<RetrievePipelineInstructions<TIn, TOut>>();
            var (pipeline, ok) = instructions.TryGetPipeline<TRetriever, TIn, TOut>(_serviceProvider);
            if (!ok)
                throw new Exception("Could not create pipeline");

            return pipeline.Run;
        }

        public RequestDelegate GetRetrieve<TIn, TOut>(Func<IServiceProvider, Func<TIn, Task<TOut>>> retrieverFn)
        {
            var instructions = _serviceProvider.GetRequiredService<RetrievePipelineInstructions<TIn, TOut>>();
            var retriever = retrieverFn(_serviceProvider);
            var (pipeline, ok) = instructions.TryGetPipeline<TIn, TOut>(retriever, _serviceProvider);
            if (!ok)
                throw new Exception("Could not create pipeline");

            return pipeline.Run;
        }
    }
}
