using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Contracts;
using SDK;

namespace Implementations.OpenAI;

public class OpenAiIntelligence : Intelligence<OpenAiIntelligenceConfiguration>,IImageInterpreter
{
    private HttpClient HttpClient { get; }

    public OpenAiIntelligence(OpenAiIntelligenceConfiguration configuration,HttpClient httpClient) : base(configuration)
    {
        HttpClient = httpClient;
    }

    public async Task<ImageQueryResult> InterpretImage(ImageQuery query)
    {
        var question = new StringBuilder("What is the probability that the image  ");
        var criteriaLookup = new Dictionary<int, string?>
        {
            { 1, query.Retailer },
            { 2, query.Brand },
            { 3, query.Product },
            { 4, query.Quantity?.ToString() },
            { 5, query.Uom }
        };
        if (criteriaLookup.Values.All(x => !string.IsNullOrWhiteSpace(x)))
            question.Append(
                $"is from the retailer {query.Retailer} ," +
                $"has a product called {query.Product} from a brand called {query.Brand} in a quantity numbered {query.Quantity} of {query.Uom}");
        else
        {
            if (!string.IsNullOrWhiteSpace(criteriaLookup[1]))
                question.Append($"is from the retailer {query.Retailer} ,");
            if (!string.IsNullOrWhiteSpace(criteriaLookup[3]) && !string.IsNullOrWhiteSpace(criteriaLookup[2]))
                question.Append($"has a product called {query.Product} from a brand called {query.Brand} ");
            else if (!string.IsNullOrWhiteSpace(criteriaLookup[3]) && string.IsNullOrWhiteSpace(criteriaLookup[2]))
                question.Append($"has a product called {query.Product} ");
            else if (string.IsNullOrWhiteSpace(criteriaLookup[3]) && !string.IsNullOrWhiteSpace(criteriaLookup[2]))
                question.Append($"has a product from a brand called {query.Brand} ");
            if (!string.IsNullOrWhiteSpace(criteriaLookup[4]))
                question.Append($"in a quantity numbered {query.Quantity} ");
            if (!string.IsNullOrWhiteSpace(criteriaLookup[5]))
                question.Append($" of {query.Uom}");
        }

        await Request(
            new
            {
                type = "text",
                text = question.ToString()
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
        var body = new JsonObject
        {
            { "name", "gpt-4-vision-preview" },
            { "content", JsonNode.Parse(JsonSerializer.SerializeToUtf8Bytes(content)) },
            { "max_tokens", Configuration.MaxTokens }
        };
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

    }
}