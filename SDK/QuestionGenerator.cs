using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;

namespace SDK;

public static class QuestionGenerator
{
    public static string GenerateQuestion(ImageQuery query)
    {
        var question = new StringBuilder("What is the probability that the image  ");
        var criteriaLookup = new Dictionary<int, string?>
        {
            { 1, query.Retailer },
            { 2, query.Brand },
            { 3, query.Product },
            { 4, query.Quantity?.ToString() },
            { 5, query.Uom }
        };
        if (criteriaLookup.Values.All(x => !string.IsNullOrWhiteSpace(x)))
            question.Append(
                $"is from the retailer {query.Retailer}, " +
                $"has a product called {query.Product} from a brand called {query.Brand} in a quantity numbered {query.Quantity} of uom {query.Uom}");
        else
        {
            if (!string.IsNullOrWhiteSpace(criteriaLookup[1]))
                question.Append($"is from the retailer {query.Retailer}, ");
            if (!string.IsNullOrWhiteSpace(criteriaLookup[3]) && !string.IsNullOrWhiteSpace(criteriaLookup[2]))
                question.Append($"has a product called {query.Product} from a brand called {query.Brand} ");
            else if (!string.IsNullOrWhiteSpace(criteriaLookup[3]) && string.IsNullOrWhiteSpace(criteriaLookup[2]))
                question.Append($"has a product called {query.Product} ");
            else if (string.IsNullOrWhiteSpace(criteriaLookup[3]) && !string.IsNullOrWhiteSpace(criteriaLookup[2]))
                question.Append($"has a product from a brand called {query.Brand} ");
            if (!string.IsNullOrWhiteSpace(criteriaLookup[4]))
                question.Append($"in a quantity {query.Quantity} ");
            if (!string.IsNullOrWhiteSpace(criteriaLookup[5]))
                question.Append($"{query.Uom}");
        }

        return question.ToString();
    }
}