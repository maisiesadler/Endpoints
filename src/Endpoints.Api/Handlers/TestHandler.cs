using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Endpoints.Api.Handlers
{
    [HandlerAttribute("/test")]
    public class TestHandler
    {
        [Get("/")]
        public StringHandlerResponse Get()
        {
            return "Hello!";
        }

        [Get("/{something}")]
        public StringHandlerResponse Get(string something)
        {
            return "Hello! " + something;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HandlerAttribute : Attribute
    {
        public HandlerAttribute(string endpoint)
        {
            Endpoint = endpoint;
        }

        public string Endpoint { get; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class MethodAttribute : Attribute
    {
        public MethodAttribute(string endpoint)
        {
            Endpoint = endpoint;
        }

        public string Endpoint { get; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class GetAttribute : MethodAttribute
    {
        public GetAttribute(string endpoint) : base(endpoint)
        {
        }
    }

    public interface IHandlerResponse
    {
        string Response();
    }

    public class StringHandlerResponse : IHandlerResponse
    {
        public string S { get; }

        public StringHandlerResponse(string s)
        {
            S = s;
        }

        public static implicit operator StringHandlerResponse(string s) => new StringHandlerResponse(s);

        public string Response() => S;
    }
}
