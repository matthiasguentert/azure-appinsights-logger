using Microsoft.AspNetCore.Mvc;

namespace ManualTests.Controllers
{
    [Route("api/")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("test")]
        public ActionResult<TestData> Test([FromBody] TestData data)
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
    }
}
