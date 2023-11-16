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
    // [Consumes("multipart/form-data")]
    public async Task<ImageQueryResult> AnalyseImageAsync([FromBody] ImageQuery? query)
    {
        var interpreter = _imageInterpreters.FirstOrDefault();
        if (interpreter == null)
            throw new NotImplementedException();

        try
        {
            return await interpreter.InterpretImage(query);
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