using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using SimpleDecorator.Abstractions;

namespace SimpleDecorator.Tests;

public class UnitTest1
{
    [Fact]
    public async Task TwoStacked()
    {
        var sp = new ServiceCollection()
            .SetKeyedHandler<Request, Response, RequestHandler>("1")
            .AddKeyedDecorator<Request, Response, KeyDecorator>("1")
            .AddKeyedDecorator<Request, Response, WhatDecorator>("1")
            .BuildServiceProvider();

        var handler = sp.GetRequiredKeyedService<IHandler<Request, Response>>("1");
        var ret = await handler.HandleAsync(new Request(), CancellationToken.None);

        Assert.Equal("What", ret.Message);
    }

    [Fact]
    public async Task TwoStackedReversed()
    {
        var sp = new ServiceCollection()
            .SetKeyedHandler<Request, Response, RequestHandler>("1")
            .AddKeyedDecorator<Request, Response, WhatDecorator>("1")
            .AddKeyedDecorator<Request, Response, KeyDecorator>("1")
            .BuildServiceProvider();
        var handler = sp.GetRequiredKeyedService<IHandler<Request, Response>>("1");
        var ret = await handler.HandleAsync(new Request(), CancellationToken.None);
        Assert.Equal("1", ret.Message);
    }

    [Fact]
    public async Task TwoStackedDifferentKeys()
    {
        var sp = new ServiceCollection()
            .SetKeyedHandler<Request, Response, RequestHandler>("1")
            .SetKeyedHandler<Request, Response, RequestHandler>("2")
            .AddKeyedDecorator<Request, Response, KeyDecorator>("1")
            .AddKeyedDecorator<Request, Response, WhatDecorator>("2")
            .BuildServiceProvider();
        var handler1 = sp.GetRequiredKeyedService<IHandler<Request, Response>>("1");
        var handler2 = sp.GetRequiredKeyedService<IHandler<Request, Response>>("2");
        var ret1 = await handler1.HandleAsync(new Request(), CancellationToken.None);
        var ret2 = await handler2.HandleAsync(new Request(), CancellationToken.None);
        Assert.Equal("1", ret1.Message);
        Assert.Equal("What", ret2.Message);
    }

    [Fact]
    public async Task NonStacked()
    {
        var sp = new ServiceCollection()
            .SetKeyedHandler<Request, Response, RequestHandler>("1")
            .BuildServiceProvider();
        var handler = sp.GetRequiredKeyedService<IHandler<Request, Response>>("1");
        var ret = await handler.HandleAsync(new Request(), CancellationToken.None);
        Assert.Equal("Hello World!", ret.Message);
    }

}

file class RequestHandler : IHandler<Request, Response>
{
    public ValueTask<Response> HandleAsync(Request request, CancellationToken cancellationToken) => new(new Response
    {
        Message = "Hello World!",
    });
}

file class WhatDecorator([ServiceKey] string key) : IDecorator<Request, Response>
{
    public async ValueTask<Response> DecorateAsync(Request request, Func<ValueTask<Response>> next, CancellationToken cancellationToken)
    {
        var response = await next();

        return new Response
        {
            Message = "What",
        };
    }

    
}

file class KeyDecorator([ServiceKey] string key) : IDecorator<Request, Response>
{
    public async ValueTask<Response> DecorateAsync(Request request, Func<ValueTask<Response>> next, CancellationToken cancellationToken)
    {
        var response = await next();
        return new Response
        {
            Message = key,
        };
    }
}

file record Request { }
file record Response
{
    public required string Message { get; init; }
}
