using AuthorizationProto;
using chat_service.Internal;
using chat_service.Models;
using chat_service.service;
using infrastructure;
using infrastructure.rabit_mq;
using MediaProto;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Cho phép gRPC HTTP/2 qua HTTP (plaintext) trong Docker
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// docker dev system

// gRPC tới các service nội bộ (HTTP, không TLS)
builder.Services.AddGrpcClient<AuthorizationGrpcService.AuthorizationGrpcServiceClient>(o =>
{
    o.Address = new Uri("http://authorization_service:8081");
});
builder.Services.AddGrpcClient<MediaGrpcService.MediaGrpcServiceClient>(o =>
{
    o.Address = new Uri("http://media_service:9086");
});

// gRPC host system
//builder.Services.AddGrpcClient<AuthorizationGrpcService.AuthorizationGrpcServiceClient>(o =>
//{
//    o.Address = new Uri("http://authorization_service:8081");
//});
//builder.Services.AddGrpcClient<MediaGrpcService.MediaGrpcServiceClient>(o =>
//{
//    o.Address = new Uri("https://localhost:7121");
//});

// DB + Redis + RabbitMQ
builder.Services.AddSqlServer<TextingServicesContext>(
    builder.Configuration.GetConnectionString("SqlServer"));

var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";
var rabbitPort = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var p) ? p : 5672;



builder.Services.AddRabbitCore(opt =>
{
    opt.HostName = rabbitHost;
    opt.UserName = rabbitUser;
    opt.Password = rabbitPass;
    opt.Port = rabbitPort;
});
builder.Services.AddInfrastructure(
    builder.Configuration["Redis:ConnectionString"] ?? "",
    builder.Configuration["Redis:ServicePrefix"] ?? ""
);

// Topology RabbitMQ (ví dụ)
builder.Services.AddRabbitTopology();
builder.Services.Configure<TopologyOption>("chat_mqtt", o =>
{
    o.Exchange = "amq.topic";
    o.ExchangeType = RabbitMQ.Client.ExchangeType.Topic;
    o.Queue = "chat.user-chat.q";
    o.RoutingKey = "chat.room.#";
    o.Dlx = "chat.dlx";
    o.Dlq = "chat.user-chat.dlq";
    o.Prefetch = 10;
    o.QueueArgs = new Dictionary<string, object> { ["x-message-ttl"] = 600_000 };
});
// chat_notification
builder.Services.Configure<TopologyOption>("chat_notification", o =>
{
    o.Exchange = "notification.exchange";
    o.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
    o.Queue = "notification.chat_message.q";
    o.RoutingKey = "notification.chat_message";
    o.Dlx = "notification.dlx";
    o.Dlq = "notification.chat_message.dlq";
});
// DI app
builder.Services.AddHostedService<ChatConsumerService>();
builder.Services.AddScoped<IAuthorization, Authorization>();
builder.Services.AddScoped<IConversation, chat_service.service.Conversation>();
var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ❌ Không redirect HTTPS trong Docker dev (chạy HTTP thuần)
//// app.UseHttpsRedirection();

app.UseRouting();
// app.UseAuthentication(); // nếu có
app.UseAuthorization();

// Health check tối giản
app.MapGet("/health", () => Results.Ok(new { ok = true }));

// Controllers
app.MapControllers();

app.Run();
