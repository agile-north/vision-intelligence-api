using System;
using System.Collections;
using System.Collections.Generic;

namespace Contracts
{
    public class Blob
    {
        public byte[]? Data { get; set; }

        public string? Base64 { get; set; }
        public string? ContentType { get; set; }
        public string AsBase64()
        {
            if (Base64 != null)
                return Base64;

            return Convert.ToBase64String(Data!);
        }
        public string AsDataUrl()
        {
            var base64 = AsBase64();
            return $"data:{ContentType};base64,{base64}";
        }
    }

    public class ImageQuery
    {
        public string Quality = "high";
        public string? Retailer { get; set; }
        public ImageCriteria<string> Brands { get; set; } = new ImageCriteria<string>();
        public string? Product { get; set; }
        public ImageCriteria<ImageQueryProductCriteria> Products { get; set; } = new ImageCriteria<ImageQueryProductCriteria>();
        public Blob? Image { get; set; }
    }

    public class ImageCriteria<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public bool Any { get; set; } = false;
    }

    public class ImageQueryProductCriteria
    {
        public string? Product { get; set; }
        public int? Quantity { get; set; }
        public string? Uom { get; set; }
    }

    public class FormImageQuery
    {
        public string Detail { get; set; } = "low";
        public string? Retailer { get; set; }
        public string? Brand { get; set; }
        public string? Product { get; set; }
        public int? Quantity { get; set; }
        public string? Uom { get; set; }
    }
}