using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using Common;
using Common.MultiTenancy;
using Common.Runtime.ExceptionServices;
using Contracts;
using Contracts.Receipts;
using Microsoft.Extensions.Caching.Distributed;
using SDK;

namespace Implementations.HappenSoft;

public class HSIntelligence : Intelligence<HSIntelligenceConfiguration>, IReceiptInterpreter
{
    private readonly ITenantAccessor _tenantAccessor;
    private readonly IIdempotencyKeyProvider _idempotencyKeyProvider;
    private readonly IDistributedCache _distributedCache;
    private HttpClient HttpClient { get; }

    public HSIntelligence(HSIntelligenceConfiguration configuration,
        HttpClient httpClient,
        ITenantAccessor tenantAccessor,
        IIdempotencyKeyProvider idempotencyKeyProvider,
        IDistributedCache distributedCache) : base(configuration)
    {
        HttpClient = httpClient;
        _tenantAccessor = tenantAccessor;
        _idempotencyKeyProvider = idempotencyKeyProvider;
        _distributedCache = distributedCache;
    }

    public Task<ReceiptQueryResult> Interpret(ReceiptQuery query)
    {
        return Interpret(query, query.Image);
    }

    ConfigureCampaignRequest FromReceiptQuery(string campaignCode, string campaignName, ReceiptQuery query)
    {
        var req = new ConfigureCampaignRequest
        {
            CampaignName = campaignName,
            CampaignCode = campaignCode,
            Stores = query.Criteria.SelectMany(c => c.Retailers).ToList(),
            Active = true,
            ProductCodes = query.Criteria.SelectMany(c =>
                c.Products.Items.Where(p => !string.IsNullOrWhiteSpace(p!.Product)).Select(p => p!.Product)).ToList(),
        };

        var first = query.Criteria.FirstOrDefault();

        if (first.FromDate.HasValue)
            req.IssuedStartDate = first.FromDate.Value;

        if (first.ToDate.HasValue)
            req.IssuedEndDate = first.ToDate.Value;

        if (req.IssuedEndDate.HasValue || req.IssuedStartDate.HasValue)
            req.ValidateIssuedDate = true;

        req.ValidateStore = true;

        return req;
    }

    public async Task<ReceiptQueryResult> Interpret(ReceiptQuery query, Blob image)
    {
        var defaultValue = new ReceiptQueryResult();
        if (query == null)
            return defaultValue;

        var tenantSchemeCandidate = await _tenantAccessor.ResolveTenantAsync();
        //unpack tenant url
        if (!Uri.TryCreate(tenantSchemeCandidate, UriKind.Absolute, out var tenantScheme) ||
            tenantScheme.Scheme != "uwina" ||
            !tenantScheme.Query.Contains("name="))
            throw new InvalidCredentialException("Tenant");
        var campaignCode = tenantScheme.Host;
        var campaignName =
            tenantScheme.Query[tenantScheme.Query.LastIndexOf("name=", StringComparison.InvariantCulture)..];
        campaignName = campaignName.SplitExact("=")[1];

        var idempotencyKey = await _idempotencyKeyProvider.ResolveAsync();
        if (idempotencyKey is null)
            throw new ArgumentException("Idempotency");
        try
        {
            var key = $"Happensoft:ConfiguredCampaign:{campaignCode}";
            var provisioned = _distributedCache.Get(key);
            if (provisioned is null)
            {
                var configureResponse =
                    await ConfigureCampaign(FromReceiptQuery(campaignCode, campaignName, query), false);
                if (!configureResponse.WasSuccessful && !configureResponse.Description.Contains("unique"))
                    throw new Exception(configureResponse.Description);
                _distributedCache.Set(key, Encoding.UTF8.GetBytes("true"));
            }

            var updateResponse =
                await ConfigureCampaign(FromReceiptQuery(campaignCode, campaignName, query));

            var request = new SubmitReceiptRequest
            {
                CampaignCode = campaignCode,
                ReceiptId = idempotencyKey,
                ReceiptImage = image
            };

            var scanResponse = await SubmitReceipt(request);

            request.ReceiptImage = null; // remove the binary

            return new ReceiptQueryResult
            {
                Certainty = ((scanResponse?.Matched ?? false) ? 100 : 0),
                ImprovementHint = scanResponse?.Description ?? $"There was an error",
                Provider = new
                {
                    Type = typeof(HSIntelligence).FullName,
                    Request = request,
                    Response = scanResponse
                }
            };
        }
        catch (Exception ex)
        {
            return new ReceiptQueryResult
            {
                Certainty = 0,
                ImprovementHint = $"There was an error",
                Exception = string.Join(Environment.NewLine + Environment.NewLine,
                    ex.GetAllExceptions().Select(x => $"{x.Message}{Environment.NewLine}{x.StackTrace}"))
            };
        }
    }

    private async Task<ConfigureCampaignResponse> ConfigureCampaign(ConfigureCampaignRequest request, bool update = true)
    {
        var http = new HttpRequestMessage
        {
            Method = update ? HttpMethod.Put : HttpMethod.Post,
            RequestUri = new Uri(HttpClient.BaseAddress!, update ? "Campaign/UpdateCampaign" : "Campaign/CreateCampaign")
        };

        http.Content = update
            ? new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            : new StringContent(JsonSerializer.Serialize(new
            {
                request.CampaignCode,
                request.CampaignName
            }), Encoding.UTF8, "application/json");

        var response = await HttpClient.SendAsync(http);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ConfigureCampaignResponse>();
    }

    private async Task<SubmitReceiptResponse> SubmitReceipt(SubmitReceiptRequest request)
    {
        var http = new HttpRequestMessage
        { Method = HttpMethod.Post, RequestUri = new Uri(HttpClient.BaseAddress!, "Receipt/SubmitReceipt") };

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

        var campaignKeyContent = new StringContent(request.CampaignCode);
        campaignKeyContent.Headers.ContentDisposition =
            new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = $"\"{nameof(request.CampaignCode)}\""
            };
        form.Add(campaignKeyContent);
        var idempotencyKeyContent = new StringContent(request.ReceiptId);
        idempotencyKeyContent.Headers.ContentDisposition =
            new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = $"\"{nameof(request.ReceiptId)}\""
            };
        form.Add(idempotencyKeyContent);

        http.Content = form;

        var response = await HttpClient.SendAsync(http);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SubmitReceiptResponse>();
    }
}