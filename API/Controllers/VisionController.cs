using Contracts;
using Microsoft.AspNetCore.Mvc;
using SDK;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class VisionController : ControllerBase
{
    private readonly IEnumerable<IImageInterpreter> _imageInterpreters;

    public VisionController(IEnumerable<IImageInterpreter> imageInterpreters)
    {
        _imageInterpreters = imageInterpreters;
    }

    [HttpPost("AnalyseImage")]
    public async Task<ImageQueryResult> AnalyseImageAsync(IFormFile? file,[FromQuery] FormImageQuery query)
    {
        var interpreter = _imageInterpreters.FirstOrDefault();
        if (interpreter == null)
            throw new NotImplementedException();
        var q = new ImageQuery
        {
            Brand = query.Brand,
            Product = query.Product,
            Detail = query.Detail,
            Quantity = query.Quantity,
            Retailer = query.Retailer,
            Uom = query.Uom
        };
        try
        {
            if (file != null)
            {
                byte[] data;
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    data = ms.ToArray();
                }

                q.Base64 = Convert.ToBase64String(data);
                q.ContentType = file.ContentType;
                return await interpreter.InterpretImage(q);
            }
        }
        catch (Exception ex)
        {
            return new ImageQueryResult
            {
                ImprovementHint = "Image could not interpreted",
                Exception = ex.GetBaseException().ToString()
            };
        }
        return new ImageQueryResult
        {
            ImprovementHint = "Image could not interpreted",
        };
    }
}