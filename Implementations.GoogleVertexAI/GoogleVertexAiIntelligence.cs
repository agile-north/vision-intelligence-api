using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Contracts;
using SDK;

namespace Implementations.GoogleVertexAI;

public class GoogleVertexAiIntelligence : Intelligence<GoogleVertexAiIntelligenceConfiguration>, IImageInterpreter
{
    private HttpClient HttpClient { get; }

    public GoogleVertexAiIntelligence(GoogleVertexAiIntelligenceConfiguration configuration, HttpClient httpClient) :
        base(configuration)
    {
        HttpClient = httpClient;
    }

    public Task<ImageQueryResult> InterpretImage(ImageQuery query)
    {
        return Request(QuestionGenerator.GenerateQuestion(query), query.Base64!);
    }

    private async Task<ImageQueryResult> Request(string prompt, string bytesBase64EncodedImage)
    {
        var result = new ImageQueryResult();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(HttpClient.BaseAddress!,
                $"v1/projects/{Configuration.ProjectId}/locations/us-central1/publishers/google/models/imagetext:predict")
        };
        var body = new
        {
            instances = new[]
            {
                new
                {
                    prompt,
                    image = new
                    {
                        bytesBase64Encoded = bytesBase64EncodedImage
                    }
                }
            },
            parameters = new
            {
                sampleCount = 3
            }
        };
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await HttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            result.Exception = $"{response.ReasonPhrase}/n{await response.Content.ReadAsStringAsync()}";
        }
        else
        {
            var responseResult =
                await JsonSerializer.DeserializeAsync<Result>(await response.Content.ReadAsStreamAsync());
            if (responseResult != null && responseResult.predictions.Count() == 3 &&
                double.TryParse(responseResult.predictions.ElementAt(0), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var first) &&
                double.TryParse(responseResult.predictions.ElementAt(1), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var second) &&
                double.TryParse(responseResult.predictions.ElementAt(2), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var third))
            {
                result.Certainty = Math.Round((first + second + third) / 3, 2);
                if (result.Certainty < 90)
                    result.ImprovementHint =
                        "Please upload a cleaner image following the instructions provided (if any)";
            }
            else
            {
                result.Certainty = 0d;
                result.ImprovementHint = "Please upload a cleaner image following the instructions provided (if any)";
            }
        }

        return result;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private record Result
    {
        public IEnumerable<string> predictions { get; set; } = new List<string>();
        public string? deployedModelId { get; set; }
        public string? model { get; set; }
        public string? modelDisplayName { get; set; }
        public string? modelVersionId { get; set; }
    }
}