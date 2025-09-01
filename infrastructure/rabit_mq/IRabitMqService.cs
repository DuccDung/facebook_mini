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
        void Publish(IModel _channel,TopologyOption topology, string message, string? routingKey = null, string? exchange = null);
        IModel CreateChannel();
        IConnection GetConnection();
        void Bind(IModel _channel, TopologyOption topology);
    }
}
