using System.Collections.Generic;
using System.Web.Http;

namespace Owin.Sample.Controllers
{
    public class SampleController : ApiController
    {
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        public IEnumerable<string> Get(int x, string y)
        {
            return new[] { "value1", "value2" };
        }
    }
}
