using infrastructure.rabit_mq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class ChatConsumerService : BackgroundService
{
    private readonly IRabitMqService _mq;          // service tạo connection/channel của bạn
    private readonly IRabbitTopology _topology;
    private readonly IOptionsMonitor<TopologyOption> _options; // đọc theo tên
    private IModel? _channel;

    public ChatConsumerService(
        IRabitMqService mq,
        IRabbitTopology topology,
        IOptionsMonitor<TopologyOption> options)
    {
        _mq = mq;
        _topology = topology;
        _options = options;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opt = _options.Get("chat_mqtt");             // <-- chọn bộ cấu hình theo tên
        _channel = _mq.CreateChannel();

        _topology.EnsureTopology(_channel, opt);
        _channel.BasicQos(0, opt.Prefetch, false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (s, ea) =>
        {
            var body = ea.Body.ToArray();
            var msg = Encoding.UTF8.GetString(body);
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
