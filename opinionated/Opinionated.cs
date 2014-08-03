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
        public void ConfigureLogging()
        {
            var hierarchy = LogManager.GetRepository() as Hierarchy;

            ConfigureConsoleTarget(hierarchy);
            ConfigureFileTarget(hierarchy);
            ConfigureEmailTarget(hierarchy);

            if (hierarchy != null) {
                hierarchy.Root.Level = Level.All;
                hierarchy.Configured = true;
            }
        }

        static
        void ConfigureConsoleTarget(Hierarchy hierarchy)
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
        }

        static
        void ConfigureFileTarget(Hierarchy hierarchy)
        {
            var patternLayout = new PatternLayout { ConversionPattern = ConversionPattern };
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender {
                Layout = patternLayout,
                AppendToFile = true,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                MaxSizeRollBackups = 7,
                File = Path.Combine(
                    new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName,
                    "logs", Environment.MachineName + ".log")
            };

            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);
        }

        static
        void ConfigureEmailTarget(Hierarchy hierarchy)
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
        }

        const String ConversionPattern = @"[%d %logger{1}] [%t] %-5p %m%n";
    }
}
