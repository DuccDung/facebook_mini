using authorization_service.Internal;
using authorization_service.Models;
using authorization_service.service;
using infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AuthorizationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

Console.WriteLine(">>> SQL Connection String: " + builder.Configuration.GetConnectionString("SqlServer"));
    
builder.Services.AddInfrastructure(
    builder.Configuration["Redis:ConnectionString"] ?? "",
    builder.Configuration["Redis:ServicePrefix"] ?? ""
);
builder.WebHost.ConfigureKestrel(o =>
{
    // AUTHZ
   // o.ListenAnyIP(8081, lo => lo.Protocols = HttpProtocols.Http2);
    // Nếu service này cũng có REST controller, dùng:
     o.ListenAnyIP(8081, lo => lo.Protocols = HttpProtocols.Http1AndHttp2);

    // MEDIA (nếu đây là project media_service)
    // o.ListenAnyIP(8086, lo => lo.Protocols = HttpProtocols.Http2);
});
// add scode generation service
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddGrpc();
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
app.MapGrpcService<AuthorizationGrpcService>();
app.Run();
