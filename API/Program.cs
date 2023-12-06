using System.Net.Http.Headers;
using API;
using Implementations.GoogleVertexAI;
using Implementations.OpenAI;
using Microsoft.OpenApi.Models;
using SDK;
using Common;
using Implementations.HappenSoft;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // options.ModelBinderProviders.Insert(0, new BlobHandlingModelBinderProvider());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Vision Intelligence API", Version = "v1" });
    // c.OperationFilter<SwaggerFileUploadOperationFilter>();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMultiTenancy();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();