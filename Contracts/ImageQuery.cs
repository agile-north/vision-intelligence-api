using System;

namespace Contracts
{
    public class ImageQuery
    {
        public string Detail = "low";
        public Uri? Url { get; set; }
        public string? Base64 { get; set; }
        public string? Retailer { get; set; }
        public string? Brand { get; set; }
        public string? Product { get; set; }
        public int? Quantity { get; set; }
        public string? Uom { get; set; }
        public string ContentType { get; set; } = "image/jpeg";
    }

    public class FormImageQuery
    {
        public string Detail { get; set; }= "low";
        public string? Retailer { get; set; }
        public string? Brand { get; set; }
        public string? Product { get; set; }
        public int? Quantity { get; set; }
        public string? Uom { get; set; }
    }
}