using SDK;

namespace API.Idempotency;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdempotency(
        this IServiceCollection services,
        Action<HttpContextIdempotencyKeyProviderOptions>? optionsFactory = null)
    {
        var implementationInstance = new HttpContextIdempotencyKeyProviderOptions();
        optionsFactory?.Invoke(implementationInstance);
        services.AddSingleton(implementationInstance);
        services.AddTransient<IIdempotencyKeyProvider, HttpContextIdempotencyKeyProvider>();
        return services;
    }
}