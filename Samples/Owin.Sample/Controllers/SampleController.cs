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

        [Route("withparams/{x}/{y}")]
        public IEnumerable<string> Get(int x, string y)
        {
            return new[] { "value1", "value2" };
        }
    }
}
