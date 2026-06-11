using KASserver.Soap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace KASserver;

/// <summary>
/// Dependency-injection extensions for registering the KAS API client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IKasClient"/> and its dependencies.
    /// </summary>
    public static IServiceCollection AddKasServer(
        this IServiceCollection services,
        Action<KasServerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // TryAdd* keeps registrations idempotent if AddKasServer is called more than once.
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<KasServerOptions>, KasServerOptionsValidator>());
        services.AddOptions<KasServerOptions>()
            .Configure(configure)
            .ValidateOnStart();

        services.AddHttpClient(KasServerDefaults.HttpClientName);

        // One shared KasSoapTransport singleton holds the session token and flood window; IKasTransport
        // is aliased to that same instance (no second transport) so the seam is behaviour-preserving.
        services.TryAddSingleton<KasSoapTransport>();
        services.TryAddSingleton<IKasTransport>(sp => sp.GetRequiredService<KasSoapTransport>());
        services.TryAddSingleton<IKasClient>(sp =>
            new KasClient(sp.GetRequiredService<IKasTransport>()));

        return services;
    }

    private sealed class KasServerOptionsValidator : IValidateOptions<KasServerOptions>
    {
        public ValidateOptionsResult Validate(string? name, KasServerOptions options)
        {
            // Only validate the default (unnamed) instance AddKasServer configures; leave any
            // named KasServerOptions a consumer registers elsewhere untouched.
            if (name != Options.DefaultName)
                return ValidateOptionsResult.Skip;

            try
            {
                options.Validate();
                return ValidateOptionsResult.Success;
            }
            catch (InvalidOperationException ex)
            {
                return ValidateOptionsResult.Fail(ex.Message);
            }
        }
    }
}
