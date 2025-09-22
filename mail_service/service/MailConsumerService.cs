
using infrastructure.rabit_mq;
using mail_service.Internal;
using MailKit;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        private readonly IOptionsMonitor<TopologyOption> _options;
        private IModel? _ch;
        private IEmailSender _sender;
        private ITemplateRenderer _renderer;
        private readonly ILogger<MailConsumerService> _log;
        public MailConsumerService(IRabitMqService mq, IRabbitTopology topology, IOptionsMonitor<TopologyOption> options, IEmailSender sender, ITemplateRenderer renderer, ILogger<MailConsumerService> log)
        {
            _mq = mq;
            _topology = topology;
            _options = options;
            _sender = sender;
            _renderer = renderer;
            _log = log;
        }

        //protected override Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _ch = _mq.CreateChannel();
        //    _topology.EnsureTopology(_ch, _options.Get("mail_user_registered"));
        //    _ch.BasicQos(0, _options.Get("mail_user_registered").Prefetch, false);

        //    var consumer = new EventingBasicConsumer(_ch);
        //    consumer.Received += async (s, ea) =>
        //    {
        //        try
        //        {
        //            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
        //            var msg = JsonSerializer.Deserialize<UserRegisteredEvent>(body);

        //            Console.WriteLine($"[Mail Service] Gửi email xác nhận cho {msg?.email} lúc {msg?.at}");

        //            if(msg != null) // send mail
        //            {
        //                var html = _renderer.RenderSignUpConfirm(msg.email, "null", msg.at);
        //                await _sender.SendAsync(msg.email, "Xác nhận email đăng ký", html, stoppingToken);
        //            }

        //            _ch.BasicAck(ea.DeliveryTag, false);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"[Mail Service] Lỗi xử lý: {ex.Message}");
        //            _ch.BasicNack(ea.DeliveryTag, false, requeue: false);
        //        }
        //    };

        //    // Bắt đầu consume
        //    _ch.BasicConsume(queue: _options.Get("mail_user_registered").Queue, autoAck: false, consumer: consumer);

        //    return Task.CompletedTask;
        //}
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1 connection từ _mq, mở nhiều channel
            var ch1 = _mq.CreateChannel();
            var ch2 = _mq.CreateChannel();

            var topo1 = _options.Get("mail_user_registered");
            var topo2 = _options.Get("user-active-success");

            _topology.EnsureTopology(ch1, topo1);
            _topology.EnsureTopology(ch2, topo2);

            ch1.BasicQos(0, topo1.Prefetch, false);
            ch2.BasicQos(0, topo2.Prefetch, false);

            // Dùng AsyncEventingBasicConsumer để await được
            var c1 = new AsyncEventingBasicConsumer(ch1);
            c1.Received += async (_, ea) =>
            {
                try
                {
                    var msg = JsonSerializer.Deserialize<UserRegisteredEvent>(
                        Encoding.UTF8.GetString(ea.Body.ToArray()),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (msg != null) // send mail
                    {
                        var html = _renderer.RenderSignUpConfirm(msg.email, "null", msg.at);
                        await _sender.SendAsync(msg.email, "Xác nhận email đăng ký", html, stoppingToken);
                    }

                    ch1.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    ch1.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
                await Task.CompletedTask;
            };

            var c2 = new AsyncEventingBasicConsumer(ch2);
            c2.Received += async (_, ea) =>
            {
                try
                {
                    var env = JsonSerializer.Deserialize<Envelope>(
                        Encoding.UTF8.GetString(ea.Body.ToArray()),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // xử lý...

                    //if (env != null) // send mail
                    //{
                    //    var html = _renderer.RenderSignUpConfirm(msg.email, "null", msg.at);
                    //    await _sender.SendAsync(msg.email, "Xác nhận email đăng ký", html, stoppingToken);
                    //}

                    ch2.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception)
                {
                    ch2.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
                await Task.CompletedTask;
            };

            ch1.BasicConsume(queue: topo1.Queue, autoAck: false, consumer: c1);
            ch2.BasicConsume(queue: topo2.Queue, autoAck: false, consumer: c2);

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
