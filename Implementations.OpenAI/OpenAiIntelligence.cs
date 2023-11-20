using System.Net.Http.Json;
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

        var chatCompletion = await Request(
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
                    url = query.Image!.AsDataUrl(),
                    detail = query.Quality
                }
            });

        var s = chatCompletion.Choices.FirstOrDefault()?.Message?.Content;

        var defaultValue = new ImageQueryResult();
        if (s == null)
            return defaultValue;
        var results = JsonSerializer.Deserialize<ImageQueryResult[]>(s!);
        return new ImageQueryResult
        {
            ImprovementHint = string.Join(Environment.NewLine,
                results?.Where(x => !string.IsNullOrWhiteSpace(x.ImprovementHint)).Select(x => x.ImprovementHint) ??
                Array.Empty<string>()),
            Exception = string.Join(Environment.NewLine,
                results?.Where(x => !string.IsNullOrWhiteSpace(x.Exception)).Select(x => x.Exception) ??
                Array.Empty<string>()),
            Certainty = results?.Average(x => x.Certainty) ?? 0
        };
    }

    private async Task<ChatCompletion> Request(params object[] content)
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

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ChatCompletion>();
    }
}