using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAR.Apps.Server.Objects;
using SAR.Libraries.Common.Helpers;
using SAR.Libraries.Common.Storage;
using SAR.Libraries.Fountain.Objects;
using SAR.Modules.Script.Constants;
using SAR.Modules.Script.Helpers;
using SAR.Modules.Script.Objects;
using SAR.Modules.Script.Services;
using SAR.Modules.Server.Constants;
using SAR.Modules.Server.Objects;
using SAR.Modules.Server.Services;

namespace SAR.Apps.Server.Services
{
    public class ProjectService
    {
        private readonly ScriptService _scriptService;
        private readonly ServerService _serverService;
        private readonly IFileStorage _fileStorage;

        public ProjectService(
            ScriptService scriptService,
            ServerService serverService,
            IFileStorage fileStorage)
        {
            _scriptService = scriptService;
            _serverService = serverService;
            _fileStorage = fileStorage;
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

        public void SaveProject(Guid userId, Guid userPersonId, Project project)
        {
            var existing = _scriptService.GetProject(project.Id);
            if (existing == null)
            {
                //New Project.  Check of user is a system owner
                if (IsSystemOwner(userId))
                {
                    //They are so save it and add project access
                    _scriptService.Save(project);

                    ProjectAccess pa = new ProjectAccess
                    {
                        ProjectId = project.Id,
                        PersonId = userPersonId,
                    };
                    pa.AccessTypes.Add(AccessTypes.Owner);
                    _scriptService.Save(pa);
                }

                return;
            }

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
                var character = _scriptService.GetCharacter(characterId);
                return character;
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

        public UICharacter BuildUICharacter(Character character)
        {
            var uic = new UICharacter(character);

            if (character.PerformerPersonId != null)
            {
                uic.Performer = _scriptService.GetPerson(character.PerformerPersonId.Value);
            }

            return uic;
        }

        public IEnumerable<UICharacter> GetUICharacters(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var access = _scriptService.GetProjectAccess(userPersonId, projectId);
                
                bool onlyOwn = access.AccessTypes.Count == 1
                               && access.AccessTypes.Contains(AccessTypes.Performer);

                var output = new List<UICharacter>();

                var characters = _scriptService.GetCharactersByProject(projectId);
                foreach (var character in characters)
                {
                    if (onlyOwn)
                    {
                        if (character.PerformerPersonId == userPersonId)
                        {
                            output.Add(BuildUICharacter(character));
                        }                        
                    }
                    else
                    {
                        output.Add(BuildUICharacter(character));    
                    }
                }

                return output;
            }

            throw new UnauthorizedAccessException();
        }

        public UICharacter GetUICharacter(Guid userPersonId, Guid projectId, Guid characterId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var output = new List<UICharacter>();

                var character = _scriptService.GetCharacter(characterId);
                return BuildUICharacter(character);
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
        
        /// <summary>
        /// Returns a list of people not currently associated with the project
        /// </summary>
        /// <param name="userPersonId"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public IEnumerable<Person> GetAvailablePeople(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var currentPeople = GetPeople(userPersonId, projectId)
                    .ToDictionary(p=> p.Id);

                var output = new Dictionary<Guid, Person>();
                
                var allPeople = _serverService.GetUsers();
                foreach (var user in allPeople)
                {
                    if (!currentPeople.ContainsKey(user.PersonId)
                        && !output.ContainsKey(user.PersonId)) //Shouldn't be the case, but just to be safe
                    {
                        var person = _scriptService.GetPerson(user.PersonId);
                        output.Add(person.Id, person);
                    }
                }

                return output.Values.ToList();
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
        
        public Person GetPerson(Guid userId, Guid personId)
        {
            if (IsSystemOwner(userId)
                || IsSystemAdmin(userId))
            {
                var person = _scriptService.GetPerson(personId);
                return person;
            }

            throw new UnauthorizedAccessException();
        }

        public Person GetPerson(Guid userPersonId)
        {
            return _scriptService.GetPerson(userPersonId);
        }
        
        public IEnumerable<ProjectAccess> GetAccessRights(Guid userPersonId)
        {
            return _scriptService.GetAccessRights(userPersonId);
        }

        public void SaveProfile(
            Guid userPersonId,
            Profile profile)
        {
            if (userPersonId == profile.PersonId)
            {
                var person = _scriptService.GetPerson(profile.PersonId);

                person.FamilyName = profile.FamilyName;
                person.GivenName = profile.GivenName;
                person.PhoneNumber = profile.PhoneNumber;

                _scriptService.Save(person);
                
                return;
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

        public void SaveProjectAccess(Guid userPersonId, Guid projectId, Guid personId, List<string> access)
        {
            if (IsProjectOwner(userPersonId,projectId))
            {
                var accessRight = _scriptService.GetProjectAccess(personId, projectId);

                if (accessRight == null)
                {
                    accessRight = new ProjectAccess();
                    accessRight.ProjectId = projectId;
                    accessRight.PersonId = personId;
                }

                accessRight.AccessTypes = access;
                _scriptService.Save(accessRight);

                return;
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

        private UIScene BuildScene(SAR.Modules.Script.Objects.Scene scene)
        {
            if (scene == null)
            {
                return null;
            }

            var scriptElement = _scriptService.GetScriptElement(scene.ScriptElementId);
            var fs = (SceneElement)scriptElement.ToFountain();

            UIScene s = new UIScene
            {
                Id = scene.Id,
                InteriorExterior = fs.InteriorExterior,
                Location = fs.Location,
                SceneNumber = fs.SceneNumber,
                TimeOfDay = fs.TimeOfDay,
                SequenceNumber = scene.Number,
                ScriptPosition = scriptElement.SequenceNumber
            };

            return s;
        }

        public UIScene GetScene(Guid userPersonId, Guid projectId, Guid sceneId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var scene = _scriptService.GetScene(sceneId);
                var s = BuildScene(scene);
                return s;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<UIScene> GetScenes(Guid userPersonId, Guid projectId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var scenes = _scriptService
                    .GetScenesByProject(projectId)
                    .ToList();
                var output = new List<Objects.UIScene>();

                foreach (var scene in scenes)
                {
                    UIScene s = BuildScene(scene);
                    output.Add(s);
                }

                if (scenes.Count > 0)
                {
                    var first = scenes[0];

                    if (first.ScriptSequenceNumber > 1)
                    {
                        var zero = new UIScene
                        {
                            Id = Guid.Empty,
                            InteriorExterior = "Intro", 
                            SequenceNumber = 0, 
                            ScriptPosition = 0
                        };

                        output.Insert(0, zero);
                    }
                }

                return output;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<ScriptLine> GetScript(Guid userPersonId, Guid projectId, Guid sceneId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                int start = 0;
                int nextSceneNumber = 1;

                if (sceneId != Guid.Empty)
                {
                    var thisScene = _scriptService.GetScene(sceneId);

                    start = thisScene.ScriptSequenceNumber; // Include the script scene line itself.
                    nextSceneNumber = thisScene.Number + 1;
                } 
                
                int? end = null;

                var nextScene = _scriptService.GetSceneByProjectNumber(projectId, nextSceneNumber);
                if (nextScene != null)
                {
                    end = nextScene.ScriptSequenceNumber - 1; //Remove the next scenes line.
                }
                
                var elements = _scriptService.GetScriptElements(projectId, start, end);

                var output = new List<ScriptLine>();

                foreach (var se in elements)
                {
                    output.Add(BuildScriptLine(se));
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
                    output.Add(BuildScriptLine(se));
                }

                return output;
            }

            throw new UnauthorizedAccessException();
        }

        public CharacterDialog GetCharacterDialog(Guid userPersonId, Guid projectId, Guid characterDialogId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                return _scriptService.GetCharacterDialog(characterDialogId);
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<ScriptLine> GetScriptLinesByCharacter(Guid userPersonId, Guid projectId, Guid characterId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var elements = _scriptService.GetCharacterDialogsByCharacter(characterId);

                var output = new List<ScriptLine>();

                foreach (var cdc in elements)
                {
                    var se = _scriptService.GetScriptElement(cdc.ScriptElementId);
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

        public ScriptLine GetNextScriptLineByCharacter(
            Guid userPersonId, 
            Guid projectId, 
            Guid characterId,
            int currentScriptSequenceNumber)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var characterDialog = _scriptService.GetNextCharacterDialogByCharacter(characterId, currentScriptSequenceNumber);

                var sl = BuildScriptLine(characterDialog);
                return sl;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<ScriptLine> GetNextScriptLinesByCharacter(
            Guid userPersonId,
            Guid projectId,
            Guid characterId,
            int currentScriptSequenceNumber,
            int limit)
        {
            Dictionary<Guid, UIScene> scenes = new Dictionary<Guid, UIScene>();

            if (HasAccessToProject(userPersonId, projectId))
            {
                var output = new List<ScriptLine>();
                var characterDialogs = _scriptService.GetNextCharacterDialogsByCharacter(characterId, currentScriptSequenceNumber, limit);

                foreach (var characterDialog in characterDialogs)
                {
                    var sl = BuildScriptLine(characterDialog, scenes);
                    output.Add(sl);
                }

                return output;
            }

            throw new UnauthorizedAccessException();
        }

        public ScriptLine GetPreviousScriptLineByCharacter(
            Guid userPersonId, 
            Guid projectId, 
            Guid characterId, 
            int currentScriptSequenceNumber)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var characterDialog = _scriptService.GetPreviousCharacterDialogByCharacter(characterId, currentScriptSequenceNumber);

                var sl = BuildScriptLine(characterDialog);
                return sl;
            }

            throw new UnauthorizedAccessException();
        }

        private ScriptLine BuildScriptLine(
            CharacterDialog characterDialog,
            Dictionary<Guid, UIScene> scenes = null,
            ScriptElement scriptElement = null)
        {
            UIScene scene = null;

            if (characterDialog.SceneId.HasValue)
            {
                Guid sceneId = characterDialog.SceneId.Value;

                if (scenes != null)
                {
                    if (scenes.ContainsKey(sceneId))
                    {
                        scene = scenes[sceneId];
                    }
                    else
                    {
                        scene = BuildScene(_scriptService.GetScene(sceneId));
                        scenes.Add(sceneId, scene);
                    }
                }
                else
                {
                    scene = BuildScene(_scriptService.GetScene(sceneId));
                }
            }

            var se = scriptElement;
            if (se == null)
            {
                se = _scriptService.GetScriptElement(characterDialog.ScriptElementId);
            }
                
            var parts = se.FountainElementType.Split(".".ToCharArray());
            var type = parts[parts.Length - 1].Replace("Element", "");

            var sl = new ScriptLine
            {
                CharacterDialogId = characterDialog.Id,
                CharacterId = characterDialog.CharacterId,
                ProjectId = se.ProjectId,
                Line = se.FountainRawData,
                SequenceNumber = se.SequenceNumber,
                LineType = type,
                RecordingCount = characterDialog.RecordingCount
            };

            if (scene != null)
            {
                sl.SceneId = scene.Id;

                if (scene.SceneNumber != null)
                {
                    sl.SceneNumber = scene.SceneNumber;
                }
                else
                {
                    sl.SceneNumber = Convert.ToString(scene.SequenceNumber);
                }
            }

            return sl;
        }

        private ScriptLine BuildScriptLine(
            ScriptElement scriptElement,
            Dictionary<Guid, UIScene> scenes = null)
        {
            var parts = scriptElement.FountainElementType.Split(".".ToCharArray());
            var type = parts[parts.Length - 1].Replace("Element", "");

            var sl = new ScriptLine
            {
                CharacterDialogId = null,
                ProjectId = scriptElement.ProjectId,
                Line = scriptElement.FountainRawData,
                SequenceNumber = scriptElement.SequenceNumber,
                LineType = type,
                RecordingCount = 0
            };

            return sl;
        }

        public IEnumerable<ScriptLine> GetPreviousScriptLinesByCharacter(
            Guid userPersonId,
            Guid projectId,
            Guid characterId,
            int currentScriptSequenceNumber,
            int limit)
        {
            Dictionary<Guid, UIScene> scenes = new Dictionary<Guid, UIScene>();

            if (HasAccessToProject(userPersonId, projectId))
            {
                var output = new List<ScriptLine>();
                var characterDialogs = _scriptService.GetPreviousCharacterDialogsByCharacter(characterId, currentScriptSequenceNumber, limit);

                foreach (var characterDialog in characterDialogs)
                {
                    var sl = BuildScriptLine(characterDialog, scenes);
                    output.Add(sl);
                }

                return output.OrderBy(sl => sl.SequenceNumber);
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<ScriptLine> GetScriptLineContext(
            Guid userPersonId,
            Guid projectId,
            Guid characterDialogId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                Guid sceneId = Guid.Empty;
                var focusCD = _scriptService.GetCharacterDialog(characterDialogId);
                if (focusCD.SceneId.HasValue)
                {
                    sceneId = focusCD.SceneId.Value;
                }

                var scene = _scriptService.GetScene(sceneId);
                
                int start = focusCD.ScriptSequenceNumber - 4;
                int end = focusCD.ScriptSequenceNumber + 4;
                
                var elements = _scriptService.GetScriptElements(projectId, start, end);

                var output = new List<ScriptLine>();

                foreach (var se in elements)
                {
                    if (se.Id.Equals(focusCD.ScriptElementId))
                    {
                        output.Add(BuildScriptLine(focusCD, null, se));
                    }
                    else
                    {
                        output.Add(BuildScriptLine(se));
                    }
                }

                return output.OrderBy(sl => sl.SequenceNumber);
            }

            throw new UnauthorizedAccessException();
        }

        public Guid? GetNextLineId(
            Guid userPersonId,
            Guid projectId,
            Guid characterDialogId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                Guid sceneId = Guid.Empty;
                var focusCd = _scriptService.GetCharacterDialog(characterDialogId);

                var next = _scriptService.GetNextCharacterDialogByCharacter(
                    focusCd.CharacterId,
                    focusCd.ScriptSequenceNumber);

                return next?.Id;
            }

            throw new UnauthorizedAccessException();
        }

        public Guid? GetPreviousLineId(
            Guid userPersonId,
            Guid projectId,
            Guid characterDialogId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                Guid sceneId = Guid.Empty;
                var focusCd = _scriptService.GetCharacterDialog(characterDialogId);

                var previous = _scriptService.GetPreviousCharacterDialogByCharacter(
                    focusCd.CharacterId,
                    focusCd.ScriptSequenceNumber);

                return previous?.Id;
            }

            throw new UnauthorizedAccessException();
        }

        public void SaveRecording(
            Guid userPersonId,
            Guid projectId,
            Guid characterDialogId,
            Stream data)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var characterDialog = _scriptService.GetCharacterDialog(characterDialogId);

                var recordings = _scriptService
                    .GetRecordings(characterDialogId)
                    .ToList();

                var recording = new Recording
                {
                    ProjectId = projectId,
                    CharacterDialogId = characterDialogId,
                    PerformerPersonId = userPersonId,
                    RecordedOn = DateTime.UtcNow,
                    SequenceNumber = recordings.Count + 1
                };

                _scriptService.Save(recording);

                string tempPath = _fileStorage.ToPath(
                    projectId.ToString(),
                    "recordings",
                    $"{recording.Id}.wav");
                _fileStorage.SaveFile(tempPath, data);

                return;
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<Recording> GetRecordings(
            Guid userPersonId,
            Guid projectId,
            Guid characterDialogId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                return _scriptService.GetRecordings(characterDialogId);
            }

            throw new UnauthorizedAccessException();
        }

        public Stream GetRecording(
            Guid userPersonId,
            Guid projectId,
            Guid recordingId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                string tempPath = _fileStorage.ToPath(
                    projectId.ToString(),
                    "recordings",
                    $"{recordingId.ToString()}.wav"
                );

                return _fileStorage.GetFile(tempPath);
            }

            throw new UnauthorizedAccessException();
        }

        public void DeleteRecording(
            Guid userPersonId,
            Guid projectId,
            Guid recordingId)
        {
            if (HasAccessToProject(userPersonId, projectId))
            {
                var recording = _scriptService.GetRecording(recordingId);

                if (recording.PerformerPersonId.Equals(userPersonId)
                    || IsProjectOwner(userPersonId, projectId))
                {
                    _scriptService.DeleteRecording(recordingId);
                    _scriptService.ResequenceRecordings(recording.CharacterDialogId);
                }
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }

        public void SaveUser(
            Guid userPersonId,
            UserEdit userEdit)
        {
            var actingUser = _serverService.GetUserByPerson(userPersonId);

            if (IsSystemAdmin(actingUser.Id))
            {
                var user = _serverService.GetUser(userEdit.UserId);

                if (user == null)
                {
                    user = new User
                    {
                        Id = userEdit.UserId,
                        PersonId = userEdit.PersonId
                    };
                }

                user.UserType = userEdit.UserType;
                user.Email = userEdit.Email;
                _serverService.Save(user);

                var person = _scriptService.GetPerson(userEdit.PersonId);
                if (person == null)
                {
                    person = new Person
                    {
                        Id = userEdit.PersonId
                    };
                }
                
                person.FamilyName = userEdit.FamilyName;
                person.GivenName = userEdit.GivenName;
                person.PhoneNumber = userEdit.PhoneNumber;
                _scriptService.Save(person);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }

        public void SetPassword(
            Guid userPersonId,
            Guid userId,
            string newPassword)
        {
            var actingUser = _serverService.GetUserByPerson(userPersonId);

            if (IsSystemAdmin(actingUser.Id)
                || userId == actingUser.Id)
            {
                _serverService.SetPassword(userId, newPassword);
                return;
            }
            
            throw new UnauthorizedAccessException();
        }

        public IEnumerable<User> GetUsers(
            Guid userPersonId)
        {
            var actingUser = _serverService.GetUserByPerson(userPersonId);

            if (IsSystemAdmin(actingUser.Id))
            {
                return _serverService.GetUsers();
            }

            throw new UnauthorizedAccessException();
        }

        public IEnumerable<UserEdit> GetUserEdits(
            Guid userPersonId)
        {
            var actingUser = _serverService.GetUserByPerson(userPersonId);

            if (IsSystemAdmin(actingUser.Id))
            {
                var users = _serverService.GetUsers();

                List<UserEdit> output = new List<UserEdit>();
                
                foreach (var user in users)
                {
                    var userEdit = new UserEdit
                    {
                        Email = user.Email, 
                        UserId = user.Id, 
                        PersonId = user.PersonId, 
                        UserType = user.UserType
                    };

                    var person = _scriptService.GetPerson(user.PersonId);
                    if (person != null)
                    {
                        userEdit.FamilyName = person.FamilyName;
                        userEdit.GivenName = person.GivenName;
                        userEdit.PhoneNumber = person.PhoneNumber;
                    }

                    output.Add(userEdit);
                }

                return output;
            }

            throw new UnauthorizedAccessException();
        }
        
        public UserEdit GetUserEdit(
            Guid userId,
            Guid userPersonId)
        {
            var actingUser = _serverService.GetUserByPerson(userPersonId);

            if (IsSystemAdmin(actingUser.Id))
            {
                var user = _serverService.GetUser(userId);
                var userEdit = new UserEdit
                {
                    Email = user.Email, 
                    UserId = user.Id, 
                    PersonId = user.PersonId, 
                    UserType = user.UserType
                };

                var person = _scriptService.GetPerson(user.PersonId);
                if (person != null)
                {
                    userEdit.FamilyName = person.FamilyName;
                    userEdit.GivenName = person.GivenName;
                    userEdit.PhoneNumber = person.PhoneNumber;
                }

                return userEdit;
            }

            throw new UnauthorizedAccessException();
        }
    }
}
