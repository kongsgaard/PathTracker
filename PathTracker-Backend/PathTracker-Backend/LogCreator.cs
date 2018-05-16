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

        public static ILog CreateLog(string logName) {

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender {
                AppendToFile = true,
                File = logDir + logName + ".txt",
                Layout = patternLayout,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "1GB",
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();

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

            Hierarchy hierarchy;

            try {
                hierarchy = (Hierarchy)LogManager.CreateRepository("All");
            }
            catch {
                hierarchy = (Hierarchy)LogManager.GetRepository("All");
            }

            var logger = hierarchy.LoggerFactory.CreateLogger(hierarchy, logName);
            logger.Hierarchy = hierarchy;
            logger.Level = Level.All;

            logger.AddAppender(roller);
            logger.AddAppender(rollerAll);

            logger.Repository.Configured = true;
            

            ILog log = new LogImpl(logger);
            
            
            return log;
        }
    }
}
