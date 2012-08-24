using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NuGet;

namespace Solutionizer.Update {
    public class UpdateManager {
        private const string REPOSITORY_URL = "http://www.myget.org/F/solutionizer/api/v2/";
        const string PACKAGE_ID = "Solutionizer";

        private readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        private IPackage _latestPackage;
        private List<UpdateInfo> _availableUpdates = new List<UpdateInfo>();

        public Task CheckForUpdate (Version currentVersion) {
            return Task.Factory.StartNew(() => {
                var repositoryFactory = PackageRepositoryFactory.Default.CreateRepository(REPOSITORY_URL);

                var progressProvider = repositoryFactory as IProgressProvider;
                if (progressProvider != null) {
                    progressProvider.ProgressAvailable += (sender, args) => _logger.Info("{0}: {1}", args.Operation, args.PercentComplete);
                }

                var httpClientEvents = repositoryFactory as IHttpClientEvents;
                if (httpClientEvents != null) {
                    httpClientEvents.SendingRequest += (sender, args) => _logger.Info("requesting {0}", args.Request.RequestUri);
                }

                var versionSpec = currentVersion != null 
                    ? new VersionSpec { MinVersion = new SemanticVersion(currentVersion),  IsMinInclusive = false }
                    : null;
                var packages = repositoryFactory.FindPackages(PACKAGE_ID, versionSpec, true, true).ToArray();
                _latestPackage = packages.SingleOrDefault(p => p.IsAbsoluteLatestVersion);

                _availableUpdates = packages.Select(p => new UpdateInfo {
                    Version = p.Version.Version,
                    ReleaseDate = p.Published,
                    ReleaseNotes = p.ReleaseNotes,
                    IsReleaseVersion = p.IsReleaseVersion()
                }).ToList();
            });
        }

        //public void 
    }

    public class UpdateInfo {
        public Version Version { get; set; }
        public DateTimeOffset? ReleaseDate { get; set; }
        public string ReleaseNotes { get; set; }
        public bool IsReleaseVersion { get; set; }
    }
}