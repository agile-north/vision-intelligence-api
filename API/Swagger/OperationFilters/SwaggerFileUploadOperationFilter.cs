using Contracts;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var blobProperties = context.ApiDescription.ParameterDescriptions.SelectMany(description =>
            description.Type.GetProperties().Where(p => p.PropertyType == typeof(Blob)).Select(b => new { Parameter = description, Property = b })).GroupBy(x => x.Parameter);

        if (blobProperties.Any())
        {
            foreach (var param in blobProperties)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = context.SchemaRepository.Schemas.FirstOrDefault(x=>x.Key == param.Key.Type.Name).Value
                        }
                    }
                };

                var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

                foreach (var blobProperty in param)
                {
                    var key = schema.Properties.Keys.FirstOrDefault(k => string.Equals(blobProperty.Property.Name, k, StringComparison.OrdinalIgnoreCase));
                    schema.Properties[key] = new OpenApiSchema
                    {
                        Description = $"Upload {blobProperty.Property.Name}",
                        Type = "string",
                        Format = "binary"
                    };
                }
            }
        }
    }
}
