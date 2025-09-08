using System.Collections.Generic;

namespace infrastructure.rabit_mq
{
    public class TopologyOption
    {
        // Exchange/Queue chính
        public string Exchange { get; set; } = "";
        public string ExchangeType { get; set; } = "";   // vd: ExchangeType.Topic
        public string Queue { get; set; } = "";
        public string RoutingKey { get; set; } = "";

        // Dead-letter
        public string Dlx { get; set; } = "app.dlx";
        public string DlxType { get; set; } = RabbitMQ.Client.ExchangeType.Direct; // NEW
        public string Dlq { get; set; } = "app.dlq";

        // QoS cho consumer
        public ushort Prefetch { get; set; } = 1;

        // Tham số mở rộng cho Queue (TTL, max length,...)
        public IDictionary<string, object>? QueueArgs { get; set; } // NEW
    }
}
