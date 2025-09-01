using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.rabit_mq
{
    public sealed class RabbitOptions
    {
        // config connection rabbitmq
        public string HostName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public int Port { get; set; } 

        // config topology
        public string Exchange { get; set; } = "";
        public string ExchangeType { get; set; } = "";
        public string Queue { get; set; } = "";
        public string RoutingKey { get; set; } = "";

        public string Dlx { get; set; } = "app.dlx";
        public string Dlq { get; set; } = "app.dlq";
        
        // anthor config
        public ushort Prefetch { get; set; } = 16;
        public int PublisherConfirmsTimeoutSeconds { get; set; } = 5;
    }
}

