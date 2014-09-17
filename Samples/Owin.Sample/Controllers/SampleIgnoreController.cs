
namespace Owin.Sample.Controllers
{
    using System.Web.Http;

    [RoutePrefix("sampleignore")]
    public class SampleIgnoreController : ApiController
    {
        [Route("")]
        public string Get()
        {
            return "get";
        }
    }
}
