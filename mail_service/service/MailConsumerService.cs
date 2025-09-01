
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using infrastructure.rabit_mq;
namespace mail_service.service
{
    public class MailConsumerService : BackgroundService
    {
        private readonly IRabitMqService _mq;
        private readonly IRabbitTopology _topology;
        private readonly TopologyOption _opt;
        private IModel? _ch;

        public MailConsumerService(IRabitMqService mq, IRabbitTopology topology, TopologyOption opt)
        {
            _mq = mq;
            _topology = topology;
            _opt = opt;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Tạo channel riêng cho consumer
            _ch = _mq.CreateChannel();

            // Consumer sở hữu queue → khai báo topology cho chắc chắn
            _topology.EnsureTopology(_ch, _opt);

            // Prefetch = số message chưa xử lý được giữ trong consumer
            _ch.BasicQos(0, _opt.Prefetch, false);

            var consumer = new EventingBasicConsumer(_ch);
            consumer.Received += (s, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var msg = JsonSerializer.Deserialize<UserRegisteredEvent>(body);

                    // TODO: xử lý nghiệp vụ — gửi email xác nhận
                    Console.WriteLine($"[Mail Service] Gửi email xác nhận cho {msg?.Email} lúc {msg?.At}");

                    // Thông báo đã xử lý xong
                    _ch.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Mail Service] Lỗi xử lý: {ex.Message}");

                    // Lỗi business → đẩy vào DLQ, không requeue
                    _ch.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };

            // Bắt đầu consume
            _ch.BasicConsume(queue: _opt.Queue, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _ch?.Dispose();
            base.Dispose();
        }
    }

    // Model sự kiện
    public record UserRegisteredEvent(string Email, DateTime At);

}
