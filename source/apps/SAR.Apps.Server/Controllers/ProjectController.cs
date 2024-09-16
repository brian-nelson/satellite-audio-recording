using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAR.Apps.Server.Helpers;
using SAR.Apps.Server.Services;
using SAR.Libraries.Common.Interfaces;
using SAR.Modules.Script.Objects;

namespace SAR.Apps.Server.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;
        private readonly ISarLogger _logger;

        public ProjectController(
            ProjectService projectService,
            ISarLogger logger)
        {
            _projectService = projectService;
            _logger = logger;
        }


        [HttpGet]
        [Route("api/projects")]
        public ActionResult<List<Project>> GetProjects()
        {
            var response = _projectService.GetProjects(User.GetPersonId());
            return Ok(response);
        }

        [HttpGet]
        [Route("api/projects/{projectId:Guid}")]
        public ActionResult<Project> GetProject([FromRoute] Guid projectId)
        {
            var personId = User.GetPersonId();

            try
            {
                var project = _projectService.GetProject(personId, projectId);
                return Ok(project);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("api/projects")]
        public ActionResult SaveProject([FromBody] Project project)
        {
            var userId = User.GetUserId();
            var personId = User.GetPersonId();

            try
            {
                _projectService.SaveProject(userId, personId, project);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
    }
}
