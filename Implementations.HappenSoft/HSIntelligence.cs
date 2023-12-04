using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Common.MultiTenancy;
using Common.Runtime.ExceptionServices;
using Contracts;
using Contracts.Receipts;
using SDK;

namespace Implementations.HappenSoft;

public class HSIntelligence : Intelligence<HSIntelligenceConfiguration>, IReceiptInterpreter
{
    private ITenantAccessor _tenantAccessor;
    private HttpClient HttpClient { get; }

    public HSIntelligence(HSIntelligenceConfiguration configuration, HttpClient httpClient, ITenantAccessor tenantAccessor) : base(configuration)
    {
        HttpClient = httpClient;
        _tenantAccessor = tenantAccessor;
    }

    public Task<ReceiptQueryResult> Interpret(ReceiptQuery query)
    {
        return Interpret(query, query.Image);
    }

    ConfigureCampaignRequest FromReceiptQuery(string tenant, ReceiptQuery query)
    {
        var req = new ConfigureCampaignRequest
        {
            CampaignName  = tenant,
            CampaignKey = tenant,
            Stores = query.Criteria.SelectMany(c=>c.Retailers).ToList(),
            Active = true,
            ProductCodes = query.Criteria.SelectMany(c => c.Products.Items.Where(p => !string.IsNullOrWhiteSpace(p!.Product)).Select(p => p!.Product)).ToList(),
        };

        var first = query.Criteria.FirstOrDefault();
        
        if (first.FromDate.HasValue)
            req.ActiveDateRange.Add(first.FromDate.Value);

        if (first.ToDate.HasValue)
            req.ActiveDateRange.Add(first.ToDate.Value);

        if (req.ActiveDateRange.Any())
            req.ValidateIssuedDate = true;

        req.ValidateStore = true;

        return req;
    }

    public async Task<ReceiptQueryResult> Interpret(ReceiptQuery query, Blob image)
    {
        var defaultValue = new ReceiptQueryResult();
        if (query == null)
            return defaultValue;

        var tenantId = await _tenantAccessor.ResolveTenantAsync();

        if (string.IsNullOrWhiteSpace(tenantId))
            tenantId = Guid.NewGuid().ToString();

        try {
            var configueResponse = await ConfigureCampaign(FromReceiptQuery(tenantId, query));

            if (!configueResponse.WasSuccessful)
                throw new Exception(configueResponse.Description);

            var scanResponse = await SubmitReceipt(new SubmitReceiptRequest
            {
                CampaignKey = tenantId,
                ReceiptImage = image
            });

            return new ReceiptQueryResult
            {
                Certainty = ((scanResponse?.WasSuccessful ?? false) ? 100 : 0),
                ImprovementHint = scanResponse?.Description ?? $"There was an error",
            };
        }
        catch(Exception ex)
        {
            return new ReceiptQueryResult
            {
                Certainty = 0,
                ImprovementHint = $"There was an error",
                Exception = string.Join(Environment.NewLine + Environment.NewLine, ex.GetAllExceptions().Select(x=>$"{x.Message}{Environment.NewLine}{x.StackTrace}"))
            };
        }
    }

    private async Task<ConfigureCampaignResponse> ConfigureCampaign(ConfigureCampaignRequest request)
    {
        var http = new HttpRequestMessage
        { Method = HttpMethod.Post, RequestUri = new Uri(HttpClient.BaseAddress!, "api/ReceiptOcr/ConfigureCampaign") };

        http.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var response = await HttpClient.SendAsync(http);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ConfigureCampaignResponse>();
    }

    private async Task<SubmitReceiptResponse> SubmitReceipt(SubmitReceiptRequest request)
    {
        var http = new HttpRequestMessage
        { Method = HttpMethod.Post, RequestUri = new Uri(HttpClient.BaseAddress!, "api/ReceiptOcr/SubmitReceipt") };

        http.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        using var form = new MultipartFormDataContent("--------------------------785356097778383213449565");

        var data = Convert.FromBase64String(request.ReceiptImage!.Base64);
        using var ms = new MemoryStream(data);
        ms.Position = 0;
        
        var fileContent = new StreamContent(ms);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.ReceiptImage!.ContentType);
        fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
        {
            Name = $"\"{nameof(request.ReceiptImage)}\"",
            FileName = $"\"DsDL57nXQAEePRJ-02.jpg\""
        };
        form.Add(fileContent);

        var campaignKeyContent = new StringContent(request.CampaignKey);
        campaignKeyContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
        {
            Name = $"\"{nameof(request.CampaignKey)}\""
        };
        form.Add(campaignKeyContent);

        http.Content = form;

        var response = await HttpClient.SendAsync(http);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SubmitReceiptResponse>();
    }
}