using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using log4net.Core;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using log4net.Config;
using log4net.Layout;
using System.Reflection;
using System.IO;
using System.Threading;

namespace PathTracker_Backend
{
    public static class LogCreator
    {
        public static ILog CreateLog(string logName) {

            var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly()).Name;

            var dir = Directory.GetCurrentDirectory();

            //Create the rolling file appender
            var appender = new log4net.Appender.RollingFileAppender();
            appender.Name = "RollingFileAppender";
            appender.File = dir + "\\Logs\\" + logName;
            appender.StaticLogFileName = true;
            appender.AppendToFile = false;
            appender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
            appender.MaxSizeRollBackups = 10;
            appender.MaximumFileSize = "10MB";
            appender.PreserveLogFileNameExtension = true;

            string repositoryName = string.Format("{0}Repository","MyRepo");
            var repository = LoggerManager.CreateRepository(repositoryName);
            string loggerName = string.Format("{0}Logger", "MyRepo");


            BasicConfigurator.Configure(repository, appender);
            ILog logger = LogManager.GetLogger(repositoryName, loggerName);

            return logger;
        }

        public static void Setup() {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetEntryAssembly());

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            var dir = Directory.GetCurrentDirectory();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = false;
            roller.File = dir + "\\Logs\\Test.txt";
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 5;
            roller.MaximumFileSize = "1GB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            //MemoryAppender memory = new MemoryAppender();
            //memory.ActivateOptions();
            //hierarchy.Root.AddAppender(memory);

            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;
        }
    }
}
