using Infrastructure.Database.Data;
using Infrastructure.Database.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDatabase(
        this IServiceCollection services)
    {
        services.AddOptions<ElasticSettings>()
            .BindConfiguration(nameof(ElasticSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        //services.AddSingleton(sp =>
        //    sp.GetRequiredService<IOptions<ElasticSettings>>().Value);

        services.AddScoped<IElasticDbContext, ElasticDbContext>();
        services.AddScoped(typeof(IElasticDbSet<>), typeof(ElasticDbSet<>));

        return services;
    }
}
