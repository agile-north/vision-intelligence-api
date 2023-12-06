using Contracts;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API;

public class BlobHandlingModelBinder : IModelBinder
{
    private readonly IModelBinder _fallbackBinder;

    public BlobHandlingModelBinder(IModelBinder fallbackBinder)
    {
        _fallbackBinder = fallbackBinder;
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // First, call the fallback binder
        if (_fallbackBinder != null)
            await _fallbackBinder!.BindModelAsync(bindingContext);

        if (bindingContext.Result.IsModelSet)
        {
            // Get the model after default binding
            var model = bindingContext.Result.Model;

            // Now, handle the Blob properties
            foreach (var property in bindingContext.ModelType.GetProperties())
            {
                if (property.PropertyType == typeof(Blob))
                {
                    var file = bindingContext.HttpContext.Request.Form.Files.FirstOrDefault(f => string.Equals(f.Name, property.Name, StringComparison.InvariantCultureIgnoreCase));

                    if (file != null)
                    {
                        Blob blob = new Blob();
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            blob.Data = memoryStream.ToArray();
                        }
                        blob.ContentType = file.ContentType;

                        property.SetValue(model, blob);
                    }
                }
            }
        }
    }
}

// public class BlobHandlingModelBinder : IModelBinder
// {
//     private readonly IDictionary<Type, IModelBinder> _binders;

//     public BlobHandlingModelBinder(IDictionary<Type, IModelBinder> binders)
//     {
//         _binders = binders ?? throw new ArgumentNullException(nameof(binders));
//     }

//     public async Task BindModelAsync(ModelBindingContext bindingContext)
//     {
//         var modelType = bindingContext.ModelType;
//         if (_binders.TryGetValue(modelType, out var binder))
//         {
//             await binder.BindModelAsync(bindingContext);

//             if (!bindingContext.Result.IsModelSet)
//             {
//                 return;
//             }

//             var model = bindingContext.Result.Model;

//             // Handle Blob properties
//             foreach (var property in modelType.GetProperties())
//             {
//                 if (property.PropertyType == typeof(Blob))
//                 {
//                     var file = bindingContext.HttpContext.Request.Form.Files.FirstOrDefault(f => string.Equals(f.Name, property.Name, StringComparison.InvariantCultureIgnoreCase));

//                     if (file != null)
//                     {
//                         Blob blob = new Blob();
//                         using (var memoryStream = new MemoryStream())
//                         {
//                             await file.CopyToAsync(memoryStream);
//                             blob.Data = memoryStream.ToArray();
//                         }
//                         blob.ContentType = file.ContentType;

//                         property.SetValue(model, blob);
//                     }
//                 }
//             }

//             // Update the binding context with the fully bound model
//             bindingContext.Result = ModelBindingResult.Success(model);
//         }
//     }
// }