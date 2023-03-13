using App.Metrics;
using GeocodingService.Middleware;
using GeocodingService.Services;
using GeocodingService.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var metrics = AppMetrics.CreateDefaultBuilder()
            .Build();

builder.Services.AddMetrics(metrics);
builder.Services.AddMetricsEndpoints();

// Add services to the container.
builder.Services.AddScoped<IGeocodeService, GeocodeService>();

builder.Services.Configure<CacheStoreSettings>(
    builder.Configuration.GetSection("CachingStoreDatabase"));

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMetricsAllEndpoints();

app.UseMiddleware<CustomExceptionHandlerMiddleware>();

app.MapControllers();

app.Run();
