using System;
using NuGet;

namespace Solutionizer.Update {
    public class UpdateInfo {
        private readonly IPackage _package;

        public UpdateInfo(IPackage package) {
            _package = package;
        }

        public SemanticVersion Version {
            get { return _package.Version; }
        }

        public DateTimeOffset? Published {
            get { return _package.Published; }
        }

        public string ReleaseNotes {
            get { return _package.ReleaseNotes; }
        }

        public bool IsReleaseVersion {
            get { return _package.IsReleaseVersion(); }
        }
    }
}