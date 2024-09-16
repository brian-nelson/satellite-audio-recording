using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAR.Apps.Server.Helpers;
using SAR.Apps.Server.Objects;
using SAR.Apps.Server.Services;
using SAR.Libraries.Common.Interfaces;

namespace SAR.Apps.Server.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public class ScriptController : ControllerBase
    {
        private readonly ProjectService _projectService;
        private readonly ISarLogger _logger;

        public ScriptController(
            ProjectService projectService,
            ISarLogger logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        [HttpGet]
        [Route("api/projects/{projectId:Guid}/script/{start:int}/{end:int}")]
        public ActionResult<IEnumerable<ScriptLine>> GetScript(
            [FromRoute] Guid projectId,
            [FromRoute] int start,
            [FromRoute] int end)
        {
            var response = _projectService.GetScript(
                User.GetPersonId(),
                projectId,
                start,
                end);

            return Ok(response);
        }

        [HttpGet]
        [Route("api/projects/{projectId:Guid}/script/scene/{sceneId:Guid}")]
        public ActionResult<IEnumerable<ScriptLine>> GetScript(
            [FromRoute] Guid projectId,
            [FromRoute] Guid sceneId)
        {
            var response = _projectService.GetScript(
                User.GetPersonId(),
                projectId,
                sceneId);

            return Ok(response);
        }

        [HttpGet]
        [Route("api/projects/{projectId:Guid}/script/{characterId:Guid}/next/{currentScriptSequenceNumber:int}")]
        public ActionResult<ScriptLine> GetNextScriptLineByCharacter(
            [FromRoute] Guid projectId,
            [FromRoute] Guid characterId,
            [FromRoute] int currentScriptSequenceNumber)
        {
            var response = _projectService.GetNextScriptLineByCharacter(
                User.GetPersonId(), 
                projectId, 
                characterId,
                currentScriptSequenceNumber);

            return Ok(response);
        }

        [HttpGet]
        [Route("api/projects/{projectId:Guid}/script/{characterId:Guid}/previous/{currentScriptSequenceNumber:int}")]
        public ActionResult<ScriptLine> GetPreviousScriptLineByCharacter(
            [FromRoute] Guid projectId,
            [FromRoute] Guid characterId,
            [FromRoute] int currentScriptSequenceNumber)
        {
            var response = _projectService.GetPreviousScriptLineByCharacter(
                User.GetPersonId(),
                projectId,
                characterId,
                currentScriptSequenceNumber);

            return Ok(response);
        }

        [HttpGet]
        [Route("api/projects/{projectId:Guid}/script/{characterId:Guid}/next/{currentScriptSequenceNumber:int}/limit/{limit:int}")]
        public ActionResult<IEnumerable<ScriptLine>> GetNextScriptLinesByCharacter(
            [FromRoute] Guid projectId,
            [FromRoute] Guid characterId,
            [FromRoute] int currentScriptSequenceNumber,
            [FromRoute] int limit)
        {
            var response = _projectService.GetNextScriptLinesByCharacter(
                User.GetPersonId(),
                projectId,
                characterId,
                currentScriptSequenceNumber,
                limit);

            return Ok(response);
        }

        [HttpGet]
        [Route("api/projects/{projectId:Guid}/script/{characterId:Guid}/previous/{currentScriptSequenceNumber:int}/limit/{limit:int}")]
        public ActionResult<IEnumerable<ScriptLine>> GetPreviousScriptLinesByCharacter(
            [FromRoute] Guid projectId,
            [FromRoute] Guid characterId,
            [FromRoute] int currentScriptSequenceNumber,
            [FromRoute] int limit)
        {
            var response = _projectService.GetPreviousScriptLinesByCharacter(
                User.GetPersonId(),
                projectId,
                characterId,
                currentScriptSequenceNumber,
                limit);

            return Ok(response);
        }

        [HttpGet]
        [Route("api/projects/{projectId:Guid}/script/{characterDialogId:Guid}/context")]
        public ActionResult<ScriptContextResponse> GetScriptLineContext(
            [FromRoute] Guid projectId,
            [FromRoute] Guid characterDialogId)
        {
            var response = new ScriptContextResponse();

            Guid userPersonId = User.GetPersonId();

            response.Context = _projectService.GetScriptLineContext(
                    userPersonId,
                    projectId,
                    characterDialogId)
                .ToList();

            response.NextLine = _projectService.GetNextLineId(
                userPersonId,
                projectId,
                characterDialogId);

            response.PreviousLine = _projectService.GetPreviousLineId(
                userPersonId,
                projectId,
                characterDialogId);

            return Ok(response);
        }
    }
}
