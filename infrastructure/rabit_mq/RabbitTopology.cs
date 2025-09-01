using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.rabit_mq
{
    internal class RabbitTopology : IRabbitTopology
    {
        public void EnsureTopology(IModel channel, TopologyOption options)
        {
            // Exchange chính
            channel.ExchangeDeclare(options.Exchange, options.ExchangeType, durable: true);

            // DLX
            channel.ExchangeDeclare(options.Dlx, ExchangeType.Direct, durable: true);

            // DLQ
            channel.QueueDeclare(options.Dlq, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(options.Dlq, options.Dlx, routingKey: options.RoutingKey + ".dlq");

            // Queue chính
            channel.QueueDeclare(options.Queue, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    ["x-dead-letter-exchange"] = options.Dlx,
                    ["x-dead-letter-routing-key"] = options.RoutingKey + ".dlq"
                });
            channel.QueueBind(options.Queue, options.Exchange, options.RoutingKey);
        }
    }
}
