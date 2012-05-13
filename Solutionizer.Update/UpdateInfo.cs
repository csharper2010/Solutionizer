using System;

namespace Solutionizer.Update {
    public class UpdateInfo {
        public Uri DownloadUri { get; set; }
        public SemanticVersion Version { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }
        public string ReleaseNotes { get; set; }
    }
}