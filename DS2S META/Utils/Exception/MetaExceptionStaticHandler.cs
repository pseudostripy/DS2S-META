﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

using System.IO;

namespace DS2S_META.Utils
{


    /// <summary>
    ///  Extension methods for Exception class.
    ///  Taken from https://www.neolisk.blog/posts/2015-08-13-csharp-capture-full-stacktrace/ 
    /// </summary>
    static class MetaExceptionStaticHandler
    {
        /// <summary>
        ///  Provides full stack trace for the exception that occurred.
        ///  Usually when a catch statement traps an error, you only
        ///  get the stack trace frame upto the try-catch function.
        ///  This implementation should allow the full track trace for 
        ///  extra debug information.
        /// </summary>
        /// <param name="exception">Exception object.</param>
        /// <param name="environmentStackTrace">Environment stack trace, for pulling additional stack frames.</param>
        public static string ToLogString(this Exception exception, string environmentStackTrace)
        {
            String logMessage;

            List<string> environmentStackTraceLines = GetUserStackTraceLines(environmentStackTrace);
            
            // BugFix in case there's further changes to stack trace internal things
            // This was likely caused by the .net8 switch.
            if (environmentStackTraceLines.Count == 0)
            {
                logMessage = exception?.Message + Environment.NewLine +
                    "Please run META in debug mode for full error stack trace.";
            }
            else
            {
                environmentStackTraceLines.RemoveAt(0);

                List<string> stackTraceLines = GetStackTraceLines(exception?.StackTrace);
                stackTraceLines.AddRange(environmentStackTraceLines);

                string fullStackTrace = string.Join(Environment.NewLine, stackTraceLines);

                logMessage = exception?.Message + Environment.NewLine + fullStackTrace;
            }
            return RemoveBuildPaths(logMessage);
        }

        /// <summary>
        ///  Gets a list of stack frame lines, as strings.
        /// </summary>
        /// <param name="stackTrace">Stack trace string.</param>
        private static List<string> GetStackTraceLines(string? stackTrace)
        {
            if (stackTrace == null)
                return new();
            return stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }

        /// <summary>
        ///  Gets a list of stack frame lines, as strings, only including those for which line number is known.
        /// </summary>
        /// <param name="fullStackTrace">Full stack trace, including external code.</param>
        private static List<string> GetUserStackTraceLines(string fullStackTrace)
        {
            List<string> outputList = new();
            Regex regex = new(@"([^\)]*\)) in (.*):line (\d)*$");

            List<string> stackTraceLines = GetStackTraceLines(fullStackTrace);
            foreach (string stackTraceLine in stackTraceLines)
            {
                if (!regex.IsMatch(stackTraceLine))
                {
                    continue;
                }

                outputList.Add(stackTraceLine);
            }

            return outputList;
        }
        private static void ShowMetaExceptionWindow(string logmsg)
        {
            var meWindow = new MetaExceptionWindow(logmsg); // notify user on screen
            meWindow.ShowDialog();
        }
        private static void ShowMetaExceptionWindowWithTitle(string title, string logmsg)
        {
            var meWindow = new MetaExceptionWindow(title, logmsg); // notify user on screen
            meWindow.ShowDialog();
        }
        private static void ShowMetaWarningWindow(string warnmsg) => MetaWarningWindow.ShowMetaWarning(warnmsg);
        public static void Handle(Exception e)
        {
            // Used to cleanup exception message cleanup before passing into Dispatcher
            // thread for handling UI error messaging
            ((App)Application.Current).LogCaughtException(e);   // dump to log file

            // Invoke on Dispatcher thread to avoid STA errors
            var logmsg = e.ToLogString(Environment.StackTrace); // get clean stack trace

            // Add more granularity or tidy as appropriate
            if (e is MetaException me)
                Application.Current.Dispatcher.Invoke(() => ShowMetaExceptionWindowWithTitle(me.Etype, logmsg));
            else
                Application.Current.Dispatcher.Invoke(() => ShowMetaExceptionWindow(logmsg));
        }
        public static void Raise(string message)
        {
            var e = new Exception(message);
            Handle(e);
        }
        public static void RaiseUserWarning(string msg)
        {
            Application.Current.Dispatcher.Invoke(() => ShowMetaWarningWindow(msg));
        }
        public static void RaiseInfo(string msg)
        {
            MetaInfoWindow.ShowMetaInfo(msg);
        }

        // Log Cleaner
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