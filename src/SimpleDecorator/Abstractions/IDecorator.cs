namespace SimpleDecorator.Abstractions;

public interface IDecorator<in TRequest, TResponse> 
{
    Task<TResponse> DecorateAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken);
}
