using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.Visualization
{
    public static class FlotWebApp
    {
        private static readonly Assembly thisAssembly = Assembly.GetAssembly(typeof(FlotWebApp));
        private const string FlotAppResource = "Metrics.Visualization.index.full.html.gz";
        private const string FavIconResource = "Metrics.Visualization.metrics_32.png";

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

        public static async Task WriteFavIcon(Stream output,CancellationToken token)
        {
            using (var stream = thisAssembly.GetManifestResourceStream(FavIconResource))
            {
                Debug.Assert(stream != null, "Unable to read embeded flot app");
                await stream.CopyToAsync(output, (int) stream.Length, token).ConfigureAwait(false);
            }
        }

        public static Stream GetAppStream(bool decompress = false)
        {
            var stream = !decompress ? thisAssembly.GetManifestResourceStream(FlotAppResource) : new GZipStream(GetAppStream(), CompressionMode.Decompress, false);
            Debug.Assert(stream != null, "Unable to read embeded flot app");
            return stream;
        }

        public static async Task WriteFlotAppAsync(Stream output, CancellationToken token, bool decompress = false)
        {
            using (var stream = GetAppStream(decompress))
            {
                await stream.CopyToAsync(output, 81920, token).ConfigureAwait(false);
            }
        }
    }
}
