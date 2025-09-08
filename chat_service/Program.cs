using chat_service.Models;
using infrastructure;
using infrastructure.rabit_mq;
using RabbitMQ.Client;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSqlServer<TextingServicesContext>(builder.Configuration.GetConnectionString("SqlServer"));

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

// Đăng ký topology service
builder.Services.AddRabbitTopology();

// Cấu hình NHIỀU bộ options bằng tên:
builder.Services.Configure<TopologyOption>("chat_mqtt", o =>
{
    o.Exchange = "amq.topic";
    o.ExchangeType = RabbitMQ.Client.ExchangeType.Topic;
    o.Queue = "chat.user-chat.q";
    o.RoutingKey = "chat.room.#";
    o.Dlx = "chat.dlx";
    o.Dlq = "chat.user-chat.dlq";
    o.Prefetch = 10;
    // ví dụ thêm TTL 10 phút
    o.QueueArgs = new Dictionary<string, object> { ["x-message-ttl"] = 600_000 };
});

builder.Services.AddHostedService<ChatConsumerService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
