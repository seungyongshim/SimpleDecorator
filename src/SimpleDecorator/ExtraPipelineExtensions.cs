namespace Microsoft.Extensions.DependencyInjection;

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SimpleDecorator;

public static partial class ExtraPipelineExtensions
{
    public static SimpleHandlerBuilder<TRequest, TResponse> AddLoggerPipeline<TRequest, TResponse>
    (
       this SimpleHandlerBuilder<TRequest, TResponse> builder,
       ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
    ) => builder.AddPipeline
    (
        async (sp, request, next, ct) =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(TRequest));

            try
            {
                var response = await next().ConfigureAwait(false);
                logger.LogInformation("Complete to handle {@request} {@response}", request, response);
                return response;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to handle");
                throw;
            }
        },
        serviceLifetime
    );

    public static SimpleHandlerBuilder<TRequest, TResponse> AddTracingPipeline<TRequest, TResponse>
    (
        this SimpleHandlerBuilder<TRequest, TResponse> builder,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
    ) => builder.AddPipeline
    (
        async (sp, request, next, ct) =>
        {
            using var scope = ActivitySourceStatic.StartActivity($"Handle {request?.GetType().Name}");
            return await next().ConfigureAwait(false);
        },
        serviceLifetime
    );

    public static ActivitySource ActivitySourceStatic { get; } = new ActivitySource("Aums.OpenTelemetry");
}
    


