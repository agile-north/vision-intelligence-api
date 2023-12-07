using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Contracts;
using Contracts.Receipts;
using SDK;

namespace Implementations.OpenAI;

public class OpenAiIntelligence : Intelligence<OpenAiIntelligenceConfiguration>, IReceiptInterpreter
{
    private HttpClient HttpClient { get; }

    public OpenAiIntelligence(OpenAiIntelligenceConfiguration configuration, HttpClient httpClient) : base(
        configuration)
    {
        HttpClient = httpClient;
    }

    public Task<ReceiptQueryResult> Interpret(ReceiptQuery query)
    {
        var first = query.Criteria.FirstOrDefault();
        return Interpret(first, query.Image);
    }

    public async Task<ReceiptQueryResult> Interpret(ReceiptCriteria query, Blob image)
    {
        var imageUrl = image!.AsDataUrl().ToString();

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
                    url = imageUrl,
                    detail = query.Quality
                }
            });

        var s = chatCompletion.Choices.FirstOrDefault()?.Message?.Content;

        var defaultValue = new ReceiptQueryResult();
        if (s == null)
            return defaultValue;
        var startIndexOfJson = s.IndexOf("[", StringComparison.InvariantCulture);
        var lastIndexOfJson =s.LastIndexOf("]", StringComparison.InvariantCulture);
        if (startIndexOfJson == -1 || lastIndexOfJson == -1)
            throw new Exception(s);
        s = s.Substring(startIndexOfJson, lastIndexOfJson - startIndexOfJson + 1);
        try
        {
            var results = JsonSerializer.Deserialize<ReceiptQueryResult[]>(s!);
            return new ReceiptQueryResult
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
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task<ChatCompletion> Request(params object[] content)
    {
        var request = new HttpRequestMessage
            { Method = HttpMethod.Post, RequestUri = new Uri(HttpClient.BaseAddress!, "v1/chat/completions") };

        var body = new
        {
            model = "gpt-4-vision-preview",
            messages = new object[]
            {
                new
                {
                    content,
                    role = "user"
                }
            },
            max_tokens = Configuration.MaxTokens
        };

        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await HttpClient.SendAsync(request);


        if (!response.IsSuccessStatusCode)
            Debugger.Break();

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ChatCompletion>();
    }
}