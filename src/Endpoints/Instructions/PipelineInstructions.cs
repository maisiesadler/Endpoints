using Microsoft.Extensions.DependencyInjection;
using Endpoints.Pipelines;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace Endpoints.Instructions
{
    public interface IPipelineBuilder<TPipeline, TIn, TOut> where TPipeline : Pipeline<TIn, TOut>

    {
        PipelineInstructions<TPipeline, TIn, TOut> WithStage<TPipelineStage>()
              where TPipelineStage : PipelineStage<TIn, TOut>;
    }

    public class PipelineInstructions<TPipeline, TIn, TOut> : IPipelineBuilder<TPipeline, TIn, TOut>
        where TPipeline : Pipeline<TIn, TOut>
    {
        private readonly Stack<Type> _stages;
        private bool _built = false;

        private bool _stagesAllowed = false;

        public PipelineInstructions()
        {
            var pipelineCtors = typeof(TPipeline).GetConstructors(BindingFlags.Instance | BindingFlags.Public).ToList();
            if (pipelineCtors.Count != 1)
                throw new InvalidOperationException("Must have one public constructor");

            var ctorParams = pipelineCtors[0].GetParameters().ToList();
            if (ctorParams.Count == 0)
            {
                _stagesAllowed = false;
            }
            else if (ctorParams.Count == 1)
            {
                _stagesAllowed = true;
                _stages = new Stack<Type>();
            }
            else
                throw new InvalidOperationException("Public constructor have one public parameter either no parameters or one with type PipelineStage");
        }

        public PipelineInstructions<TPipeline, TIn, TOut> WithStage<TPipelineStage>()
           where TPipelineStage : PipelineStage<TIn, TOut>
        {
            if (!_stagesAllowed)
            {
                throw new InvalidOperationException("Cannot add stages when there is no constructor for stages");
            }

            var type = typeof(TPipelineStage);

            _stages.Push(type);

            return this;
        }

        public Pipeline<TIn, TOut> GetPipeline(IServiceProvider serviceProvider)
        {
            var (pipeline, ok) = TryGetPipeline(serviceProvider);
            if (!ok)
                throw new InvalidOperationException("Could not create pipeline");
            return pipeline;
        }

        public (Pipeline<TIn, TOut>, bool) TryGetPipeline(IServiceProvider serviceProvider)
        {
            if (_stagesAllowed)
            {
                return TryGetStagedPipeline(serviceProvider);
            }

            var pipeline = (TPipeline)Activator.CreateInstance(typeof(TPipeline));
            return (pipeline, true);
        }

        private (Pipeline<TIn, TOut>, bool) TryGetStagedPipeline(IServiceProvider serviceProvider)
        {
            var (stages, ok) = BuildStages(serviceProvider);
            if (!ok)
            {
                return (null, false);
            }

            var pipeline = (TPipeline)Activator.CreateInstance(typeof(TPipeline), new[] { stages });

            return (pipeline, ok);
        }

        public (PipelineStage<TIn, TOut>, bool) BuildStages(IServiceProvider serviceProvider)
        {
            if (_built) return (null, false);
            _built = true;

            PipelineStage<TIn, TOut> stage = null;

            while (_stages.TryPop(out var type))
            {
                if (stage == null)
                {
                    stage = CreateInnerPipelineStage(type, serviceProvider);
                }
                else
                {
                    stage = (PipelineStage<TIn, TOut>)Activator.CreateInstance(type, stage);
                }
            }

            return (stage, stage != null);
        }

        private PipelineStage<TIn, TOut> CreateInnerPipelineStage(Type type, IServiceProvider serviceProvider)
        {
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                var ctorParams = GetConstructorParameters(type, serviceProvider);
                return (PipelineStage<TIn, TOut>)Activator.CreateInstance(type, ctorParams);
            }
            else
            {
                return (PipelineStage<TIn, TOut>)Activator.CreateInstance(type);
            }
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
