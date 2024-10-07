namespace SimpleDecorator.Abstractions;

public interface IHandler<in TRequest, TResponse> 
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}


