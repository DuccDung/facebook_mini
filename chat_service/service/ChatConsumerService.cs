using chat_service.Models;
using chat_service.Models.ModelBase;
using infrastructure.rabit_mq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json.Serialization;
public class ChatConsumerService : BackgroundService
{
    private readonly IRabitMqService _mq;          // service tạo connection/channel của bạn
    private readonly IRabbitTopology _topology;
    private readonly IOptionsMonitor<TopologyOption> _options; // đọc theo tên
    private IModel? _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    public ChatConsumerService(
        IRabitMqService mq,
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
        var opt = _options.Get("chat_mqtt");             // <-- chọn bộ cấu hình theo tên
        _channel = _mq.CreateChannel();
        _topology.EnsureTopology(_channel, opt);
        _channel.BasicQos(0, opt.Prefetch, false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (s, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var msg = System.Text.Json.JsonSerializer.Deserialize<ChatMessageDto>(json);

            Console.WriteLine(">>> Message: " + json);
            try
            {
                var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<TextingServicesContext>();
                if (msg == null) throw new Exception("Message is null");
                var message = new Message
                {
                    MessageId = Guid.NewGuid(),
                    SenderId = int.Parse(msg.SenderId ?? ""),
                    ConversationId = msg.ThreadId,
                    Content = msg?.Text ?? "",
                    CreatedAt = DateTime.UtcNow
                };
                await db.Messages.AddAsync(message);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> RabbitMQ Consumer Exception: " + ex.Message);
                throw;
            }
            // TODO: handle
            _channel.BasicAck(ea.DeliveryTag, multiple: false);
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
