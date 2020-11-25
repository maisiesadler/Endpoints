using System;
using System.Collections.Generic;
using System.Reflection;
using Endpoints.Attributes;
using Endpoints.Responses;

namespace Endpoints.Extensions
{
    public class DiscoveryExtensions
    {
        public static IEnumerable<(Type, HandlerAttribute)> GetHandlers(Assembly assembly)
            => GetTypesWithAttribute<HandlerAttribute>(assembly);

        private static IEnumerable<(Type, T)> GetTypesWithAttribute<T>(Assembly assembly)
            where T : Attribute
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<T>();
                if (attr != null)
                {
                    yield return (type, attr);
                }
            }
        }

        public static IEnumerable<(MethodInfo, MethodAttribute)> GetMethods(Type type)
            => GetMethodsWithAttribute<MethodAttribute>(type);

        private static IEnumerable<(MethodInfo, T)> GetMethodsWithAttribute<T>(Type type)
            where T : Attribute
        {
            foreach (MethodInfo methodInfo in type.GetMethods())
            {
                var attr = methodInfo.GetCustomAttribute<T>();
                if (attr != null)
                {
                    if (methodInfo.ReturnType.IsAssignableTo(typeof(IHandlerResponse)))
                    {
                        yield return (methodInfo, attr);
                    }
                }
            }
        }
    }
}
