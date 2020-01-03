﻿using System;
using System.Collections.Generic;
using System.Linq;
using SAR.Apps.Server.Objects;
using SAR.Libraries.Fountain.Objects;
using SAR.Modules.Script.Constants;
using SAR.Modules.Script.Helpers;
using SAR.Modules.Script.Objects;
using SAR.Modules.Script.Services;
using SAR.Modules.Server.Constants;
using SAR.Modules.Server.Services;

namespace SAR.Apps.Server.Services
{
    public class ProjectService
    {
        private readonly ScriptService _scriptService;
        private readonly ServerService _serverService;

        public ProjectService(
            ScriptService scriptService,
            ServerService serverService)
        {
            _scriptService = scriptService;
            _serverService = serverService;
        }

        public IEnumerable<Project> GetProjects(Guid userPersonId)
        {
            return _scriptService.GetProjectsByPerson(userPersonId);
        }

        public bool HasAccessToProject(Guid userPersonId, Guid projectId)
        {
            return _scriptService.HasAccessToProject(userPersonId, projectId);
        }

        public Project GetProject(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                return _scriptService.GetProject(projectId);
            }

            throw new UnauthorizedAccessException();
        }

        public void SaveProject(Guid userPersonId, Project project)
        {
            if (HasAccessToProject(userPersonId, project.Id))
            {
                //TODO - Validate Write Access
                _scriptService.Save(project);
                return;
            }

            throw new UnauthorizedAccessException();
        }

        public void DeleteProject(Guid userPersonId, Guid projectId)
        {
            if (IsProjectOwner(userPersonId, projectId))
            {
                //TODO - Validate Write Access (Owner?)

                //Delete all children of the project
                _scriptService.DeleteProjectScript(projectId);

                //Delete the access rights
                _scriptService.DeleteProjectAccessByProject(projectId);

                //Delete the Project
                _scriptService.DeleteProject(projectId);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }


        }

        public void DeleteProjectScript(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                //TODO - Validate Write Access (Owner?)

                //Delete all children of the project
                _scriptService.DeleteProjectScript(projectId);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }

        public bool IsProjectOwner(Guid personId, Guid projectId)
        {
            var projectAccess = _scriptService.GetProjectAccess(personId, projectId);

            if (projectAccess != null)
            {
                if (projectAccess.AccessTypes.Contains(AccessTypes.Owner))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<Character> GetCharacters(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                return _scriptService.GetCharactersByProject(projectId);
            }

            throw new UnauthorizedAccessException();
        }

        public Character GetCharacter(Guid userPersonId, Guid projectId, Guid characterId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                return _scriptService.GetCharacter(characterId);
            }

            throw new UnauthorizedAccessException();
        }

        public void SaveCharacter(Guid userPersonId, Character character)
        {
            if (HasAccessToProject(userPersonId, character.ProjectId))
            {
                _scriptService.Save(character);
                return;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<CharacterWithActor> GetCharactersWithActors(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var output = new List<CharacterWithActor>();

                var characters = _scriptService.GetCharactersByProject(projectId);
                foreach (var character in characters)
                {
                    var cwa = new CharacterWithActor(character);

                    if (character.ActorPersonId != null)
                    {
                        cwa.Actor = _scriptService.GetPerson(character.ActorPersonId.Value);
                    }

                    output.Add(cwa);
                }

                return output;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Person> GetPeople(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var temp = new Dictionary<Guid, Person>();

                var accessRights = _scriptService.GetProjectAccessByProject(projectId);
                foreach (var projectAccess in accessRights)
                {
                    if (!temp.ContainsKey(projectAccess.PersonId))
                    {
                        var person = _scriptService.GetPerson(projectAccess.PersonId);
                        temp.Add(person.Id, person);
                    }
                }

                return temp.Values.ToList();
            }

            throw new UnauthorizedAccessException();
        }

        public Person GetPerson(Guid userPersonId, Guid projectId, Guid personId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var temp = new Dictionary<Guid, Person>();

                var accessRight = _scriptService.GetProjectAccess(personId, projectId);

                if (accessRight != null)
                {
                    var person = _scriptService.GetPerson(personId);
                    return person;
                }

                return null;
            }

            throw new UnauthorizedAccessException();
        }

        public PersonWithAccess GetPersonWithAccess(Guid userPersonId, Guid projectId, Guid personId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var temp = new Dictionary<Guid, Person>();

                var accessRight = _scriptService.GetProjectAccess(personId, projectId);

                if (accessRight != null)
                {

                    var person = _scriptService.GetPerson(personId);
                    var pwa = new PersonWithAccess(person);
                    pwa.AccessTypes.AddRange(accessRight.AccessTypes);

                    return pwa;
                }

                return null;
            }

            throw new UnauthorizedAccessException();
        }

        public bool IsSystemOwner(Guid userId)
        {
            var user = _serverService.GetUser(userId);

            if (user.UserType.Equals(UserTypes.Owner))
            {
                return true;
            }

            return false;
        }

        public bool IsSystemAdmin(Guid userId)
        {
            var user = _serverService.GetUser(userId);

            if (user.UserType.Equals(UserTypes.Owner)
                || user.UserType.Equals(UserTypes.Admin))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Person> GetPeopleInSystem(Guid userId)
        {
            if (IsSystemAdmin(userId))
            {
                return _scriptService.GetPeople();
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<PersonWithAccess> GetPeopleWithAccess(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var temp = new Dictionary<Guid, PersonWithAccess>();

                var accessRights = _scriptService.GetProjectAccessByProject(projectId);
                foreach (var projectAccess in accessRights)
                {
                    if (!temp.ContainsKey(projectAccess.PersonId))
                    {
                        var person = _scriptService.GetPerson(projectAccess.PersonId);
                        var pwa = new PersonWithAccess(person);
                        pwa.AccessTypes.AddRange(projectAccess.AccessTypes);

                        temp.Add(person.Id, pwa);
                    }
                }

                return temp.Values.ToList();
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Scene> GetScenes(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var project = _scriptService.GetProject(projectId);

                var output = new List<Scene>();

                int i = 0;

                foreach (var sceneId in project.Scenes)
                {
                    var scriptElement = _scriptService.GetScriptElement(sceneId);
                    var fs = (SceneElement) scriptElement.ToFountain();

                    Scene s = new Scene
                    {
                        Id = sceneId,
                        InteriorExterior = fs.InteriorExterior,
                        Location = fs.Location,
                        SceneNumber = fs.SceneNumber,
                        TimeOfDay = fs.TimeOfDay,
                        SequenceNumber = i,
                        ScriptPosition = scriptElement.SequenceNumber
                    };

                    output.Add(s);

                    i++;
                }

                return output;

            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<ScriptLine> GetScript(Guid userPersonId, Guid projectId, int start, int end)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var elements =  _scriptService.GetScriptElements(projectId, start, end);

                var output = new List<ScriptLine>();

                foreach (var se in elements)
                {
                    var parts = se.FountainElementType.Split(".".ToCharArray());
                    var type = parts[parts.Length - 1].Replace("Element", "");

                    var sl = new ScriptLine
                    {
                        ProjectId = se.ProjectId,
                        Line = se.FountainRawData,
                        SequenceNumber = se.SequenceNumber,
                        LineType = type
                    };

                    output.Add(sl);
                }

                return output;
            }

            throw new UnauthorizedAccessException();
        }
    }
}
