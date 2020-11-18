using Dispatcher.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher.Services
{
    public class FileDispatchService : BackgroundService
    {
        private int _requestsNumber;
        private readonly Uri _requestUri;
        private readonly string _inputFolder;
        private readonly string _ignordedFolder;
        private readonly string _fileExtension;
        private readonly int _maxRequestsNumber;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FileDispatchService> _logger;
        private FileSystemWatcher _fileSystemWatcher;

        public FileDispatchService(IConfiguration configuration, ILogger<FileDispatchService> logger,
                                   IServiceProvider serviceProvider, IBackgroundTaskQueue taskQueue)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            
            _requestsNumber = 0;
            _maxRequestsNumber = Environment.ProcessorCount;
            _requestUri = new Uri(configuration["RequestUri"]);
            _ignordedFolder = configuration["IgnoredFolder"];
            _inputFolder = configuration["InputFolder"];
            _fileExtension = configuration["FileExtension"];
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("File Dispatcher Service started");

            try
            {
                if (!Directory.Exists(_inputFolder))
                {
                    Directory.CreateDirectory(_inputFolder);
                }

                if (!Directory.Exists(_ignordedFolder))
                {
                    Directory.CreateDirectory(_ignordedFolder);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cannot create folder. Exception: {ex.Message}");

                throw ex;
            }

            ConfigureFileSystemWatcher();

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("File Dispatcher Service is stopping");

            _fileSystemWatcher.EnableRaisingEvents = false;

            await base.StopAsync(cancellationToken);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Interlocked.Increment(ref _requestsNumber);

            if (Thread.VolatileRead(ref _requestsNumber) >= _maxRequestsNumber)
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
            }

            _taskQueue.Enqueue(async token => await SendFileAsync(e.FullPath, e.Name));

            Interlocked.Decrement(ref _requestsNumber);

            if (Thread.VolatileRead(ref _requestsNumber) < _maxRequestsNumber && !_fileSystemWatcher.EnableRaisingEvents)
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogInformation($"File System Watcher error: {e.GetException().Message}");
        }

        public async Task SendFileAsync(string fullPath, string fileName)
        {
            ITransferService transferService = (ITransferService)_serviceProvider.GetService(typeof(ITransferService));

            bool isSucceeded = await transferService.SendFileAsync(_requestUri, fullPath, fileName);

            try
            {
                if (isSucceeded)
                {
                    System.IO.File.Delete(fullPath);
                }
                else
                {
                    System.IO.File.Move(fullPath, Path.Combine(_ignordedFolder, fileName));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"File: {fileName}. Exception: {ex.Message}");
            }
        }

        private void ConfigureFileSystemWatcher()
        {
            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = _inputFolder,
                Filter = _fileExtension,
                InternalBufferSize = 65536,
                IncludeSubdirectories = false,
                NotifyFilter =  NotifyFilters.FileName      
            };

            _fileSystemWatcher.Error += OnError;
            _fileSystemWatcher.Created += OnCreated;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing File Dispatcher Service");

            _fileSystemWatcher?.Dispose();

            base.Dispose();
        }
    }
}
