

using ArasTrader.Application;
using ArasTrader.Infrastructure;
using ArasTrader.Infrastructure.Jobs;
using ArasTrader.Infrastructure.Persistence;
using ArasTrader.Web.Api.Extensions;
using ArasTrader.Web.Api.Gateway;
using ArasTrader.Web.Api.Gateway.Abstractions;
using ArasTrader.Web.Api.Gateway.Handlers;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// ── Layers ───────────────────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Gateway Pipeline ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IOrderGatewayHandler, LoggingGatewayHandler>();
builder.Services.AddScoped<IOrderGatewayHandler, ValidationGatewayHandler>();
//todo add RateLimit if 
builder.Services.AddScoped<OrderGatewayPipeline>();

// ── Endpoints ────────────────────────────────────────────────────────────────
builder.Services.AddEndpoints(typeof(Program).Assembly);

// ── API ───────────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ArasTrader Interview", Version = "v1" });
});


var app = builder.Build();

// ── Migrations ────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SuperApp v1"));

app.UseHttpsRedirection();

app.MapEndpoints();


// ── Hangfire ────────────────────────────────────────────────────────────────
app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<WalletUpdateJob>(
    "wallet-update-job",
    job => job.ExecuteAsync(CancellationToken.None), 
    "*/2 * * * *"); //Cron.Minutely);


app.Run();
