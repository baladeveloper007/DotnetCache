using Microsoft.Extensions.Configuration;
using System.Configuration;


//private readonly IConfiguration configuration;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration.GetSection("Redis:ConnectionString");

// Redis Configuration.
builder.Services.AddStackExchangeRedisCache(options =>
 {
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "cache-LYBC8QTK";
 });

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// Add the custom caching middleware
app.UseMiddleware<CustomCacheMiddleware>();

app.MapControllers();

app.Run();
