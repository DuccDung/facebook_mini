using infrastructure;
using infrastructure.rabit_mq;
using profile_service.Internal;
using profile_service.Models;
using profile_service.service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSqlServer<ProfileContext>(builder.Configuration.GetConnectionString("SqlServer"));


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
builder.Services.AddRabbitTopology(); // Đăng ký topology service

builder.Services.Configure<TopologyOption>("setup_profile", o =>
{
    o.Exchange = "profile.exchange";
    o.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
    o.Queue = "profile.setup_profile.q";
    o.RoutingKey = "profile.setup_profile";
    o.Dlx = "profile.dlx";
    o.Dlq = "profile.setup_profile.dlq";
});
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddHostedService<ProfileConsumerService>();
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
