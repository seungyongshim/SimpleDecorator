using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SimpleDecorator.Abstractions;

namespace SimpleDecorator;

file delegate ValueTask<TResponse> HandleAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken);
file delegate ValueTask<TResponse> PipelineAsync<TRequest, TResponse>(TRequest request, Func<ValueTask<TResponse>> next, CancellationToken cancellationToken);

public static partial class HostExtensionMethods
{
    public static SimpleHandlerBuilder<TRequest, TResponse> UseSimpleHandler<TRequest, TResponse>
    (
        this IHostApplicationBuilder hostBuilder,
        string key,
        Func<IServiceProvider, TRequest, CancellationToken, ValueTask<TResponse>> handlerFactory,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    )
    {
        hostBuilder.Services.Replace
        (
            new ServiceDescriptor
            (
                typeof(HandleAsync<TRequest, TResponse>),
                key,
                (sp, key) => new HandleAsync<TRequest, TResponse>(async (req, ct) => await handlerFactory.Invoke(sp, req, ct).ConfigureAwait(false)),
                serviceLifetime
            )
        );

        hostBuilder.Services.Replace
        (
            new ServiceDescriptor
            (
                typeof(IHandler<TRequest, TResponse>),
                key,
                typeof(RequestHandlerWrapper<TRequest, TResponse>),
                serviceLifetime
            )
        );

        return new SimpleHandlerBuilder<TRequest, TResponse>
        {
            Key = key,
            Services = hostBuilder.Services
        };
    }

    public static SimpleHandlerBuilder<TRequest, TResponse> AddPipeline<TRequest, TResponse>
    (
        this SimpleHandlerBuilder<TRequest, TResponse> builder,
        Func<IServiceProvider, TRequest, Func<ValueTask<TResponse>>, CancellationToken, ValueTask<TResponse>> pipelineFactory,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
    )
    {
        builder.Services.Add
        (
            new ServiceDescriptor
            (
                typeof(PipelineAsync<TRequest, TResponse>),
                builder.Key,
                (sp, key) => new PipelineAsync<TRequest, TResponse>(async (req, next, ct) => await pipelineFactory.Invoke(sp, req, next, ct).ConfigureAwait(false)),
                serviceLifetime
            )
        );
        return builder;
    }
}

file class RequestHandlerWrapper<TRequest, TResponse>
(
    [ServiceKey] string key,
    IServiceProvider sp    
) : IHandler<TRequest, TResponse>
{
    public ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var handler = sp.GetRequiredKeyedService<HandleAsync<TRequest, TResponse>>(key);
        var decorators = sp.GetKeyedServices<PipelineAsync<TRequest, TResponse>>(key) ?? [];
        var decorated = decorators.Aggregate
        (
            () => handler.Invoke(request, cancellationToken),
            (next, decorator) => () => decorator.Invoke(request, next, cancellationToken)
        );

        return decorated();
    }
}
