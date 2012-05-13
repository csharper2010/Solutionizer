using System.Linq;
using System.Collections.Generic;
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
            return WebClientHelper.Download(info.DownloadUri, targetPath);
        }
    }
}