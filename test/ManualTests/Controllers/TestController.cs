using Microsoft.AspNetCore.Mvc;

namespace ManualTests.Controllers
{
    [Route("api/")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("echo")]
        public ActionResult<TestData> Echo([FromBody] TestData data)
        {
            return Ok(data);
        }

        [HttpPost("maxbytes")]
        public ActionResult<string> MaxBytesTest([FromForm] string data)
        {
            return Ok(data);
        }

        [HttpPost("throw")]
        public ActionResult<string> ThrowException([FromBody] TestData data)
        {
            throw new InvalidOperationException("Test exception!");
        }

        [HttpPost("redact")]
        public ActionResult<string> RedactSensitiveData([FromBody] SensitiveData data)
        {
            return Ok(data);
        }
    }
}
