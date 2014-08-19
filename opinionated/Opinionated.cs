using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;


using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Core;
using log4net;
using log4net.Appender;

namespace opinionated
{
    static
    public class Opinionated
    {
        static
        public void ConfigureLogging(String path = null)
        {
            Configure()
                .ConsoleTarget()
                .EmailTarget()
                .FileTarget(path)
                .Done();
        }

        static 
        public Hierarchy TraceTarget(this Hierarchy hierarchy)
        {
            var patternLayout = new PatternLayout { ConversionPattern = ConversionPattern };
            patternLayout.ActivateOptions();

            var trace = new AspNetTraceAppender { Layout = patternLayout };

            trace.ActivateOptions();
            hierarchy.Root.AddAppender(trace);

            return hierarchy;
        }

        static
        public Hierarchy ConsoleTarget(this Hierarchy hierarchy)
        {
            if (Environment.UserInteractive) {
                var patternLayout = new PatternLayout { ConversionPattern = ConversionPattern };
                patternLayout.ActivateOptions();

                var console = new ColoredConsoleAppender {
                    Layout = patternLayout
                };

                console.ActivateOptions();
                hierarchy.Root.AddAppender(console);
            }

            return hierarchy;
        }

        static
        public Hierarchy FileTarget(this Hierarchy hierarchy, String path = null)
        {
            var patternLayout = new PatternLayout { ConversionPattern = ConversionPattern };
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender {
                Layout = patternLayout,
                AppendToFile = true,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                MaxSizeRollBackups = 7,
                File = Path.Combine(
                    new FileInfo(path ?? Assembly.GetExecutingAssembly().Location).DirectoryName,
                    @"..\logs", "today.log")
            };

            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            return hierarchy;
        }

        static
        public Hierarchy EmailTarget(this Hierarchy hierarchy)
        {
            var appSettings = ConfigurationManager.AppSettings;

            var patternLayout = new PatternLayout { ConversionPattern = ConversionPattern };
            patternLayout.ActivateOptions();

            var smtp = new SmtpAppender {
                SmtpHost = appSettings["smtp:Host"] ?? "127.0.0.1",
                Port     = Int32.Parse(appSettings["smtp:Port"] ?? "25"),
                Username = appSettings["smtp:Username"],
                Password = appSettings["smtp:Password"],
                To       = appSettings["smtp:ToEmail"],
                BufferSize = 1,
            };

            smtp.Threshold = Level.Error;

            smtp.Subject = String.Format("{0} on {1}", smtp.Threshold, Environment.MachineName);

            smtp.Layout = patternLayout;

            smtp.ActivateOptions();

            hierarchy.Root.AddAppender(smtp);

            return hierarchy;
        }

        static
        public Hierarchy Configure()
        {
            return LogManager.GetRepository() as Hierarchy;
        }

        static
        public void Done(this Hierarchy hierarchy)
        {
            if (hierarchy == null) return;

            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;
        }

        const String ConversionPattern = @"[%d %logger{1}] [%t] %-5p %m%n";
    }
}
