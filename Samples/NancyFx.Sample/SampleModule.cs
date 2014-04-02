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
            this.MetricForRequestTime<SampleModule>("TestRequest", "Get", "/test");

            Get["/test"] = _ => Response.AsText("test");
            Get["/error"] = _ => { throw new InvalidOperationException(); };
        }
    }
}
