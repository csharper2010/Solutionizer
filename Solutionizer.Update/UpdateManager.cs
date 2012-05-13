using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Solutionizer.Update.FeedProvider;

namespace Solutionizer.Update {
    public class UpdateManager {
        public IUpdateFeedProvider UpdateFeedProvider { get; set; }
        public SemanticVersion CurrentVersion { get; set; }

        public Task<List<UpdateInfo>> GetUpdateInfosSinceCurrentVersion() {
            return UpdateFeedProvider
                .GetUpdateInfos()
                .ContinueWith(t => t.Result.Where(updateInfo => updateInfo.Version > CurrentVersion).ToList());
        }

        public Task DownloadUpdate(UpdateInfo info, string targetPath) {
            var tcs = new TaskCompletionSource<string>();

            var wc = new WebClient();
            wc.DownloadFileCompleted += (sender, args) => {
                if (args.Error != null) {
                    tcs.SetException(args.Error);
                } else if (args.Cancelled) {
                    tcs.SetCanceled();
                } else {
                    tcs.SetResult(targetPath);
                }
            };
            wc.DownloadFileAsync(info.DownloadUri, targetPath);

            return tcs.Task;
        }
    }
}