using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.rabit_mq
{
    public interface IRabitMqService : IDisposable
    {
        void Publish(IModel _channel, string message, string? routingKey = null, string? exchange = null);
        IModel CreateChannel();
        IConnection GetConnection();
    }
}
