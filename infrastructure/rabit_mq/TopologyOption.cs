using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.rabit_mq
{
    public class TopologyOption
    {
        // config topology
        public string Exchange { get; set; } = "";
        public string ExchangeType { get; set; } = "";
        public string Queue { get; set; } = "";
        public string RoutingKey { get; set; } = "";

        public string Dlx { get; set; } = "app.dlx";
        public string Dlq { get; set; } = "app.dlq";
        public ushort Prefetch { get; set; } = 1;
    }
}
