using Dispatcher.Collections;
using Dispatcher.Configs;
using Dispatcher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace File.Dispatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger.Initialize();

            try
            {
                Log.Information("Application started");

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<FileDispatchService>();
                    services.AddHostedService<TaskQueueService>();
                    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
                    services.AddTransient<ITransferService, TransferService>();
                });
    }
}
