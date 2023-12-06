using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class StatusController:ControllerBase
{
 
    public static string Version = Assembly.GetAssembly(typeof(StatusController)).GetName().Version.ToString();
    public static DateTime UpSince = DateTime.UtcNow;
    public class StatusResponse
    {
        public string Version { get; set; }
        public DateTime UpSince { get; set; }
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
        public TimeSpan UpTime => ServerTime.Subtract(UpSince);
    }

    [HttpPost]
    [HttpGet]
    [HttpOptions]
    [HttpHead]
    public IActionResult Get()
    {
        return Ok(new StatusResponse
        {
            UpSince = UpSince,
            Version = Version
        });
    }

}