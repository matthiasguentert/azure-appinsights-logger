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
    }
}

public class TestData
{
    public string Name { get; set; }
    public string Blog { get; set; }
    public string Topics { get; set; }
}