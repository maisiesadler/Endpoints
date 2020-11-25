using System;

namespace Endpoints.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class GetAttribute : MethodAttribute
    {
        public GetAttribute(string endpoint) : base(endpoint)
        {
        }
    }
}
