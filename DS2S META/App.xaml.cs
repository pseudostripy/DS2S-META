using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


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
                LogGlobalException(exception);
            } 
            //This catch hides an exception, but can't really help it at this point.
            catch { }
        }


        private void WpfExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                LogGlobalException(e.Exception);
            }
            //This catch hides an exception, but can't really help it at this point.
            catch{ }

            e.Handled = true;//If we don't set this event as "handled", the application will crash.
        }



        private readonly object _logFileLock = new();
        private void LogGlobalException(Exception exception)
        {
            var logMessage = exception.ToGlobalExLogString(); // clean build paths
            WriteToLog(logMessage);
        }
        public void LogCaughtException(Exception e) => WriteToLog(e.ToLogString(Environment.StackTrace));
        public void LogCaughtExceptionMessage(string msg) => WriteToLog(msg);
        private static readonly string NL = Environment.NewLine;
        private void WriteToLog(string message)
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

                //Log the error
                var error = $"[{DateTime.Now}]{NL}{message}{NL}{NL}";
                File.AppendAllText(logFile, error);
            }
        }
    }
}
