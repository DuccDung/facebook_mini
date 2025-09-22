using infrastructure.rabit_mq;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Options;
using profile_service.Internal;
using profile_service.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace profile_service.service
{
    public class ProfileConsumerService : BackgroundService
    {
        private readonly IRabitMqService _mq;
        private readonly IRabbitTopology _topology;
        private readonly IOptionsMonitor<TopologyOption> _options;
        private IModel? _ch;
        private readonly IServiceScopeFactory _scopeFactory;   
        public ProfileConsumerService(IRabitMqService mq, IRabbitTopology topology, IOptionsMonitor<TopologyOption> options, IServiceScopeFactory serviceScopeFactory)
        {
            _mq = mq;
            _topology = topology;
            _options = options;
            _scopeFactory = serviceScopeFactory;    
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ch = _mq.CreateChannel();
            _topology.EnsureTopology(_ch, _options.Get("setup_profile"));
            _ch.BasicQos(0, _options.Get("setup_profile").Prefetch, false);
            var consumer = new EventingBasicConsumer(_ch);
            consumer.Received += async (s, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var opts = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    using var doc = JsonDocument.Parse(json);
                    var envElem = doc.RootElement.GetProperty("env");
                    var env = envElem.Deserialize<Envelope>(opts);

                    using var scope = _scopeFactory.CreateScope(); // <-- tạo scope cho mỗi message
                    var profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();

                    var profile = new Profile
                    {
                        UserId = env.user_id,
                        FullName = env.Req.Firstname + " " + env.Req.Lastname,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow

                    };
                    await profileService.CreateProfile(profile);
                    _ch.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Mail Service] Lỗi xử lý: {ex.Message}");
                    _ch.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };
            _ch.BasicConsume(queue: _options.Get("setup_profile").Queue, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
        public override void Dispose()
        {
            _ch?.Dispose();
            base.Dispose();
        }
    }
}
