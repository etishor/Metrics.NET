using System;
using System.Threading;

namespace Metrics.Reporters
{
  public interface IScheduledReporter : IDisposable
  {
    void RunReport(CancellationToken token);
    void Start();
    void Stop();
    new void Dispose();
  }
}