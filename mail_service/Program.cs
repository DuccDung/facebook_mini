using infrastructure;
using infrastructure.rabit_mq;
using mail_service.Internal;
using mail_service.service;
using RabbitMQ.Client;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Đăng ký Rabbit core
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


// Đăng ký topology cho queue mà mail_service SỞ HỮU
builder.Services.AddRabbitTopology(new TopologyOption
{
    Exchange = "mail.exchange",
    ExchangeType = ExchangeType.Direct,
    Queue = "mail.user-registered.q", 
    RoutingKey = "user-registered",
    Dlx = "mail.dlx",
    Dlq = "mail.user-registered.dlq",
    Prefetch = 16
});

// Đăng ký BackgroundService consumer
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<ITemplateRenderer, SimpleTemplateRenderer>();
builder.Services.AddHostedService<MailConsumerService>();


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
