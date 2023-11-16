using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Contracts;
using SDK;

namespace Implementations.OpenAI;

public class OpenAiIntelligence : Intelligence<OpenAiIntelligenceConfiguration>, IImageInterpreter
{
    private HttpClient HttpClient { get; }

    public OpenAiIntelligence(OpenAiIntelligenceConfiguration configuration, HttpClient httpClient) : base(configuration)
    {
        HttpClient = httpClient;
    }

    public async Task<ImageQueryResult> InterpretImage(ImageQuery query)
    {
    
        await Request(
            new
            {
                type = "text",
                text = QuestionGenerator.GenerateQuestion(query)
            },
            new
            {
                type = "image_url",
                image_url = new
                {
                    url = !string.IsNullOrWhiteSpace(query.Base64)
                        ? $"data:{query.ContentType};base64,{query.Base64}"
                        : query.Url?.ToString(),
                    detail = query.Detail
                }
            });
        return new ImageQueryResult();
    }

    private async Task Request(params object[] content)
    {
        var request = new HttpRequestMessage
        { Method = HttpMethod.Post, RequestUri = new Uri(HttpClient.BaseAddress!, "v1/chat/completions") };

        var body = new
        {
            model = "gpt-4-vision-preview",
            messages = new object[]{
                new {
                    content,
                    role = "user"
                }
            },
            max_tokens = Configuration.MaxTokens
        };

        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await HttpClient.SendAsync(request);

        var str = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
    }
}