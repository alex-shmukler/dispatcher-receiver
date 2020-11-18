using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Receiver.Handlers;
using System;
using System.Threading.Tasks;

namespace Receiver.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileHandler _fileHandler;
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger, IFileHandler fileHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileHandler = fileHandler ?? throw new ArgumentNullException(nameof(fileHandler));
        }

        [HttpPost]
        [Route("receive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Receive()
        {
            if (!Request.HasFormContentType)
            {
                return BadRequest();
            }

            try
            {
                bool isSucceeded = await _fileHandler.Save(HttpContext.Request.Form.Files);

                if (isSucceeded)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Exception: {ex.Message}");

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
