using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KeePassIconDownloader {

    internal readonly struct Provider(string name, string url) {
        public readonly string Name { get; } = name;
        public readonly string Url { get; } = url;
    }

    internal static class IconDownloader {
        public static readonly List<Provider> Providers = [];

        static IconDownloader() {
            Providers.Add(new Provider("None", string.Empty));
            Providers.Add(new Provider("Favicon Kit", "https://ico.faviconkit.net/favicon/{ENTRYURL}?sz={ICONSIZE}"));
            Providers.Add(new Provider("Google", "https://www.google.com/s2/favicons?domain={ENTRYURL}&size={ICONSIZE}"));
            Providers.Add(new Provider("DuckDuckGo", "https://icons.duckduckgo.com/ip3/{ENTRYURL}.ico"));
            Providers.Add(new Provider("Yandex", "https://favicon.yandex.net/favicon/{ENTRYURL}"));
        }

        internal static Provider GetProvider(string name) {
            return Providers.Find(p => p.Name == name);
        }

        /// <summary>
        /// Fetch a favicon of a single size using the specified provider.
        /// </summary>
        /// <param name="provider">Service to use to fetch icons.</param>
        /// <param name="entryUrl">Website to fetch the icon from.</param>
        /// <param name="size">Icon size in pixels.</param>
        /// <returns>A favicon of the specified size, or <see langword="null"/> if one could not be fetched.</returns>
        private static async Task<Image?> FetchFaviconPngAsync(Provider provider, string entryUrl, int size = 128) {
            if (provider.Url == string.Empty) return null;

            var fetchUrl = provider.Url.Replace("{ENTRYURL}", Uri.EscapeDataString(entryUrl)).Replace("{ICONSIZE}", size.ToString());

            try {
                using var client = new WebClient();
                client.Headers["User-Agent"] = "Mozilla/5.0";
                var data = await client.DownloadDataTaskAsync(fetchUrl);
                using var stream = new MemoryStream(data);
                return Image.FromStream(stream);
            }
            catch {
                return null;
            }
        }

        //private static async Task<Dictionary<int, Image?>> FetchFaviconIcoAsync(Provider provider, string entryUrl) {
        //    if (provider.Url == string.Empty) return [];

        //    var fetchUrl = provider.Url.Replace("{ENTRYURL}", Uri.EscapeDataString(entryUrl)).Replace("{ICONSIZE}", "128");
        //    try {
        //        using var client = new WebClient();
        //        client.Headers["User-Agent"] = "Mozilla/5.0";
        //        client.Headers["Accept"] = "image/x-icon";
        //        var data = await client.DownloadDataTaskAsync(fetchUrl);

        //        return new Dictionary<int, Image?> {
        //            { 128, ExtractIconSize(data, 128) },
        //            { 64,  ExtractIconSize(data, 64)  },
        //            { 32,  ExtractIconSize(data, 32)  },
        //            { 16,  ExtractIconSize(data, 16)  }
        //        };
        //    }
        //    catch (Exception) {
        //        return new Dictionary<int, Image?> {
        //            { 128, null },
        //            { 64, null },
        //            { 32, null },
        //            { 16, null }
        //        };
        //    }
        //}
        //private static Bitmap? ExtractIconSize(byte[] data, int size) {
        //    try {
        //        using var stream = new MemoryStream(data);
        //        using var icon = new Icon(stream, new Size(size, size));
        //        return icon.ToBitmap();
        //    }
        //    catch {
        //        return null;
        //    }
        //}

        /// <summary>
        /// Fetches four icons for the specified entryUrl concurrently from the indicated provider.
        /// </summary>
        /// <param name="provider">Service to use to fetch icons.</param>
        /// <param name="entryUrl">Website to fetch the icon from.</param>
        /// <returns>A dictionary with four sizes of favicons: 128px, 64px, 32px, 16px.</returns>
        internal static async Task<Dictionary<int, Image?>> FetchFaviconsAsync(Provider provider, string entryUrl) {
            if (false) {
                //return await FetchFaviconIcoAsync(provider, entryUrl);
            } else {
                var task128 = FetchFaviconPngAsync(provider, entryUrl, 128);
                var task64 = FetchFaviconPngAsync(provider, entryUrl, 64);
                var task32 = FetchFaviconPngAsync(provider, entryUrl, 32);
                var task16 = FetchFaviconPngAsync(provider, entryUrl, 16);
                await Task.WhenAll(task128, task64, task32, task16);

                return new Dictionary<int, Image?> {
                    { 128, task128.Result },
                    { 64, task64.Result },
                    { 32, task32.Result },
                    { 16, task16.Result }
                };
            }
        }
    }
}
