using System;
using System.Threading.Tasks;

namespace Solutionizer.Update {
    public interface IUpdateManager {
        Task CheckForUpdate(Version currentVersion = null, bool includePrereleases = false);
    }
}