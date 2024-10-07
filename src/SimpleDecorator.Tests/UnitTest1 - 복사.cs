using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleDecorator.Abstractions;

namespace SimpleDecorator.Tests;

public class UnitTest2
{
    [Fact]
    public async Task TwoStacked()
    {
        var host = Host.CreateApplicationBuilder();

        host.UseSimpleHandler<Request, Response>("1", async (sp, req, ct) => new Response
        {
            Message = "Hello World!",
        }).AddPipeline(async (sp, req, next, ct) =>
        {
            var ret = await next().ConfigureAwait(false);
            return ret with { Message = "What" };
        });

        await using var scope = host.Build().Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        var handler = sp.GetRequiredKeyedService<IHandler<Request, Response>>("1");
        var ret = await handler.HandleAsync(new Request(), CancellationToken.None);

        Assert.Equal("What", ret.Message);
    }
}

file record Request { }
file record Response
{
    public required string Message { get; init; }
}
