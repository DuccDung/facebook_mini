using infrastructure.rabit_mq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure
{
    public static class DependencyInjectionRabbitCore
    {
        /// Đăng ký: RabbitOptions + IRabitMqService (1 connection/svc).
        public static IServiceCollection AddRabbitCore(
            this IServiceCollection services,
            Action<RabbitOptions> configureOptions)
        {
            var options = new RabbitOptions();
            configureOptions(options);
            services.AddSingleton(options);

            services.AddSingleton<IRabitMqService, RabitMqService>(); // KHÔNG đụng topology
            return services;
        }
    }
}
