namespace SimpleDecorator.Abstractions;

public interface IDecorator<in TRequest, TResponse> 
{
    ValueTask<TResponse> DecorateAsync(TRequest request, Func<ValueTask<TResponse>> next, CancellationToken cancellationToken);
}
