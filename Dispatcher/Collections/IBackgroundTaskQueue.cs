﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher.Collections
{
    public interface IBackgroundTaskQueue
    {
        void Enqueue(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
