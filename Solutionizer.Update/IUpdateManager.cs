using System;
using System.Threading.Tasks;

namespace Solutionizer.Update {
    public interface IUpdateManager {
        Task<UpdateInfo> CheckForUpdate(Version currentVersion = null, bool includePrereleases = false);

        Task DownloadPackage(UpdateInfo updateInfo);
    }
}