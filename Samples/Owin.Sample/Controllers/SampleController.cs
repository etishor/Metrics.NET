using System.Collections.Generic;
using System.Web.Http;

namespace Owin.Sample.Controllers
{
    [RoutePrefix("sample")]
    public class SampleController : ApiController
    {
        [Route("")]
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }
    }
}
