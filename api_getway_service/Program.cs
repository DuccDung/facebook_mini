using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Đọc file ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);


// Đăng ký Ocelot
builder.Services.AddOcelot();
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFront", p => p
         .WithOrigins("http://127.0.0.1:8000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()  // chỉ bật nếu bạn cần gửi cookie/token theo dạng credentials: 'include'
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
await app.UseOcelot();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
