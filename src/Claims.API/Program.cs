using Claims.Application;
using Claims.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApplicationServices();
await builder.Services.AddInfrastructureAsync(
    builder.Configuration,
    builder.Environment);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Claims API",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Claims API v1"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

await app.Services.MigrateSqlServerDbAsync();

app.Run();

public partial class Program { }
