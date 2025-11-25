using chat_service.Models;
using chat_service.Models.ModelBase;
using chat_service.service;
using infrastructure.rabit_mq;
using MediaProto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;
public class ChatConsumerService : BackgroundService
{
    private readonly IRabitMqService _mq;          // service tạo connection/channel của bạn
    private readonly IRabbitTopology _topology;
    private readonly IOptionsMonitor<TopologyOption> _options; // đọc theo tên
    private IModel? _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MediaGrpcService.MediaGrpcServiceClient _mediaGrpc;
    private readonly HttpClient _http;
    public ChatConsumerService(
        IRabitMqService mq,
        IRabbitTopology topology,
        IOptionsMonitor<TopologyOption> options,
        IServiceScopeFactory scopeFactory,
         MediaGrpcService.MediaGrpcServiceClient mediaGrpc)
    {
        _mq = mq;
        _topology = topology;
        _options = options;
        _scopeFactory = scopeFactory;
        _mediaGrpc = mediaGrpc;
        _http = new HttpClient
        {
            //  BaseAddress = new Uri("https://localhost:7070/")
            BaseAddress = new Uri("http://profile_service:8084/")
        };
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
            _ = Task.Run(async () =>
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
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                    throw;
                }
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            });
            _ = Task.Run(async () =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var msg = System.Text.Json.JsonSerializer.Deserialize<ChatMessageDto>(json);

                try
                {
                    if (msg == null) throw new Exception("Message is null");
                    var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<TextingServicesContext>();
                    var message = new Message // 2
                    {
                        MessageId = Guid.NewGuid(),
                        SenderId = int.Parse(msg.SenderId ?? throw new Exception("SenderId is null")),
                        ConversationId = msg.ThreadId,
                        Content = msg.Text ?? throw new Exception("Text is null"),
                        CreatedAt = DateTime.UtcNow
                    };

                    var receiverId = await db.ConversationMembers
                        .Where(cm => cm.ConversationId == msg.ThreadId && cm.UserId != message.SenderId)
                        .Select(cm => cm.UserId)
                        .ToListAsync(); // 1

                    var media = _mediaGrpc.GetByAssetIdGrpc(new GetByAssetIdRequest { AssetId = msg.ThreadId.ToString() });
                    if (media == null || media.Items == null || media.Items.Count == 0) throw new Exception("Media response is null or empty");
                    var url = $"api/Profiles/get-profile?userId={message.SenderId}";
                    var profile = await _http.GetFromJsonAsync<ProfileRes>(url);

                    mes_notification notification = new mes_notification
                    {
                        receiver_ids = receiverId,
                        sender = new user
                        {
                            userId = message.SenderId,
                            username = profile != null ? profile.FullName : "Unknown",
                        },
                        avatar_url = media.Items[0].MediaUrl,
                        content = message.Content.Length > 50 ? message.Content.Substring(0, 50) + "..." : message.Content,
                        created_at = message.CreatedAt,
                        type = "message",
                    };
                    using var ch_notification = _mq.CreateChannel();
                    var topo_notification = _options.Get("chat_notification");
                    var json_notification = System.Text.Json.JsonSerializer.Serialize(notification);
                    _mq.Bind(ch_notification, topo_notification);
                    _mq.Publish(ch_notification, topo_notification, json_notification);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(">>> RabbitMQ Consumer Exception: " + ex.Message);
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                    throw;
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
