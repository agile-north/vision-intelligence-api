using Contracts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace API;

public class BlobHandlingModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType.GetProperties().Any(p => p.PropertyType == typeof(Blob)))
        {
            var defaultBinder = new ComplexTypeModelBinderProvider().GetBinder(context);
            return new BlobHandlingModelBinder(defaultBinder);
        }

        return null;
    }
}


