using System.Net.Http.Headers;
using API;
using Implementations.GoogleVertexAI;
using Implementations.OpenAI;
using Microsoft.OpenApi.Models;
using SDK;
using Common;
using Implementations.HappenSoft;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);
var AppTitle = "Vision Intelligence API";
var UpSince = DateTime.UtcNow;
var currentVersion = AssemblyVersion.AsVersion;
builder.Services.AddControllers(options =>
{
    // options.ModelBinderProviders.Insert(0, new BlobHandlingModelBinderProvider());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc($"v{currentVersion.Major}", new OpenApiInfo() { Title = AppTitle, Version = currentVersion.ToString() });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMultiTenancy();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
var happenSoftIntelligenceConfiguration = builder.Configuration.GetSection("Intelligences:HappenSoft")
    .Get<HSIntelligenceConfiguration>();
if (happenSoftIntelligenceConfiguration.Enabled)
{
    builder.Services.AddSingleton(happenSoftIntelligenceConfiguration);
    builder.Services.AddHttpClient<HSIntelligence>((serviceProvider, client) =>
    {
        var configuration = serviceProvider.GetRequiredService<HSIntelligenceConfiguration>();
        client.BaseAddress = configuration.BaseAddress;
        client.DefaultRequestHeaders.Add("ApiKey", configuration.ApiKey);
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };
    });
    builder.Services.AddScoped<IReceiptInterpreter>(sp => sp.GetRequiredService<HSIntelligence>());

}

var openAiIntelligenceConfiguration = builder.Configuration.GetSection("Intelligences:OpenAI")
    .Get<OpenAiIntelligenceConfiguration>();
if (openAiIntelligenceConfiguration.Enabled)
{
    builder.Services.AddSingleton(openAiIntelligenceConfiguration);
    builder.Services.AddHttpClient<OpenAiIntelligence>((serviceProvider, client) =>
    {
        var configuration = serviceProvider.GetRequiredService<OpenAiIntelligenceConfiguration>();
        client.BaseAddress = configuration.BaseAddress;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.ApiKey);
        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };
    });
    builder.Services.AddScoped<IReceiptInterpreter>(sp => sp.GetRequiredService<OpenAiIntelligence>());

}

var googleVertexAiIntelligenceConfiguration = builder.Configuration.GetSection("Intelligences:GoogleVertexAI")
    .Get<GoogleVertexAiIntelligenceConfiguration>();
if (googleVertexAiIntelligenceConfiguration.Enabled)
{
    builder.Services.AddSingleton(googleVertexAiIntelligenceConfiguration);
    builder.Services.AddHttpClient<GoogleVertexAiIntelligence>((serviceProvider, client) =>
    {
        var configuration = serviceProvider.GetRequiredService<GoogleVertexAiIntelligenceConfiguration>();
        client.BaseAddress = configuration.BaseAddress;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.AccessKey);
    });
    builder.Services.AddScoped<IReceiptInterpreter>(sp => sp.GetRequiredService<GoogleVertexAiIntelligence>());
}

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint($"./swagger/v{currentVersion.Major}/swagger.json", AppTitle);
        config.DocumentTitle = AppTitle;
        config.RoutePrefix = string.Empty;
    });
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();