using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NuGet;

namespace Solutionizer.Update {
    public class UpdateManager : IUpdateManager {
        private const string REPOSITORY_URL = "http://www.myget.org/F/solutionizer/api/v2/";
        const string PACKAGE_ID = "Solutionizer";

        private readonly static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IPackageRepository _packageRepository;

        private IPackage _latestPackage;
        private List<UpdateInfo> _availableUpdates = new List<UpdateInfo>();

        public UpdateManager(IPackageRepository packageRepository = null) {
            _packageRepository = packageRepository ?? PackageRepositoryFactory.Default.CreateRepository(REPOSITORY_URL);

            var progressProvider = _packageRepository as IProgressProvider;
            if (progressProvider != null) {
                progressProvider.ProgressAvailable += (sender, args) => _logger.Info("{0}: {1}", args.Operation, args.PercentComplete);
            }

            var httpClientEvents = _packageRepository as IHttpClientEvents;
            if (httpClientEvents != null) {
                httpClientEvents.SendingRequest += (sender, args) => _logger.Info("requesting {0}", args.Request.RequestUri);
            }
        }

        public Task CheckForUpdate (Version currentVersion = null, bool includePrereleases = false) {
            return Task.Factory.StartNew(() => {
                var versionSpec = currentVersion != null 
                    ? new VersionSpec { MinVersion = new SemanticVersion(currentVersion),  IsMinInclusive = false }
                    : null;
                var packages = _packageRepository.FindPackages(PACKAGE_ID, versionSpec, includePrereleases, true).ToArray();
                _latestPackage = packages.SingleOrDefault(p => includePrereleases ? p.IsAbsoluteLatestVersion : p.IsLatestVersion)
                    ?? packages.OrderByDescending(p => p.Version).FirstOrDefault();

                _availableUpdates = packages.Select(p => new UpdateInfo(p)).ToList();
                RaiseAvailableUpdatesChanged();
            });
        }

        public IEnumerable<UpdateInfo> AvailableUpdates {
            get { return _availableUpdates; }
        }

        public event EventHandler AvailableUpdatesChanged;

        private void RaiseAvailableUpdatesChanged() {
            var handler = AvailableUpdatesChanged;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }
    }
}