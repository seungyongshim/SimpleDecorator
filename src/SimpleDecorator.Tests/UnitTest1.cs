using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using SimpleDecorator.Abstractions;

namespace SimpleDecorator.Tests;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var sp = new ServiceCollection()
            .AddKeyedHandler<Request, Response, RequestHandler>("1")
            .AddKeyedDecorator<Request, Response, KeyDecorator>("1")
            .AddKeyedDecorator<Request, Response, WhatDecorator>("1")
            .BuildServiceProvider();

        var handler = sp.GetRequiredKeyedService<IRequestHandler<Request, Response>>("1");
        var ret = await handler.HandleAsync(new Request(), CancellationToken.None);

        Assert.Equal("1", ret.Message);
    }
}


file class RequestHandler : IRequestHandler<Request, Response>
{
    public Task<Response> HandleAsync(Request request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Response
        {
            Message = "Hello World!"
        });
    }
}

file class WhatDecorator([ServiceKey] string key) : IRequestDecorator<Request, Response>
{
    public async Task<Response> DecorateAsync(Request request, RequestHandlerDelegate<Response> next, CancellationToken cancellationToken)
    {
        var response = await next();

        return new Response
        {
            Message = "What"
        };
    }
}

file class KeyDecorator([ServiceKey] string key) : IRequestDecorator<Request, Response>
{
    public async Task<Response> DecorateAsync(Request request, RequestHandlerDelegate<Response> next, CancellationToken cancellationToken)
    {
        var response = await next();
        return new Response
        {
            Message = key
        };
    }
}

file record Request { }
file record Response
{
    public required string Message { get; init; }
}
