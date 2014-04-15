using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests
{
    public class ActionSchedulerTests
    {
        [Fact]
        public void ActionSchedulerExecutesScheduledFunction()
        {
            using (ActionScheduler scheduler = new ActionScheduler())
            {
                var tcs = new TaskCompletionSource<bool>();
                int data = 0;

                Func<CancellationToken, Task> function = (t) => Task.Factory.StartNew(() =>
                    {
                        data++;
                        tcs.SetResult(true);
                    });

                scheduler.Start(TimeSpan.FromMilliseconds(10), function);
                tcs.Task.Wait();
                scheduler.Stop();

                data.Should().Be(1);
            }
        }

        [Fact]
        public void ActionSchedulerExecutesScheduledAction()
        {
            using (ActionScheduler scheduler = new ActionScheduler())
            {
                var tcs = new TaskCompletionSource<bool>();
                int data = 0;

                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    data++;
                    tcs.SetResult(true);
                });

                tcs.Task.Wait();
                scheduler.Stop();

                data.Should().Be(1);
            }
        }

        [Fact]
        public void ActionSchedulerExecutesScheduledActionWithToken()
        {
            using (ActionScheduler scheduler = new ActionScheduler())
            {
                int data = 0;
                var tcs = new TaskCompletionSource<bool>();

                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    data++;
                    tcs.SetResult(true);
                });

                tcs.Task.Wait();
                scheduler.Stop();
                data.Should().Be(1);
            }
        }

        [Fact]
        public void ActionSchedulerExecutesScheduledActionMultipleTimes()
        {
            using (ActionScheduler scheduler = new ActionScheduler())
            {
                int data = 0;
                var tcs = new TaskCompletionSource<bool>();

                scheduler.Start(TimeSpan.FromMilliseconds(10), () =>
                {
                    data++;
                    tcs.SetResult(true);
                });

                tcs.Task.Wait();
                data.Should().Be(1);

                tcs = new TaskCompletionSource<bool>();
                tcs.Task.Wait();
                data.Should().Be(2);

                scheduler.Stop();
            }
        }

        [Fact]
        public void ActionSchedulerReportsExceptionWithGlobalMetricHandler()
        {
            Exception x = null;
            var tcs = new TaskCompletionSource<bool>();

            Metric.ErrorHandler = e =>
            {
                x = e;
                tcs.SetResult(true);
            };

            using (ActionScheduler scheduler = new ActionScheduler())
            {
                scheduler.Start(TimeSpan.FromMilliseconds(10), t =>
                {
                    throw new InvalidOperationException("boom");
                });

                tcs.Task.Wait(20);
                scheduler.Stop();
            }

            x.Should().NotBeNull();
        }
    }
}
