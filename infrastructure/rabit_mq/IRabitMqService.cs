using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.rabit_mq
{
    public interface IRabitMqService
    {
        void SendMessage(string message, string queueName, string exchange = "");
        void ReceiveMessage(string queueName);
        void ConfigureQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);
        IConnection GetConnection(string hostName, string userName, string password);
    }
}
