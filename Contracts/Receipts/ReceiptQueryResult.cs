using System;

namespace Contracts.Receipts
{
    public class ReceiptQueryResult
    {
        public double? Certainty { get; set; }
        public string? ImprovementHint { get; set; }
        public string? Exception { get; set; }
    }
}