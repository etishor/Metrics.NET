using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

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

        public const string FavIconMimeType = "image/png";

        public static async Task WriteFavIcon(Stream output)
        {
            using (var stream = Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.metrics_32.png"))
            {
                await stream.CopyToAsync(output);
            }
        }

        public static async Task WriteFlotApp(Stream output, bool decompress = false)
        {
            if (!decompress)
            {
                using (var stream = Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.index.full.html.gz"))
                {
                    await stream.CopyToAsync(output).ConfigureAwait(false);
                }
            }
            else
            {
                using (var stream = Assembly.GetAssembly(typeof(FlotWebApp)).GetManifestResourceStream("Metrics.Visualization.index.full.html.gz"))
                using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                {
                    await gzip.CopyToAsync(output).ConfigureAwait(false);
                }
            }
        }
    }
}
