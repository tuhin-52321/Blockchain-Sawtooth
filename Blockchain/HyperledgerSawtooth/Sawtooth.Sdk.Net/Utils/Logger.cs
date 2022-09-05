using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net.Appender;
using log4net.Layout;
using System.Diagnostics;

namespace Sawtooth.Sdk.Net.Utils
{
    public class Logger
    {
        private static bool initialized = false;
        private readonly ILog log;
        private readonly bool isDebugEnabled;
        private readonly bool isInfoEnabled;

        private static Dictionary<Type, Logger> instances = new Dictionary<Type, Logger>();

        public static Logger GetLogger(Type type)
        {
            if(!initialized)
            {
                Initialize();
            }
            if(!instances.TryGetValue(type, out Logger? logger))
            {
                logger = new Logger(type);
                instances.Add(type, logger);
            }

            return logger;
        }

        private static void Initialize()
        {
            log4net.Config.XmlConfigurator.Configure();

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            if (!hierarchy.Configured)
            {

                PatternLayout patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = "%date %-5level [%thread] %logger - %message%newline";
                
                patternLayout.ActivateOptions();

                RollingFileAppender roller = new RollingFileAppender();
                roller.AppendToFile = true;
                roller.File = AppDomain.CurrentDomain.FriendlyName + ".log";
                roller.Layout = patternLayout;
                roller.MaxSizeRollBackups = 5;
                roller.MaximumFileSize = "1GB";
                roller.RollingStyle = RollingFileAppender.RollingMode.Size;
                roller.StaticLogFileName = true;
                roller.ActivateOptions();
                hierarchy.Root.AddAppender(roller);

                hierarchy.Root.Level = Level.Debug;
                hierarchy.Configured = true;
            }
#if DEBUG
            hierarchy.Root.Level = Level.Debug;
#endif
#if RELEASE
            hierarchy.Root.Level = Level.Info;
#endif
            initialized = true;
        }



        private readonly bool isWarnEnabled;
        private readonly bool isErrorEnabled;
        private readonly bool isFatalEnabled;
        private Logger(Type type)
        {
            log = LogManager.GetLogger(type);
            isDebugEnabled = log.IsDebugEnabled;
            isInfoEnabled = log.IsInfoEnabled;
            isWarnEnabled = log.IsWarnEnabled;
            isErrorEnabled = log.IsErrorEnabled;
            isFatalEnabled = log.IsFatalEnabled;
        }

        public void Debug(string message, params object[] args)
        {
            if (isDebugEnabled)
            {
                log.Debug(string.Format(message,args));
            }
        }

        public void Info(string message, params object[] args)
        {
            if (isInfoEnabled)
            {
                log.Info(string.Format(message, args));
            }
        }
        public void Warn(string message, params object[] args)
        {
            if (isWarnEnabled)
            {
                log.Warn(string.Format(message, args));
            }
        }
        public void Error(string message, params object[] args)
        {
            if (isErrorEnabled)
            {
                log.Error(string.Format(message, args));
            }
        }
        public void Fatal(string message, params object[] args)
        {
            if (isFatalEnabled)
            {
                log.Fatal(string.Format(message, args));
            }
        }
    }
}
