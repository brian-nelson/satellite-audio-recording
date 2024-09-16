using Microsoft.AspNetCore.Mvc;
using SAR.Apps.Server.Objects;
using SAR.Apps.Server.Services;
using SAR.Libraries.Common.Interfaces;

namespace SAR.Apps.Server.Controllers
{
    [Produces("application/json")]
    public class SetupController : ControllerBase
    {
        private readonly ProjectService _projectService;
        private readonly AuthService _authService;
        private readonly ISarLogger _logger;

        public SetupController(
            AuthService authService,
            ISarLogger logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/setup")]
        public ActionResult<IsSetupResponse> IsSetup()
        {
            var response = new IsSetupResponse();

            if (_authService.IsSetup())
            {
                response.IsSetup = true;

                _logger.Info("System is setup");
            }
            else
            {
                _logger.Info("System is not setup");
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("api/setup")]
        public ActionResult<SetupResponse> Setup([FromBody] SetupRequest request)
        {
            if (!_authService.IsSetup())
            {
                if (request == null
                    || string.IsNullOrEmpty(request.OwnerEmail)
                    || string.IsNullOrEmpty(request.OwnerPassword)
                    || string.IsNullOrEmpty(request.OwnerGivenName)
                    || string.IsNullOrEmpty(request.OwnerFamilyName)
                    || string.IsNullOrEmpty(request.InitialProjectName))
                {
                    return BadRequest();
                }

                _authService.Setup(
                    request.OwnerEmail,
                    request.OwnerPassword,
                    request.OwnerGivenName,
                    request.OwnerFamilyName,
                    request.InitialProjectName);

                var response = new SetupResponse
                {
                    IsSetup = true
                };

                return Ok(response);
            }

            return BadRequest();
        }
    }
}
