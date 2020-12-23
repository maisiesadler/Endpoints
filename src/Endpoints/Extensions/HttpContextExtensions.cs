using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Endpoints.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task<string> ParseBody(this HttpContext context)
        {
            string body;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                body = await reader.ReadToEndAsync();
            }

            return body;
        }
    }
}