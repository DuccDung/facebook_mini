using authentication_service.Internal;
using authentication_service.Models;
using authentication_service.service;
using infrastructure;
using infrastructure.rabit_mq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey ?? ""))
    };
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AuthenticationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
builder.Services.AddInfrastructure(
    builder.Configuration["Redis:ConnectionString"] ?? "",
    builder.Configuration["Redis:ServicePrefix"] ?? ""
);
Console.WriteLine(">>> SQL Connection String: " + builder.Configuration.GetConnectionString("SqlServer"));

// add scode generation service
builder.Services.AddScoped<IAuthentication, AuthticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
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

builder.Services.Configure<TopologyOption>("resetpwd", o =>
{
    o.Exchange = "mail.exchange";
    o.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
    o.Queue = "mail.resetpwd.q";
    o.RoutingKey = "mail.resetpwd";
    o.Dlx = "mail.dlx";
    o.Dlq = "mail.resetpwd.dlq";
});

builder.Services.Configure<TopologyOption>("setup_profile", o =>
{
    o.Exchange = "profile.exchange";
    o.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
    o.Queue = "profile.setup_profile.q";
    o.RoutingKey = "profile.setup_profile";
    o.Dlx = "profile.dlx";
    o.Dlq = "profile.setup_profile.dlq";
});
//user-active-success
builder.Services.Configure<TopologyOption>("user-active-success", o =>
{
    o.Exchange = "mail.exchange";
    o.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
    o.Queue = "mail.user-active-success.q";
    o.RoutingKey = "mail.user-active-success";
    o.Dlx = "user-active-success.dlx";
    o.Dlq = "user-active-success.dlq";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
