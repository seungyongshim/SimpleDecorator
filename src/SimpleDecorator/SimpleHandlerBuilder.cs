using Microsoft.Extensions.DependencyInjection;

namespace SimpleDecorator;

public record SimpleHandlerBuilder<TRequest, TResponse>
{
    public required string Key { get; init; }

    public required IServiceCollection Services { get; init; }
}
