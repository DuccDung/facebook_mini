using authorization_service.Models;
using Microsoft.EntityFrameworkCore;
using infrastructure;
using authorization_service.Internal;
using authorization_service.service;
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

// add scode generation service
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
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
