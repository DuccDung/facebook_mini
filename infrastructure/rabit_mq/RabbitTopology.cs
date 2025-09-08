using RabbitMQ.Client;
using System.Collections.Generic;

namespace infrastructure.rabit_mq
{
    internal class RabbitTopology : IRabbitTopology
    {
        public void EnsureTopology(IModel channel, TopologyOption options)
        {
            // 1) Exchange chính
            if (!options.Exchange.Contains("amq"))
            {
                channel.ExchangeDeclare(
                     exchange: options.Exchange,
                     type: options.ExchangeType,
                     durable: true
           );
            }


            // 2) DLX
            channel.ExchangeDeclare(
                exchange: options.Dlx,
                type: options.DlxType,       // dùng type cấu hình
                durable: true
            );

            // 3) DLQ
            channel.QueueDeclare(
                queue: options.Dlq,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
            channel.QueueBind(
                queue: options.Dlq,
                exchange: options.Dlx,
                routingKey: options.RoutingKey
            );

            // 4) Queue chính + arguments
            var args = new Dictionary<string, object>(options.QueueArgs ?? new Dictionary<string, object>());
            args["x-dead-letter-exchange"] = options.Dlx;
            args["x-dead-letter-routing-key"] = options.RoutingKey;

            channel.QueueDeclare(
                queue: options.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,  
                arguments: args
            );

            // 5) Bind queue chính
            channel.QueueBind(
                queue: options.Queue,
                exchange: options.Exchange,
                routingKey: options.RoutingKey
            );
        }
    }
}
