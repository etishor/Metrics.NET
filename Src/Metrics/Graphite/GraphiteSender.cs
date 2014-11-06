using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace Metrics.Graphite
{
    public abstract class GraphiteSender : IDisposable
    {
        private static readonly Regex cleaner = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public void Send(IEnumerable<string> nameParts, string value, long timestamp)
        {
            var name = FormName(nameParts);
            var data = string.Concat(name, " ", value, " ", timestamp.ToString());
            SendData(data);
        }

        private static string FormName(IEnumerable<string> nameParts)
        {
            return string.Join(".", nameParts.Select(n => Clean(n)));
        }

        private static string Clean(string name)
        {
            return cleaner.Replace(name, "-");
        }

        protected abstract void SendData(string data);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }
    }
}
