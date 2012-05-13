using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Solutionizer.Update.FeedProvider {
    public class RssUpdateFeedProvider : IUpdateFeedProvider {
        public Uri Uri { get; set; }

        public Task<List<UpdateInfo>> GetUpdateInfos() {
            return WebClientHelper
                .DownloadString(Uri)
                .ContinueWith(t => ParseFeed(t.Result));
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
            var element = itemElement.Element("title");
            if (element == null || String.IsNullOrWhiteSpace(element.Value)) {
                // invalid version
                return null;
            }
            SemanticVersion version;
            var match = Regex.Match(element.Value, @"\d+(\s*\.\s*\d+){0,3}(-[a-z][0-9a-z-]*)?");
            if (!match.Success || !SemanticVersion.TryParse(match.Value, out version)) {
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