using Dispatcher.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dispatcher.Services
{
    public class TransferService: ITransferService
    {
        private readonly ILogger<TransferService> _logger;

        public TransferService(ILogger<TransferService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> SendFileAsync(Uri requstUri, string fullPath, string fileName)
        {
            try
            {              
                FileInfo fileInfo = new FileInfo(fullPath);

                fileInfo.WaitUntilLocked();

                ByteArrayContent bytes = new ByteArrayContent(System.IO.File.ReadAllBytes(fullPath));

                MultipartFormDataContent multiContent = new MultipartFormDataContent
                {
                    { bytes, "file", fileName }
                };

                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage responseMessage = await httpClient.PostAsync(requstUri, multiContent);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    _logger.LogWarning($"File: {fileName} not sent. Status Code: {responseMessage.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning($"File: {fileName}. Exception: {ex.Message}");
            }

            return false;
        }
    }
}
