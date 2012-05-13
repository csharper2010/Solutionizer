using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Solutionizer.Update.FeedProvider {
    public class RssUpdateFeedProvider : IUpdateFeedProvider {
        public Uri Uri { get; set; }

        public Task<List<UpdateInfo>> GetUpdateInfos() {
            if (Uri == null) {
                throw new InvalidOperationException("Uri was not set.");
            }

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
                // no title
                return null;
            }
            SemanticVersion version;
            var match = Regex.Match(element.Value, @"\d+(\s*\.\s*\d+){0,3}(-[a-z][0-9a-z-]*)?");
            if (!match.Success || !SemanticVersion.TryParse(match.Value, out version)) {
                // no version found
                return null;
            }

            DateTimeOffset releaseDate;
            element = itemElement.Element("pubDate");
            if (element == null || String.IsNullOrWhiteSpace(element.Value) ||
                !DateTimeOffset.TryParse(element.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate)) {
                // no pubDate
                return null;
            }

            element = itemElement.Element("description");
            if (element == null || String.IsNullOrWhiteSpace(element.Value)) {
                // no description
                return null;
            }
            var releaseNotes = element.Value;

            Uri downloadUri;
            element = itemElement.Element("enclosure");
            if (element == null) {
                // no enclosure
                return null;
            }
            var urlAttribute = element.Attribute("url");
            if (urlAttribute == null || String.IsNullOrWhiteSpace(urlAttribute.Value) || !Uri.TryCreate(urlAttribute.Value, UriKind.RelativeOrAbsolute, out downloadUri)) {
                // invalid enclosure
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