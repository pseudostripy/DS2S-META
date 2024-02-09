using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //Global
            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
            
            //WPF specific - setting this event as handled can prevent crashes
            Dispatcher.UnhandledException += WpfExceptionHandler;
        }

        

        void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = (Exception)e.ExceptionObject;
                LogException(exception);
            } 
            //This catch hides an exception, but can't really help it at this point.
            catch { }
        }


        private void WpfExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                LogException(e.Exception);
            }
            //This catch hides an exception, but can't really help it at this point.
            catch{ }

            e.Handled = true;//If we don't set this event as "handled", the application will crash.
        }



        private readonly object _logFileLock = new object();
        private string NL = Environment.NewLine;
        private void LogException(Exception exception)
        {
            lock (_logFileLock)
            {
                var logFile = Environment.CurrentDirectory + @"\log.txt";

                //Log retention: at most 2 days. Can up this, but don't want to risk creating a 10GB log file when shit goes wrong.
                //Or when it is never cleared. Use NLog? 
                var createDate = File.GetCreationTime(logFile);
                var clearDate = createDate.AddDays(2);
                if (DateTime.Now > clearDate)
                {
                    File.Delete(logFile);
                }

                // clean out build paths:
                string pattern = @"\S+?(?=\\META)"; // find all no-space paths ending in \META
                string replacement = "METAPARENTDIR";
                string cleanStackTrace;
                if (exception?.StackTrace == null)
                    cleanStackTrace = "NULL STACK TRACE!";
                else
                    cleanStackTrace = Regex.Replace(exception.StackTrace, pattern, replacement);
                
                //Log the error
                var error = $"[{DateTime.Now}]{NL}{exception?.Message}{NL}{NL}{cleanStackTrace}{NL}";
                File.AppendAllText(logFile, error);
            }
        }
    }
}
