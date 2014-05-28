using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Metrics.Visualization
{
    public static class FlotWebApp
    {
        private static string ReadFromEmbededResource()
        {
            using (var stream = Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.index.full.html.gz"))
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzip))
            {
                return reader.ReadToEnd();
            }
        }

        private static Lazy<string> htmlContent = new Lazy<string>(() => ReadFromEmbededResource());

        public static string GetFlotApp()
        {
            return htmlContent.Value;
        }
    }
}
