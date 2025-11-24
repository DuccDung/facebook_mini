using notification_service.Models;
using RabbitMQ.Client;
using infrastructure.rabit_mq;
using infrastructure;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// DB + Redis + RabbitMQ
builder.Services.AddSqlServer<NotificationDbContext>(
    builder.Configuration.GetConnectionString("SqlServer"));

// add rabbitmq service
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
// Đăng ký theo tên
builder.Services.Configure<TopologyOption>("user-registered", o =>
{
    o.Exchange = "mail.exchange";
    o.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
    o.Queue = "mail.user-registered.q";
    o.RoutingKey = "mail.user-registered";
    o.Dlx = "mail.dlx";
    o.Dlq = "mail.user-registered.dlq";
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFront", p => p
        .WithOrigins("http://api_getway_service:8085") // Chỉ định nguồn cụ thể
        .AllowAnyHeader()
        .AllowAnyMethod()
    // .AllowCredentials() // Bật AllowCredentials nếu cần gửi cookies/token
    );
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("DevFront");
app.Map("/ws" , async httpContext =>
{
    if (httpContext.WebSockets.IsWebSocketRequest) { 
        httpContext.Response.StatusCode = 400;
        return;
    }

    var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
    //await WebSocketHandler.HandleWebSocketAsync(webSocket);
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
