# Endpoints

[![NuGet version (Endpoints)](https://img.shields.io/nuget/v/Endpoints.svg)](https://www.nuget.org/packages/Endpoints/)
[![Workflow status](https://github.com/maisiesadler/Endpoints/workflows/Release%20Nuget%20Package/badge.svg)](https://github.com/maisiesadler/Endpoints/actions?query=workflow%3A%22Release+Nuget+Package%22)

Endpoints provides a more friendly way to set up functions for using AspNetCore Endpoint Routing.

## Aims

- Business logic shouldn't know anything about Http Request
- Clear in tests what has been registered and what endpoints are being used
- Few dependencies
- Easy to extend
- Simple

## Endpoint routing example

```
services.AddTransient<IBusinessLogic>();
```

```
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/endpoint", async httpContext => 
    {
        // parse model from httpContext.Request
        var model = httpContext.Request.RouteValues["id"]?.ToString();

        var result = await endpoints.ServiceProvider.GetRequiredService<IBusinessLogic>().Run(model);

        // set response in httpContext.Response
        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        await httpContext.Response.WriteAsync(response.Name)
    });
})
```

## Endpoints

Endpoints lets you define a Pipeline that defines how to parse the model and set the response. The business logic can then be added in separately.

```
services.AddTransient<IBusinessLogic>();
services.AddPipeline<Request, Response>(
    ModelParser.ParseModel,
    ModelParser.ParseResponse
);
```

```
app => app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/test", endpoints.ServiceProvider.Get<IBusinessLogic, Request, Response>(bl => bl.Run));
}));
```

Or if IBusinessLogic implements `IRetriever<Request, Response>`

```
app => app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/test", endpoints.ServiceProvider.Get<IBusinessLogic, Request, Response>());
}));
```
## Getting started

Create new empty web project using `dotnet new web`.

Add Endpoints reference using `dotnet add package Endpoints`
