using System;

namespace Contracts
{
    public class ImageQueryResult
    {
        public double? Certainty { get; set; }
        public string? ImprovementHint { get; set; }
        public string? Exception { get; set; }
    }
}