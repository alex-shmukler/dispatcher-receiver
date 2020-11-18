using Dispatcher.Collections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher.Services
{
    public class TaskQueueService : BackgroundService
    {
        private readonly Task[] _executors;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<TaskQueueService> _logger;
        private CancellationTokenSource _tokenSource;

        public TaskQueueService(ILogger<TaskQueueService> logger, IBackgroundTaskQueue taskQueue)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
            _executors = new Task[Environment.ProcessorCount];
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Queued Hosted Service started");

            await BackgroundProcessing(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping");

            _tokenSource.Cancel();

            await base.StopAsync(cancellationToken);
        }

        private async Task BackgroundProcessing(CancellationToken cancellationToken)
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            for (int i = 0; i < _executors.Length; i++)
            {
                _executors[i] = new Task(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        Func<CancellationToken, Task> workItem = await _taskQueue.DequeueAsync(cancellationToken);

                        try
                        {
                            await workItem(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error occurred executing {nameof(workItem)}");
                        }
                    }
                }, _tokenSource.Token);

                _executors[i].Start();
            }

            await Task.CompletedTask;
        }
    }
}
