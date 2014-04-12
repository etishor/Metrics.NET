using System;
using Nancy;
using Nancy.Metrics;

namespace NancyFx.Sample
{
    public class SampleModule : NancyModule
    {
        public SampleModule()
            : base("/")
        {
            this.MetricForRequestTimeAndResponseSize("TestRequest", "Get", "/test");
            this.MetricForRequestSize("TestRequestSize", "Post", "/action");

            Get["/test"] = _ => Response.AsText("test");

            Post["/action"] = _ => HttpStatusCode.Accepted;

            Get["/error"] = _ => { throw new InvalidOperationException(); };

            Get["/item/{id}"] = p => Response.AsText((string)p.id, "text/plain");
        }
    }
}
