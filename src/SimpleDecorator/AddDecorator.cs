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
    public static IServiceCollection AddKeyedDecorator<TRequest, TResponse, TDecorator>(this IServiceCollection services, string key, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        where TDecorator : IRequestDecorator<TRequest, TResponse>
    {
        services.Add(new ServiceDescriptor(typeof(IRequestDecorator<TRequest, TResponse>), key, typeof(TDecorator), serviceLifetime));
        return services;
    }

    public static IServiceCollection AddKeyedHandler<TRequest, TResponse, THandler>(this IServiceCollection services, string key, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        where THandler : IRequestHandler<TRequest, TResponse>
    {
        services.Add(new ServiceDescriptor(typeof(IRequestHandler<TRequest, TResponse>), key, (sp, key) => new RequestHandlerWrapper<TRequest, TResponse>(key, sp, ActivatorUtilities.CreateInstance<THandler>(sp)), serviceLifetime));
        return services;
    }

    public static IServiceCollection AddKeyedHandler<TRequest, TResponse, THandler>(this IServiceCollection services, string key, Func<IServiceProvider, THandler> factory, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        where THandler : IRequestHandler<TRequest, TResponse>
    {
        services.Add(new ServiceDescriptor(typeof(IRequestHandler<TRequest, TResponse>), key, (sp, key) => new RequestHandlerWrapper<TRequest, TResponse>(key, sp, factory(sp)), serviceLifetime));
        return services;
    }
}

file class RequestHandlerWrapper<TRequest, TResponse>
(
    [ServiceKey] object? key,
    IServiceProvider sp,
    IRequestHandler<TRequest, TResponse> handler
) : IRequestHandler<TRequest, TResponse>
{
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        Task<TResponse> handler1() => handler.HandleAsync(request, cancellationToken);

        var decorators = sp.GetKeyedServices<IRequestDecorator<TRequest, TResponse>>(key) ?? [];
        var decorated = decorators.Aggregate
        (
            (RequestHandlerDelegate<TResponse>)handler1,
            (next, decorator) => () => decorator.DecorateAsync(request, next, cancellationToken)
        );

        return decorated();
    }
}
