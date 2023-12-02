using Contracts.Receipts;
using Microsoft.AspNetCore.Mvc;
using SDK;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class VisionController : ControllerBase
{
    private readonly IEnumerable<IReceiptInterpreter> _imageInterpreters;

    public VisionController(IEnumerable<IReceiptInterpreter> imageInterpreters)
    {
        _imageInterpreters = imageInterpreters;
    }

    [HttpPost("receipt/analyze")]
    // [Consumes("multipart/form-data")]
    public async Task<ReceiptQueryResult> AnalyseReceipt([FromBody] ReceiptQuery? query)
    {
        var interpreter = _imageInterpreters.FirstOrDefault();
        if (interpreter == null)
            throw new NotImplementedException();

        if (query == null)
            return new ReceiptQueryResult
            {
                ImprovementHint = "No criteria or image provided",
            };

        try
        {
            return await interpreter.Interpret(query);
        }
        catch (Exception ex)
        {
            return new ReceiptQueryResult
            {
                ImprovementHint = "Image could not interpreted",
                Exception = ex.GetBaseException().ToString()
            };
        }
    }
}