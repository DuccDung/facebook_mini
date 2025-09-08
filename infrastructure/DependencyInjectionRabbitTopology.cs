using infrastructure.rabit_mq;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure
{
    public static class DependencyInjectionRabbitTopology
    {
        // Đăng ký mặc định
        public static IServiceCollection AddRabbitTopology(this IServiceCollection services)
        {
            services.AddSingleton<IRabbitTopology, RabbitTopology>();
            // Không đăng ký TopologyOption ở đây nữa!
            return services;
        }

        // Cho phép thay thế implement nếu cần
        public static IServiceCollection AddRabbitTopology<TTopology>(this IServiceCollection services)
            where TTopology : class, IRabbitTopology
        {
            services.AddSingleton<IRabbitTopology, TTopology>();
            return services;
        }
    }
}
