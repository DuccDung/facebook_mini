using Amazon.Runtime;
using Amazon.S3;
using media_services.Contracts;
using media_services.Interface;
using media_services.Models;
using media_services.Services;       // MediaService
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddDbContext<MediaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
builder.Services.AddScoped<IMediaService, MediaService>();
// Bind options
builder.Services.Configure<B2Options>(builder.Configuration.GetSection("B2"));
var b2 = builder.Configuration.GetSection("B2").Get<B2Options>()!;

// S3 client (Backblaze B2 S3 endpoint)
var keyId = "00564b1e8e6f5a80000000003";
var appKey = "K005DKMKS7n9k4qji6vJduPtK/FzMbk";
if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(appKey))
    throw new InvalidOperationException("Missing B2_KEY_ID/B2_APP_KEY env vars.");

builder.Services.AddSingleton<IAmazonS3>(_ =>
    new AmazonS3Client(
        new BasicAWSCredentials(keyId, appKey),
        new AmazonS3Config { ServiceURL = b2.ServiceURL, ForcePathStyle = b2.ForcePathStyle }
    ));

builder.Services.AddScoped<IObjectStorage, B2S3Storage>();

// Cho phép upload file lớn (tùy nhu cầu)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = b2.MaxUploadBytes; // 500MB mặc định
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGrpcService<MediaGrpcServiceImpl>();
app.Run();
