using System;
using System.Collections.Generic;

namespace Contracts.Receipts
{
    public class ReceiptQuery
    {
        public List<ReceiptCriteria> Criteria { get; set; } = new List<ReceiptCriteria>();
        public Blob? Image { get; set; }
    }

    public class ReceiptCriteria
    {
        public string? Quality = "high";
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public List<string> Retailers { get; set; } = new List<string>();
        public AnyyOrCriteria<string> Brands { get; set; } = new AnyyOrCriteria<string>();
        public AnyyOrCriteria<ReceiptProductCriteria> Products { get; set; } = new AnyyOrCriteria<ReceiptProductCriteria>();
    }

    public class ReceiptProductCriteria
    {
        public string? Brand { get; set; }
        public string? Product { get; set; }
        public decimal? Quantity { get; set; }
        public string? Uom { get; set; }
    }
}
