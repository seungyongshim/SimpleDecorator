namespace SimpleDecorator.Abstractions;

public interface IHandler<in TRequest, TResponse> 
{
    ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}


