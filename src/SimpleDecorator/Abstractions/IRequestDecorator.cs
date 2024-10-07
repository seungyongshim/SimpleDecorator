namespace SimpleDecorator.Abstractions;

public interface IRequestDecorator<in TRequest, TResponse> 
{
    Task<TResponse> DecorateAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
