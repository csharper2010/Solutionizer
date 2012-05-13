using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Solutionizer.Update {
    /// <summary>
    /// A hybrid implementation of SemVer that supports semantic versioning as described at http://semver.org while not strictly enforcing it to 
    /// allow older 4-digit versioning schemes to continue working.
    /// </summary>
    /// <remarks>
    /// Taken from NuGet sources at https://nuget.codeplex.com/SourceControl/changeset/view/f13200b00e34#src%2fCore%2fSemanticVersion.cs
    /// </remarks>
    [Serializable]
    [TypeConverter(typeof(SemanticVersionTypeConverter))]
    public sealed class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion> {
        private const RegexOptions _flags = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        private static readonly Regex _semanticVersionRegex = new Regex(@"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$", _flags);
        private static readonly Regex _strictSemanticVersionRegex = new Regex(@"^(?<Version>\d+(\.\d+){2})(?<Release>-[a-z][0-9a-z-]*)?$", _flags);
        private readonly string _originalString;

        public SemanticVersion(string version)
            : this(Parse(version)) {
            // The constructor normalizes the version string so that it we do not need to normalize it every time we need to operate on it. 
            // The original string represents the original form in which the version is represented to be used when printing.
            _originalString = version;
        }

        public SemanticVersion(int major, int minor, int build, int revision)
            : this(new Version(major, minor, build, revision)) {
        }

        public SemanticVersion(int major, int minor, int build, string specialVersion)
            : this(new Version(major, minor, build), specialVersion) {
        }

        public SemanticVersion(Version version)
            : this(version, String.Empty) {
        }

        public SemanticVersion(Version version, string specialVersion)
            : this(version, specialVersion, null) {
        }

        private SemanticVersion(Version version, string specialVersion, string originalString) {
            if (version == null) {
                throw new ArgumentNullException("version");
            }
            Version = NormalizeVersionValue(version);
            SpecialVersion = specialVersion ?? String.Empty;
            _originalString = String.IsNullOrEmpty(originalString) ? version + (!String.IsNullOrEmpty(specialVersion) ? '-' + specialVersion : null) : originalString;
        }

        internal SemanticVersion(SemanticVersion semVer) {
            _originalString = semVer.ToString();
            Version = semVer.Version;
            SpecialVersion = semVer.SpecialVersion;
        }

        /// <summary>
        /// Gets the normalized version portion.
        /// </summary>
        public Version Version {
            get;
            private set;
        }

        /// <summary>
        /// Gets the optional special version.
        /// </summary>
        public string SpecialVersion {
            get;
            private set;
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an optional special version.
        /// </summary>
        public static SemanticVersion Parse(string version) {
            if (String.IsNullOrEmpty(version)) {
                throw new ArgumentException("Value cannot be null or an empty string.", "version");
            }

            SemanticVersion semVer;
            if (!TryParse(version, out semVer)) {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "'{0}' is not a valid version string.", version), "version");
            }
            return semVer;
        }

        /// <summary>
        /// Parses a version string using loose semantic versioning rules that allows 2-4 version components followed by an optional special version.
        /// </summary>
        public static bool TryParse(string version, out SemanticVersion value) {
            return TryParseInternal(version, _semanticVersionRegex, out value);
        }

        /// <summary>
        /// Parses a version string using strict semantic versioning rules that allows exactly 3 components and an optional special version.
        /// </summary>
        public static bool TryParseStrict(string version, out SemanticVersion value) {
            return TryParseInternal(version, _strictSemanticVersionRegex, out value);
        }

        private static bool TryParseInternal(string version, Regex regex, out SemanticVersion semVer) {
            semVer = null;
            if (String.IsNullOrEmpty(version)) {
                return false;
            }

            var match = regex.Match(version.Trim());
            Version versionValue;
            if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out versionValue)) {
                return false;
            }

            semVer = new SemanticVersion(NormalizeVersionValue(versionValue), match.Groups["Release"].Value.TrimStart('-'), version.Replace(" ", ""));
            return true;
        }

        /// <summary>
        /// Attempts to parse the version token as a SemanticVersion.
        /// </summary>
        /// <returns>An instance of SemanticVersion if it parses correctly, null otherwise.</returns>
        public static SemanticVersion ParseOptionalVersion(string version) {
            SemanticVersion semVer;
            TryParse(version, out semVer);
            return semVer;
        }

        private static Version NormalizeVersionValue(Version version) {
            return new Version(version.Major,
                               version.Minor,
                               Math.Max(version.Build, 0),
                               Math.Max(version.Revision, 0));
        }

        public int CompareTo(object obj) {
            if (ReferenceEquals(obj, null)) {
                return 1;
            }
            var other = obj as SemanticVersion;
            if (other == null) {
                throw new ArgumentException("Type to compare must be an instance of SemanticVersion.", "obj");
            }
            return CompareTo(other);
        }

        public int CompareTo(SemanticVersion other) {
            if (ReferenceEquals(other, null)) {
                return 1;
            }

            int result = Version.CompareTo(other.Version);

            if (result != 0) {
                return result;
            }

            bool empty = String.IsNullOrEmpty(SpecialVersion);
            bool otherEmpty = String.IsNullOrEmpty(other.SpecialVersion);
            if (empty && otherEmpty) {
                return 0;
            } else if (empty) {
                return 1;
            } else if (otherEmpty) {
                return -1;
            }
            return StringComparer.OrdinalIgnoreCase.Compare(SpecialVersion, other.SpecialVersion);
        }

        public static bool operator ==(SemanticVersion version1, SemanticVersion version2) {
            if (ReferenceEquals(version1, null)) {
                return ReferenceEquals(version2, null);
            }
            return version1.Equals(version2);
        }

        public static bool operator !=(SemanticVersion version1, SemanticVersion version2) {
            return !(version1 == version2);
        }

        public static bool operator <(SemanticVersion version1, SemanticVersion version2) {
            if (version1 == null) {
                throw new ArgumentNullException("version1");
            }
            return version1.CompareTo(version2) < 0;
        }

        public static bool operator <=(SemanticVersion version1, SemanticVersion version2) {
            return (version1 == version2) || (version1 < version2);
        }

        public static bool operator >(SemanticVersion version1, SemanticVersion version2) {
            if (version1 == null) {
                throw new ArgumentNullException("version1");
            }
            return version2 < version1;
        }

        public static bool operator >=(SemanticVersion version1, SemanticVersion version2) {
            return (version1 == version2) || (version1 > version2);
        }

        public override string ToString() {
            return _originalString;
        }

        public bool Equals(SemanticVersion other) {
            return !ReferenceEquals(null, other) &&
                   Version.Equals(other.Version) &&
                   SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj) {
            var semVer = obj as SemanticVersion;
            return !ReferenceEquals(null, semVer) && Equals(semVer);
        }

        public override int GetHashCode() {
            return Version.GetHashCode() ^ SpecialVersion.GetHashCode();
        }

        public class SemanticVersionTypeConverter : TypeConverter {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
                return sourceType == typeof(string);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                var stringValue = value as string;
                SemanticVersion semVer;
                if (stringValue != null && TryParse(stringValue, out semVer)) {
                    return semVer;
                }
                return null;
            }
        }
    }
}