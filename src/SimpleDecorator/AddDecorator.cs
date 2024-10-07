using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleDecorator.Abstractions;

namespace SimpleDecorator;

public static class ExtensionMethods
{
    public static IServiceCollection AddKeyedDecorator<TRequest, TResponse, TDecorator>(this IServiceCollection services, string key, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TDecorator : IDecorator<TRequest, TResponse>
    {
        services.Add(new ServiceDescriptor(typeof(IDecorator<TRequest, TResponse>), key, typeof(TDecorator), serviceLifetime));
        return services;
    }

    public static IServiceCollection SetKeyedHandler<TRequest, TResponse, THandler>(this IServiceCollection services, string key, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IHandler<TRequest, TResponse>
    {
        services.Replace(new ServiceDescriptor(typeof(IHandler<TRequest, TResponse>), key, (sp, key) => new RequestHandlerWrapper<TRequest, TResponse>(key, sp, ActivatorUtilities.CreateInstance<THandler>(sp)), serviceLifetime));
        return services;
    }

    public static IServiceCollection SetKeyedHandler<TRequest, TResponse, THandler>(this IServiceCollection services, string key, Func<IServiceProvider, THandler> factory, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where THandler : IHandler<TRequest, TResponse>
    {
        services.Replace(new ServiceDescriptor(typeof(IHandler<TRequest, TResponse>), key, (sp, key) => new RequestHandlerWrapper<TRequest, TResponse>(key, sp, factory(sp)), serviceLifetime));
        return services;
    }
}

file class RequestHandlerWrapper<TRequest, TResponse>
(
    [ServiceKey] object? key,
    IServiceProvider sp,
    IHandler<TRequest, TResponse> handler
) : IHandler<TRequest, TResponse>
{
    public ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var decorators = sp.GetKeyedServices<IDecorator<TRequest, TResponse>>(key) ?? [];
        var decorated = decorators.Aggregate
        (
            () => handler.HandleAsync(request, cancellationToken),
            (next, decorator) => () => decorator.DecorateAsync(request, next, cancellationToken)
        );

        return decorated();
    }
}
