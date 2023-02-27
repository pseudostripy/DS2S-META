using DS2S_META.Properties;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public enum UPDATE_STATUS
    {
        UPTODATE,       // matches latest git release
        OUTOFDATE,      // behind latest git release
        INDEVELOPMENT,  // ahead of latest git release
        UNKNOWN_VER,    // cannot check assembly ver
        UNCHECKABLE,    // cannot find latest git release
    };

    public class MetaVersionInfo
    {
        public Version? GitVersion { get; set; }
        public Version? ExeVersion { get; set; }
        public Release? LatestRelease { get; set; }
        public Uri? LatestReleaseURI { get; set; }
        public string MetaVersionStr => ExeVersion == null ? "Version Undefined" : ExeVersion.ToString();
        public string GitVersionStr => GitVersion == null ? string.Empty : GitVersion.ToString();
        
        public bool IsAcknowledged => Settings.Default.AcknowledgeUpdateVersion == GitVersionStr;

        public UPDATE_STATUS UpdateStatus { get; set; }
        public UPDATE_STATUS SyncUpdateStatus()
        {
            if (GitVersion == null)
                return UPDATE_STATUS.UNCHECKABLE;

            if (GitVersion > ExeVersion)
                return UPDATE_STATUS.OUTOFDATE;

            if (GitVersion == ExeVersion)
                return UPDATE_STATUS.UPTODATE;

            if (GitVersion < ExeVersion)
                return UPDATE_STATUS.INDEVELOPMENT;

            return UPDATE_STATUS.UNKNOWN_VER;
        }
    }
}
