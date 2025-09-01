using infrastructure.rabit_mq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure
{
    public static class DependencyInjectionRabbitTopology
    {
        /// Đăng ký 1 provider topology (mặc định dùng RabbitTopology) + TopologyOption cụ thể.
        public static IServiceCollection AddRabbitTopology(
            this IServiceCollection services,
            TopologyOption topologyOption)
        {
            // Provider topology (nếu bạn có provider khác, map sang class khác)
            services.AddSingleton<IRabbitTopology, RabbitTopology>();

            // TopologyOption cho luồng/queue cụ thể
            services.AddSingleton(topologyOption);
            return services;
        }

        /// Nếu muốn tự chọn provider topology:
        public static IServiceCollection AddRabbitTopology<TTopology>(
            this IServiceCollection services,
            TopologyOption topologyOption)
            where TTopology : class, IRabbitTopology
        {
            services.AddSingleton<IRabbitTopology, TTopology>();
            services.AddSingleton(topologyOption);
            return services;
        }
    }
}
