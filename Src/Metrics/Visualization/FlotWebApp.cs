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

        private static readonly Lazy<string> htmlContent = new Lazy<string>(ReadFromEmbededResource);

        public static string GetFlotApp()
        {
            return htmlContent.Value;
        }

        public const string FavIconMimeType = "image/png";

        public static void WriteFavIcon(Stream output)
        {
            using (var stream = Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.metrics_32.png"))
            {
                stream.CopyTo(output);
            }
        }

        public static Stream GetAppStream(bool decompress = false)
        {
            if (!decompress)
            {
                return Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.index.full.html.gz");
            }
            else
            {
                return new GZipStream(Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.index.full.html.gz"), CompressionMode.Decompress);
            }
        }

        public static void WriteFlotAppAsync(Stream output, bool decompress = false)
        {
            if (!decompress)
            {
                using (var stream = Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.index.full.html.gz"))
                {
                    stream.CopyTo(output);
                }
            }
            else
            {
                using (var stream = Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.index.full.html.gz"))
                using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                {
                    gzip.CopyTo(output);
                }
            }
        }
    }
}
