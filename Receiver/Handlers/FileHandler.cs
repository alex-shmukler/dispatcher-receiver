using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Receiver.Handlers
{
    public class FileHandler : IFileHandler
    {
        private readonly string _targetFolder;
        private readonly ILogger<FileHandler> _logger;

        public FileHandler(IConfiguration configuration, ILogger<FileHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _targetFolder = configuration["TargetFolder"];

            CkeckTargetFolder();
        }

        public async Task<bool> Save(IFormFileCollection files)
        {
            try
            {
                foreach (IFormFile file in files)
                {
                    if (file.Length > 0)
                    {
                        string filePath = Path.Combine(_targetFolder, file.FileName);

                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"File Service Exception: {ex.Message}");

                return false;
            }
        }

        private void CkeckTargetFolder()
        {
            try
            {
                if (!Directory.Exists(_targetFolder))
                {
                    Directory.CreateDirectory(_targetFolder);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Ckeck Target Folder Exception: {ex.Message}");

                throw ex;
            }
        }
    }
}
