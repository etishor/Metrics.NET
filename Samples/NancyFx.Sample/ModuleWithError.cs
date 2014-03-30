using System;
using Nancy;

namespace NancyFx.Sample
{
    public class ModuleWithError : NancyModule
    {
        public ModuleWithError()
            : base("/error")
        {
            Get["/"] = _ => { throw new InvalidOperationException(); };
        }
    }
}
