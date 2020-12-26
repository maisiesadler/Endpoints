using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Endpoints.Extensions;
using Endpoints.Pipelines;
using Example.Api.Domain;
using Microsoft.AspNetCore.Http;

namespace Example.Api.Adapter
{
    public static class ModelParser
    {
        public static async Task<CreateUserRequest> CreateUserRequestFromBody(HttpContext context)
        {
            var body = await context.ParseBody();
            var userRequest = JsonSerializer.Deserialize<CreateUserRequest>(body);

            return userRequest;
        }

        public static async Task SetJsonResponse<T>(HttpContext context, T o)
        {
            await context.Response.WriteAsJsonAsync(o);
        }

        public static GetUserRequest GetUserRequestFromPath(HttpContext context)
        {
            var id = context.Request.RouteValues["id"].ToString();

            return new GetUserRequest(id);
        }

        public static async Task SetGetUserResponse(HttpContext context, PipelineResponse<GetUserResponse> response)
        {
            if (response.Success)
            {
                await context.Response.WriteAsJsonAsync(response.Result);
            }
            else
            {
                if (response.Error.ErrorMessage == GetUserInteractor.ErrorsNoUser)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync(response.Error.ErrorMessage);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync(response.Error.ErrorMessage);
                }
            }
        }
    }
}
