using System.Net.Http.Headers;
using API;
using Implementations.GoogleVertexAI;
using Implementations.OpenAI;
using Microsoft.OpenApi.Models;
using SDK;

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
    });
    builder.Services.AddScoped<IImageInterpreter>(sp => sp.GetRequiredService<OpenAiIntelligence>());

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
    builder.Services.AddScoped<IImageInterpreter>(sp => sp.GetRequiredService<GoogleVertexAiIntelligence>());
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();