using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Solutionizer.Update.FeedProvider;

namespace Solutionizer.Update {
    public class UpdateManager {
        public IUpdateFeedProvider UpdateFeedProvider { get; set; }
        public SemanticVersion CurrentVersion { get; set; }

        public Task<List<UpdateInfo>> GetUpdateInfosSinceCurrentVersion() {
            var tcs = new TaskCompletionSource<List<UpdateInfo>>();

            tcs.SetResult(new List<UpdateInfo> {
                new UpdateInfo {
                    DownloadUri = new Uri("file:///update.exe"),
                    ReleaseDate = DateTimeOffset.Now,
                    ReleaseNotes = "new release v1.1",
                    Version = new SemanticVersion("1.1")
                }
            });
            
            return tcs.Task;
        }
    }
}