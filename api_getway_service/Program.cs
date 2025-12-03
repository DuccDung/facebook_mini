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
// CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("DevFront", p => p
//        .WithOrigins("http://localhost:8000" , "http://127.0.0.1:8000") // Chỉ định nguồn cụ thể
//        .AllowAnyHeader()
//        .AllowAnyMethod()
//    );
//});
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFront", p =>
    {
        p.WithOrigins("http://localhost:8000", "http://127.0.0.1:8000")  // Chỉ định các origin cụ thể
         .AllowAnyHeader()  // Cho phép bất kỳ header nào
         .AllowAnyMethod()  // Cho phép bất kỳ phương thức HTTP nào
         .AllowCredentials();  // Cho phép gửi thông tin xác thực như cookies, headers, token
    });
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
