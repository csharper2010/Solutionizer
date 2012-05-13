using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Solutionizer.Update.FeedProvider {
    public class RssUpdateFeedProvider : IUpdateFeedProvider {
        public IWebProxy Proxy { get; set; }
        public Uri Uri { get; set; }

        public Task<List<UpdateInfo>> GetUpdateInfos() {
            var tcs = new TaskCompletionSource<List<UpdateInfo>>();

            var wc = new WebClient { Proxy = Proxy };
            wc.DownloadStringCompleted += (sender, args) => {
                if (args.Error != null) {
                    tcs.SetException(args.Error);
                } else if (args.Cancelled) {
                    tcs.SetCanceled();
                } else {
                    tcs.SetResult(ParseFeed(args.Result));
                }
            };

            wc.DownloadStringAsync(Uri);

            return tcs.Task;
        }

        private List<UpdateInfo> ParseFeed(string content) {
            return XDocument
                .Parse(content)
                .Descendants("item")
                .Select(GetUpdateInfoFromXElement)
                .Where(x => x != null)
                .ToList();
        }

        private UpdateInfo GetUpdateInfoFromXElement(XElement itemElement) {
            SemanticVersion version;
            var element = itemElement.Element("title");
            if (element == null || String.IsNullOrWhiteSpace(element.Value) || !SemanticVersion.TryParse(element.Value, out version)) {
                // invalid version
                return null;
            }

            DateTimeOffset releaseDate;
            element = itemElement.Element("pubDate");
            if (element == null || String.IsNullOrWhiteSpace(element.Value) ||
                !DateTimeOffset.TryParse(element.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate)) {
                // invalid version
                return null;
            }

            element = itemElement.Element("description");
            if (element == null || String.IsNullOrWhiteSpace(element.Value)) {
                // invalid version
                return null;
            }
            var releaseNotes = element.Value;

            Uri downloadUri;
            element = itemElement.Element("enclosure");
            if (element == null || element.Attribute("url") == null || String.IsNullOrWhiteSpace(element.Attribute("url").Value) ||
                !Uri.TryCreate(element.Attribute("url").Value, UriKind.RelativeOrAbsolute, out downloadUri)) {
                // invalid version
                return null;
            }
            if (!downloadUri.IsAbsoluteUri) {
                downloadUri = new Uri(Uri, downloadUri);
            }

            return new UpdateInfo {
                Version = version,
                ReleaseDate = releaseDate,
                ReleaseNotes = releaseNotes,
                DownloadUri = downloadUri
            };
        }
    }
}