using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public static class LogCleaner
    {
        private static readonly string NL = Environment.NewLine;
        public static string RemoveBuildPaths(string logMessage)
        {
            // clean out build paths dynamically (has issues):
            //string currdir = Directory.GetCurrentDirectory();
            //int first = currdir.IndexOf("\\bin");
            //string buildDirInnerProj = currdir.Substring(0, first);
            //var tempBuildDir = Directory.GetParent(buildDirInnerProj);
            //string buildDir = tempBuildDir == null ? string.Empty : tempBuildDir.ToString();
            //return logMessage.Replace(buildDir, "MetaSolution");

            string pattern = @"\S+?\\META";
            string replacement = "MetaSolution";
            return Regex.Replace(logMessage, pattern, replacement);
        }
        public static string ToGlobalExLogString(this Exception e)
        {
            // If exception is globally caught then the stack trace doesn't need fixing
            return $"{NL}{e?.Message}{NL}{NL}{e?.Message}{NL}{e?.StackTrace}";
        }
    }
}
