
using infrastructure.rabit_mq;
using mail_service.Internal;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
namespace mail_service.service
{
    public class MailConsumerService : BackgroundService
    {
        private readonly IRabitMqService _mq;
        private readonly IRabbitTopology _topology;
        private readonly TopologyOption _opt;
        private IModel? _ch;
        private IEmailSender _sender;
        private ITemplateRenderer _renderer;
        private readonly ILogger<MailConsumerService> _log;
        public MailConsumerService(IRabitMqService mq, IRabbitTopology topology, TopologyOption opt, IEmailSender sender, ITemplateRenderer renderer, ILogger<MailConsumerService> log)
        {
            _mq = mq;
            _topology = topology;
            _opt = opt;
            _sender = sender;
            _renderer = renderer;
            _log = log;
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
            consumer.Received += async (s, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var msg = JsonSerializer.Deserialize<UserRegisteredEvent>(body);

                    Console.WriteLine($"[Mail Service] Gửi email xác nhận cho {msg?.email} lúc {msg?.at}");

                    if(msg != null) // send mail
                    {
                        var html = _renderer.RenderSignUpConfirm(msg.email, "null", msg.at);
                        await _sender.SendAsync(msg.email, "Xác nhận email đăng ký", html, stoppingToken);
                    }

                    _ch.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Mail Service] Lỗi xử lý: {ex.Message}");
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

    public record UserRegisteredEvent(string email, DateTime at);

}
