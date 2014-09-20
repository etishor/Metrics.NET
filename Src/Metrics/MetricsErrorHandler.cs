
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Metrics.Logging;
namespace Metrics
{
    public class MetricsErrorHandler
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();
        private static readonly MetricsErrorHandler handler = new MetricsErrorHandler();

        private ConcurrentBag<Action<Exception, string>> handlers = new ConcurrentBag<Action<Exception, string>>();

        private MetricsErrorHandler()
        {
            this.AddHandler((x, msg) => log.ErrorException("Metrics: Unhandled exception in Metrics.NET Library", x));
            this.AddHandler((x, msg) => Trace.TraceError("Metrics: Unhandled exception in Metrics.NET Library " + x.ToString()));
            if (Environment.UserInteractive)
            {
                this.AddHandler((x, msg) => Console.WriteLine("Metrics: Unhandled exception in Metrics.NET Library " + x.ToString()));
            }
        }

        internal static MetricsErrorHandler Handler { get { return handler; } }

        internal void AddHandler(Action<Exception> handler)
        {
            AddHandler((x, msg) => handler(x));
        }

        internal void AddHandler(Action<Exception, string> handler)
        {
            handlers.Add(handler);
        }

        internal void ClearHandlers()
        {
            Action<Exception, string> item;
            while (!this.handlers.IsEmpty)
            {
                this.handlers.TryTake(out item);
            }
        }

        private void InternalHandle(Exception exception, string message)
        {
            foreach (var handler in this.handlers)
            {
                handler(exception, message);
            }
        }

        public static void Handle(Exception exception)
        {
            MetricsErrorHandler.Handle(exception, string.Empty);
        }

        public static void Handle(Exception exception, string message)
        {
            MetricsErrorHandler.handler.InternalHandle(exception, message);
        }
    }
}
