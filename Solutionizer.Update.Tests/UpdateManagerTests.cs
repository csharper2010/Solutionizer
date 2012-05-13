using System;
using System.IO;
using NUnit.Framework;
using Solutionizer.Update.FeedProvider;

namespace Solutionizer.Update.Tests {
    [TestFixture]
    public class UpdateManagerTests {
        [Test]
        public void FindOneUpdated() {
            var rssUpdateFeedProvider = new RssUpdateFeedProvider {
                Uri = new Uri(Path.GetDirectoryName(GetType().Assembly.CodeBase) + "/fixtures/updatefeed.xml")
            };

            var updateManager = new UpdateManager();
            updateManager.UpdateFeedProvider = rssUpdateFeedProvider;
            updateManager.CurrentVersion = new SemanticVersion(1, 0, 0, 0);

            var result = updateManager.GetUpdateInfosSinceCurrentVersion().Result;
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void FindNoUpdatedIfUpToDate() {
            var rssUpdateFeedProvider = new RssUpdateFeedProvider {
                Uri = new Uri(Path.GetDirectoryName(GetType().Assembly.CodeBase) + "/fixtures/updatefeed.xml")
            };

            var updateManager = new UpdateManager();
            updateManager.UpdateFeedProvider = rssUpdateFeedProvider;
            updateManager.CurrentVersion = new SemanticVersion(2, 0, 0, 0);

            var result = updateManager.GetUpdateInfosSinceCurrentVersion().Result;
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void CanDownloadUpdate() {
            var uri = new Uri(Path.GetDirectoryName(GetType().Assembly.CodeBase) + "/fixtures/v1.2.3.4.zip");
            var targetPath = Path.GetTempFileName();

            var updateManager = new UpdateManager();
            try {
                updateManager.DownloadUpdate(new UpdateInfo { DownloadUri = uri }, targetPath).Wait();

                Assert.IsTrue(File.Exists(targetPath));
            } finally {
                File.Delete(targetPath);
            }
        }
    }
}