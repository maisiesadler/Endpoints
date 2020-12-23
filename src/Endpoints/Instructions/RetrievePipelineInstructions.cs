using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Endpoints.Instructions
{
    public class RetrievePipelineInstructions<TIn, TOut>
    {
        public Func<HttpContext, Task<TIn>> ParseModel { get; private set; }
        public Func<HttpContext, TOut, Task> ParseResponse { get; private set; }
        public Type Retriever { get; private set; }

        public RetrievePipelineInstructions<TIn, TOut> GetModelFrom(Func<HttpContext, Task<TIn>> modelFrom)
        {
            ParseModel = modelFrom;
            return this;
        }

        public RetrievePipelineInstructions<TIn, TOut> GetModelFrom(Func<HttpContext, TIn> modelFrom)
        {
            ParseModel = ctx => Task.FromResult(modelFrom(ctx));
            return this;
        }

        public RetrievePipelineInstructions<TIn, TOut> SetResponse(Func<HttpContext, TOut, Task> response)
        {
            ParseResponse = response;
            return this;
        }

        public RetrievePipelineInstructions<TIn, TOut> Retrieve<T>()
            where T : IRetriever<TIn, TOut>
        {
            Retriever = typeof(T);
            return this;
        }

        public bool Validate()
        {
            return true;
        }

        public (RetrievePipeline<TIn, TOut>, bool) TryGetPipeline(IServiceProvider sp)
        {
            if (!Validate())
            {
                return (null, false);
            }

            var ctorParams = GetConstructorParameters(Retriever, sp);
            var retriever = (IRetriever<TIn, TOut>)Activator.CreateInstance(Retriever, ctorParams);
            var pipeline = new RetrievePipeline<TIn, TOut>(ParseModel, ParseResponse, retriever);

            return (pipeline, true);
        }

        private object[] GetConstructorParameters(Type type, IServiceProvider serviceProvider)
        {
            var publicCtor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                                .OrderBy(x => x.GetParameters().Count())
                                .FirstOrDefault();

            if (publicCtor == null)
            {
                throw new InvalidOperationException("No public constructor for " + type.Name);
            }

            var ctorParams = new List<object>();
            foreach (var p in publicCtor.GetParameters())
            {
                ctorParams.Add(serviceProvider.GetRequiredService(p.ParameterType));
            }
            return ctorParams.ToArray();
        }
    }
}
