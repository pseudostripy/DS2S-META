using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
///  Extension methods for Exception class.
///  Taken from https://www.neolisk.blog/posts/2015-08-13-csharp-capture-full-stacktrace/ 
/// </summary>
static class ExceptionExtensions
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
        List<string> environmentStackTraceLines = GetUserStackTraceLines(environmentStackTrace);
        environmentStackTraceLines.RemoveAt(0);

        List<string> stackTraceLines = GetStackTraceLines(exception?.StackTrace);
        stackTraceLines.AddRange(environmentStackTraceLines);

        string fullStackTrace = string.Join(Environment.NewLine, stackTraceLines);

        string logMessage = exception?.Message + Environment.NewLine + fullStackTrace;
        return LogCleaner.RemoveBuildPaths(logMessage);
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


}