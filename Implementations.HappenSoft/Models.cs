namespace Implementations.HappenSoft;

using Contracts;
using System.Collections.Generic;

public class SubmitReceiptRequest
{
    public string CampaignCode { get; set; }
    public string ReceiptId { get; set; }
    public Blob ReceiptImage { get; set; }
}

public class ConfigureCampaignRequest
{
    public string CampaignCode { get; set; }
    public bool ValidateStore { get; set; }
    public bool ValidateIssuedDate { get; set; }
    public bool Active { get; set; }
    public string CampaignName { get; set; }
    public List<string> ProductCodes { get; set; } = new List<string>();
    public List<string> Stores { get; set; } = new List<string>();
    public DateTimeOffset? IssuedStartDate { get; set; }
    public DateTimeOffset? IssuedEndDate { get; set; }
}

public class SubmitReceiptResponse
{
    public bool Matched { get; set; }
    public List<string> OcrLines { get; set; }
    public List<string> ProductLines { get; set; }
    public string StoreName { get; set; }
    public DateTime IssuedDateTime { get; set; }
    public List<MatchedProductCode> MatchedProducts { get; set; }
    public List<MatchedProductCode> FoundProducts { get; set; }
    public string Description { get; set; }
    public string Heading { get; set; }
    public string ModelState { get; set; }
    public string Reason { get; set; }
    public bool WasSuccessful { get; set; }
}

public class MatchedProductCode
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class ConfigureCampaignResponse
{
    public string CampaignKey { get; set; }
    public string Description { get; set; }
    public string Heading { get; set; }
    public bool WasSuccessful { get; set; }
}