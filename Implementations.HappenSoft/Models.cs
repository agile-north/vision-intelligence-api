namespace Implementations.HappenSoft;

using Contracts;
using System.Collections.Generic;

public class SubmitReceiptRequest
{
    public string CampaignKey { get; set; }
    public Blob ReceiptImage { get; set; }
}

public class ConfigureCampaignRequest
{
    public string CampaignKey { get; set; }
    public bool ValidateStore { get; set; }
    public bool ValidateIssuedDate { get; set; }
    public bool Active { get; set; }
    public string CampaignName { get; set; }
    public List<string> ProductCodes { get; set; } = new List<string>();
    public List<string> Stores { get; set; } = new List<string>();
    public List<DateTimeOffset> ActiveDateRange { get; set; } = new List<DateTimeOffset>();
}

public class SubmitReceiptResponse
{
    public bool Matched { get; set; }
    public List<string> OcrLines { get; set; }
    public List<string> ProductLines { get; set; }
    public string StoreName { get; set; }
    public DateTime IssuedDateTime { get; set; }
    public List<MatchedProductCode> MatchedProductCodes { get; set; }
    public string Description { get; set; }
    public string Heading { get; set; }
    public bool WasSuccessful { get; set; }
}

public class MatchedProductCode
{
    public string ProductCode { get; set; }
    public int Quantity { get; set; }
}

public class ConfigureCampaignResponse
{
    public string CampaignKey { get; set; }
    public string Description { get; set; }
    public string Heading { get; set; }
    public bool WasSuccessful { get; set; }
}
