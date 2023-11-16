using System.Net.Http.Headers;
using Implementations.OpenAI;
using SDK;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(builder.Configuration.GetSection("Intelligences:OpenAI")
    .Get<OpenAiIntelligenceConfiguration>());
builder.Services.AddHttpClient<OpenAiIntelligence>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<OpenAiIntelligenceConfiguration>();
    client.BaseAddress = configuration.BaseAddress;
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.ApiKey);
});
builder.Services.AddScoped<IImageInterpreter>(sp => sp.GetRequiredService<OpenAiIntelligence>());
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