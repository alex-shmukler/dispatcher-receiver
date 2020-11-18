using System;
using System.Threading.Tasks;

namespace Dispatcher.Services
{
    public interface ITransferService
    {
        public Task<bool> SendFileAsync(Uri requstUri, string fullPath, string fileName);
    }
}
