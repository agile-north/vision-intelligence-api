using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Contracts;

namespace SDK;

public static class QuestionGenerator
{
    public static string GenerateQuestion(ImageQuery query)
    {
        var question = new List<string>();

        question.Add($"Given the image of a receipt, confirm that it meets each of the following criteria exactly:");

        if (!string.IsNullOrWhiteSpace(query.Retailer))
            question.Add($"The retailer or vendor is '{query.Retailer}' represented exactly in text");

        if (!string.IsNullOrWhiteSpace(query.Product))
        {
            question.Add($"It contains the product '{query.Product}' represented exactly in text");
        }

        if (query.Products?.Items.Any() ?? false)
        {
            foreach (var product in query.Products.Items)
            {
                var items = new List<string>();
                items.Add($"It contains the product name '{product.Product}' represented exactly in text");

                if (product.Quantity.HasValue)
                    items.Add($"with a quantity of at least '{product.Quantity.Value}{product.Uom ?? ""}'");

                question.Add(string.Join(" ", items.ToArray()));
            }
        }

        if (query.Brands?.Items.Any() ?? false)
        {
            foreach (var brand in query.Brands.Items)
            {
                var items = new List<string>();
                items.Add($"It contains the brand '{brand}' represented exactly in text");
                question.Add(string.Join(" ", items.ToArray()));
            }
        }

        var responseFormat = JsonSerializer.Serialize(new ImageQueryResult
        {
            Certainty = 50,
            ImprovementHint = "",
            Exception = ""
        }, new JsonSerializerOptions { WriteIndented = false });

        question.Add("All retailers and product names need to match the spelling provided exactly, don't consider if words might be a typo or truncated.");

        question.Add($"Have the response returned not in markdown but in raw unescaped json format like this: '{responseFormat}' as an array for each criteria");

        question.Add($"where each entry has '{nameof(ImageQueryResult.Certainty)}' as 100 when all the criteria was fulfilled exactly and 0 if not.");
        question.Add($"where '{nameof(ImageQueryResult.ImprovementHint)}' gives a hint on how the certainty can be improved by providing better information or image. This can also be used to indicate how you matched the criteria.");

        return string.Join(Environment.NewLine, question.ToArray());
    }
}