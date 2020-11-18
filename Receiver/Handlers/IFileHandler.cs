using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Receiver.Handlers
{
    public interface IFileHandler
    {
        Task<bool> Save(IFormFileCollection files);
    }
}
