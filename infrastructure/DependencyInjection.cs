using infrastructure.caching;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string redisConnectionString,
            string servicePrefix)
        {
            // Đăng ký Redis connection
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnectionString));

            // Đăng ký RedisService
            services.AddScoped<ICacheService>(sp =>
            {
                var redis = sp.GetRequiredService<IConnectionMultiplexer>();
                return new RedisService(redis, servicePrefix);
            });

            return services;
        }
    }
}
