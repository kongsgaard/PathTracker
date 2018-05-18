using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using log4net.Core;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using log4net.Config;
using log4net.Layout;
using log4net.Repository;
using System.Reflection;
using System.IO;
using System.Threading;

namespace PathTracker_Backend
{
    public static class LogCreator
    {
        private static string logDir = Directory.GetCurrentDirectory() + "\\Logs\\";

        public static void Setup() {
            ILoggerRepository repository = LogManager.CreateRepository("All");
        }

        public static ILog CreateLog(string logName) {

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            //RollingFileAppender roller = new RollingFileAppender {
            //    AppendToFile = true,
            //    File = logDir + logName + ".txt",
            //    Layout = patternLayout,
            //    MaxSizeRollBackups = 5,
            //    MaximumFileSize = "1GB",
            //    RollingStyle = RollingFileAppender.RollingMode.Size,
            //    StaticLogFileName = true
            //};
            //roller.ActivateOptions();

            RollingFileAppender rollerAll = new RollingFileAppender {
                AppendToFile = true,
                File = logDir + "All" + ".txt",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1GB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            rollerAll.ActivateOptions();
            
            ILoggerRepository repository = LogManager.GetRepository("All");
            
            BasicConfigurator.Configure(repository, rollerAll);
            
            ILog log = LogManager.GetLogger("All", logName);
            
            return log;
        }
    }
}
