using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;

namespace Dispatcher.Configs
{
    public static class Logger
    {
        public static void Initialize()
        {
            const string loggerTemplate = @"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message:lj}{NewLine}{Exception}";
            string logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "log-.txt");

            Log.Logger = new LoggerConfiguration().MinimumLevel.Override(source: "Microsoft",
                                                                         minimumLevel: LogEventLevel.Warning)
                                                  .Enrich.FromLogContext()
                                                  .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information,
                                                                   outputTemplate: loggerTemplate,
                                                                   theme: AnsiConsoleTheme.None)
                                                  .WriteTo.File(path: logfile,
                                                                restrictedToMinimumLevel: LogEventLevel.Information,
                                                                outputTemplate: loggerTemplate,
                                                                rollingInterval: RollingInterval.Day,
                                                                retainedFileCountLimit: null)
                                                  .CreateLogger();
        }
    }
}
