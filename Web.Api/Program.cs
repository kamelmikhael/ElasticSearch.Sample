using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using Web.Api.ElasticServices;
using Web.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ElasticSettings>(builder.Configuration.GetSection(nameof(ElasticSettings)));

builder.Services.AddScoped<IElasticDbContext, ElasticDbContext>();
builder.Services.AddScoped(typeof(IElasticDbSet<>), typeof(ElasticDbSet<>));

// Configure Serilog
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var assemblyName = Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-");

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", assemblyName)
    .WriteTo.Console()
    .WriteTo.Elasticsearch(
        new ElasticsearchSinkOptions(
            new Uri(builder.Configuration["ElasticSettings:Url"] 
            ?? throw new("ElasticSettings:Url settings not found")))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{assemblyName}-{environment.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy.MM}"
    })
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
