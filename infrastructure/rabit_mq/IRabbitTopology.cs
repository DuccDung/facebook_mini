using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.rabit_mq
{
    public interface IRabbitTopology
    {
        void EnsureTopology(IModel channel, TopologyOption options);
    }

}
