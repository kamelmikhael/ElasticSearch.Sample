using Infrastructure.Database;
using Logging.Common;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureDatabase();

// Configure Serilog
builder.ConfigureSerilog("Web.Api")
    .ConfigureOpenTelemetry("Web.Api");
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

// Correlation middleware (see below)
app.UseMiddleware<CorrelationIdMiddleware>();

app.MapControllers();

app.Run();
