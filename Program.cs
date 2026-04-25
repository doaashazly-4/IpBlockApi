using System.Net.Http.Headers;
using IpBlockApi.Background;
using IpBlockApi.Middleware;
using IpBlockApi.Options;
using IpBlockApi.Repositories;
using IpBlockApi.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<GeoLocationOptions>(builder.Configuration.GetSection(GeoLocationOptions.SectionName));

builder.Services.AddSingleton<IBlockedCountryRepository, BlockedCountryRepository>();
builder.Services.AddSingleton<ITemporalBlockRepository, TemporalBlockRepository>();
builder.Services.AddSingleton<IBlockedAttemptLogRepository, BlockedAttemptLogRepository>();

builder.Services.AddSingleton<IIpClientInfoProvider, IpClientInfoProvider>();
builder.Services.AddSingleton<ICountryBlockPolicy, CountryBlockPolicy>();

builder.Services.AddHttpClient("GeoLocation", client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<IGeoLocationService, GeoLocationService>();

builder.Services.AddHostedService<TemporalBlockCleanupBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "IP Block & Geo API", Version = "v1" });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
