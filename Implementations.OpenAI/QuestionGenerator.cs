using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Contracts.Receipts;

namespace Implementations.OpenAI;

public static class QuestionGenerator
{
    public static string GenerateQuestion(ReceiptCriteria query)
    {
        var question = new List<string>();

        question.Add("Given the following criteria:");

        if (query.Retailers.Any())
            question.Add($"- The vendor must match exactly {string.Join(" or ", query.Retailers.Select(x=>$"'{x}'").ToArray())}");

        if (query.FromDate.HasValue)
            question.Add($"- The date is greater than {query.FromDate}");

        if (query.ToDate.HasValue)
            question.Add($"- The date is less than {query.FromDate}");

        if (query.Products?.Items.Any() ?? false)
        {
            foreach (var product in query.Products.Items)
            {
                var items = new List<string>();
                items.Add($"- It contains the product name '{product.Product}'");

                if (product.Quantity.HasValue)
                    items.Add($"with a numeric quantity of at least {product.Quantity.Value}");

                if (!string.IsNullOrWhiteSpace(product.Uom))
                    items.Add($" unit of measure of : {product.Uom}");

                question.Add(string.Join(" ", items.ToArray()));
            }
        }

        if (query.Brands?.Items.Any() ?? false)
        {
            foreach (var brand in query.Brands.Items)
            {
                var items = new List<string>();
                items.Add($"- It contains the brand '{brand}'");
                question.Add(string.Join(" ", items.ToArray()));
            }
        }

        var responseFormat = JsonSerializer.Serialize(new ReceiptQueryResult
        {
            Certainty = 50,
            ImprovementHint = "",
            Exception = ""
        }, new JsonSerializerOptions { WriteIndented = false });

        question.Add("Here are some guidelines on how to match criteria:");
        question.Add("- When matching criteria works enclused in single quotes must be matched with exact spelling of full words, this is very important to avoid false positives");
        question.Add("- Ignore casing differences");
        question.Add("- When determining quantity for a product, the quantity is usually listed below the product on a separate line");
        question.Add("- When comparing unit of measure, consider the metrics correctly to compare quantity, this is usually found on the right of the receipt next to the product");
        question.Add("- Ensure that the evaluation of the criteria is consistent when validating the receipt multiple times");

        question.Add("Here is how I want you to return the response:");
        question.Add($"Have the response returned not in natural language text or markdown but in raw unescaped json format like this: '{responseFormat}' as an array for each criteria");

        question.Add($"where each entry has '{nameof(ReceiptQueryResult.Certainty)}' as 100 when the criteria was fulfilled exactly and 0 if not.");
        question.Add($"where '{nameof(ReceiptQueryResult.ImprovementHint)}' gives a hint on how the certainty can be improved by providing better information or image. This can also be used to indicate how you matched the criteria without disclosing the asking criteria.");

        return string.Join(Environment.NewLine, question.ToArray());
    }
}