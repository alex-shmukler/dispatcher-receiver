using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher.Collections
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue, IDisposable
    {
        private readonly int _maxRequestsNumber;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems;

        public BackgroundTaskQueue()
        {
            _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
            _maxRequestsNumber = Environment.ProcessorCount;
            _semaphore = new SemaphoreSlim(0, _maxRequestsNumber);
        }

        public void Enqueue(Func<CancellationToken, Task> workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);

            if (_semaphore.CurrentCount < _maxRequestsNumber)
            {
                _semaphore.Release();
            }
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            if (_workItems.Count <= _maxRequestsNumber)
            {
                await _semaphore.WaitAsync(cancellationToken);
            }

            _workItems.TryDequeue(out Func<CancellationToken, Task> workItem);

            return workItem;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _semaphore?.Dispose();
            }
        }
    }
}
