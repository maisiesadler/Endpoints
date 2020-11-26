using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Endpoints.Attributes;
using Endpoints.Responses;

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

        [Get("/{param1}/{param2}")]
        public StringHandlerResponse Get(string param1, string param2)
        {
            return $"Params are '{param1}' and '{param2}'";
        }
    }
}
