using Xunit;
using Endpoints.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Endpoints.Api.Pipelines;
using Endpoints.Pipelines;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace Endpoints.Test
{
    public class PipelineBuilderTests
    {
        [Fact]
        public void CanCreatePipeline()
        {
            // Arrange
            var pi = new PipelineInstructions<ModelRequest, ModelResponse, MyModelPipeline>();
            pi.Register<TimingPipelineStage>()
              .Register<GetModelFromDatabase>();

            var services = new ServiceCollection();
            services.AddTransient<IDbThing, DbThing>();
            var sp = services.BuildServiceProvider();

            // Act
            var (stages, ok) = pi.BuildStages(sp);

            // Assert
            Assert.True(ok);
            Assert.NotNull(stages);
        }
    }

    public class PipelineProvider
    {

    }

    public class PipelineInstructions<TIn, TOut, TPipeline>
        where TPipeline : Pipeline<TIn, TOut>
    {
        private static readonly Type[] _pipelineStageConstructor = new[] { typeof(PipelineStage<TIn, TOut>) };

        private readonly Stack<Type> _stages = new Stack<Type>();
        private readonly Dictionary<Type, PipelineStage<TIn, TOut>> _stageToNext = new Dictionary<Type, PipelineStage<TIn, TOut>>();

        private bool _built = false;

        public PipelineInstructions()
        {
        }

        public PipelineInstructions<TIn, TOut, TPipeline> Register<TPipelineStage>(PipelineStage<TIn, TOut> next = null)
           where TPipelineStage : PipelineStage<TIn, TOut>
        {
            var type = typeof(TPipelineStage);

            // if (_stages.Count > 0)
            // {
            //     var constructor = type.GetConstructor(_pipelineStageConstructor);

            //     if (constructor == null)
            //     {
            //         throw new InvalidOperationException("No valid next pipeline stage for this stage: " + type.Name);
            //     }
            // }

            _stages.Push(type);
            _stageToNext.Add(type, next);

            return this;
        }

        public (PipelineStage<TIn, TOut>, bool) BuildStages(IServiceProvider serviceProvider)
        {
            if (_built) return (null, false);
            _built = true;

            PipelineStage<TIn, TOut> stage = null;

            while (_stages.TryPop(out var type))
            {
                if (_stageToNext.TryGetValue(type, out var nextStage))
                {
                    if (stage == null)
                    {
                        if (type.GetConstructor(Type.EmptyTypes) == null)
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

                            stage = (PipelineStage<TIn, TOut>)Activator.CreateInstance(type, ctorParams.ToArray());
                        }
                        else
                        {
                            stage = (PipelineStage<TIn, TOut>)Activator.CreateInstance(type);
                        }
                    }
                    else
                    {
                        stage = (PipelineStage<TIn, TOut>)Activator.CreateInstance(type, stage);
                    }
                }
                else
                {
                    return (null, false);
                }
            }

            return (stage, stage != null);
        }
    }
}
