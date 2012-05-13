using System;
using System.IO;
using NUnit.Framework;
using Solutionizer.Update.FeedProvider;

namespace Solutionizer.Update.Tests {
    [TestFixture]
    public class RssUpdateFeedProviderTest {
        [Test]
        public void CanParseFeed() {
            var uri = new Uri(Path.GetDirectoryName(GetType().Assembly.CodeBase) + "/fixtures/updatefeed.xml");

            var provider = new RssUpdateFeedProvider();
            provider.Uri = uri;

            var result = provider.GetUpdateInfos().Result;

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("1.2.3.4", result[0].Version.ToString());
            Assert.AreEqual("This is Version 1.2.3.4", result[0].ReleaseNotes);
            Assert.AreEqual(new Uri(Path.GetDirectoryName(GetType().Assembly.CodeBase) + "/fixtures/v1.2.3.4.zip"), result[0].DownloadUri);
        }
    }
}