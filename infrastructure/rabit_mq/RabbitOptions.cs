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

     
        
        // anthor config
        public ushort Prefetch { get; set; } = 16;
        public int PublisherConfirmsTimeoutSeconds { get; set; } = 5;
    }
}

