using infrastructure.rabit_mq;
using Microsoft.Extensions.Options;
using notification_service.Models.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
namespace notification_service.service
{
    public class NotificationConsummerService : BackgroundService
    {
        private readonly IRabitMqService _mq;          // service tạo connection/channel của bạn
        private readonly IRabbitTopology _topology;
        private readonly IOptionsMonitor<TopologyOption> _options; // đọc theo tên
        private IModel? _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        public NotificationConsummerService(IRabitMqService mq,
        IRabbitTopology topology,
        IOptionsMonitor<TopologyOption> options,
        IServiceScopeFactory scopeFactory)
        {
            _mq = mq;
            _topology = topology;
            _options = options;
            _scopeFactory = scopeFactory;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var opt = _options.Get("chat_notification");
            _channel = _mq.CreateChannel();
            _topology.EnsureTopology(_channel, opt);
            _channel.BasicQos(0, opt.Prefetch, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (s, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var msg = System.Text.Json.JsonSerializer.Deserialize<mes_notification>(json);
                if (msg == null) return;
                Console.WriteLine(">>> Notification: " + json);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine(">>> Notification ws: " + json);
                        await WebSocketHandler.SendToUsersAsync(msg);
                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(">>> Error sending notification: " + ex.Message);
                        _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                });
            };
            _channel.BasicConsume(queue: opt.Queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            base.Dispose();
        }
    }
}
