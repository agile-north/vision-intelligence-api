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

        question.Add($"Given the image, I want to find out if it meets the following criteria:");

        if (!string.IsNullOrWhiteSpace(query.Retailer))
            question.Add($"The retailer or vendor '{query.Retailer}' is mentioned with exact spelling");

        if (!string.IsNullOrWhiteSpace(query.Product))
        {
            question.Add($"'{query.Product}' is mentioned with exact spelling");
        }

        if (query.Products?.Items.Any() ?? false)
        {
            foreach (var product in query.Products.Items)
            {
                var items = new List<string>();
                items.Add($"The product '{product.Product}' is mentioned with exact spelling");

                if (product.Quantity.HasValue)
                    items.Add($"and has a quantity of at least '{product.Quantity.Value}'");

                question.Add(string.Join(" ", items.ToArray()));
            }
        }

        if (query.Brands?.Items.Any() ?? false)
        {
            foreach (var brand in query.Brands.Items)
            {
                var items = new List<string>();
                items.Add($"The brand '{brand}' is mentioned with exact spelling");
                question.Add(string.Join(" ", items.ToArray()));
            }
        }

        var responseFormat = JsonSerializer.Serialize(new ImageQueryResult
        {
            Certainty = 50,
            ImprovementHint = "",
            Exception = ""
        }, new JsonSerializerOptions { WriteIndented = false });

        question.Add("All the criteria with exact spelling have to match in order to give the certainty. If the spelling is not an exact match consider it not a match.");

        question.Add($"Have the response returned not in markdown but in raw unescaped json format like this: '{responseFormat}'");

        question.Add($"where '{nameof(ImageQueryResult.Certainty)}' gives the percentage of certainty of the criteria mentioned above matches.");
        question.Add($"where '{nameof(ImageQueryResult.ImprovementHint)}' gives a hint on how the certainty can be improved by providing better information or image. This can also be used to indicate how you matched the criteria.");

        return string.Join(Environment.NewLine, question.ToArray());
    }
}