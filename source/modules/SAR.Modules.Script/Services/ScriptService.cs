using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAR.Modules.Script.Objects;
using SAR.Modules.Script.Repos;
using SAR.Modules.Server.Repos;

namespace SAR.Modules.Script.Services
{
    public class ScriptService
    {
        private readonly CharacterDialogRepo _characterDialogRepo;
        private readonly CharacterRepo _characterRepo;
        private readonly PersonRepo _personRepo;
        private readonly ProjectAccessRepo _projectAccessRepo;
        private readonly ProjectRepo _projectRepo;
        private readonly RecordingRepo _recordingRepo;
        private readonly SceneRepo _sceneRepo;
        private readonly ScriptElementRepo _scriptElementRepo;

        public ScriptService(
            CharacterRepo characterRepo,
            CharacterDialogRepo characterDialogRepo,
            PersonRepo personRepo,
            ProjectAccessRepo projectAccessRepo,
            ProjectRepo projectRepo,
            RecordingRepo recordingRepo,
            SceneRepo sceneRepo,
            ScriptElementRepo scriptElementRepo)
        {
            _characterRepo = characterRepo;
            _characterDialogRepo = characterDialogRepo;
            _personRepo = personRepo;
            _projectAccessRepo = projectAccessRepo;
            _projectRepo = projectRepo;
            _recordingRepo = recordingRepo;
            _sceneRepo = sceneRepo;
            _scriptElementRepo = scriptElementRepo;
        }

        public Person GetPerson(Guid personId)
        {
            return _personRepo.GetById(personId);
        }

        public IEnumerable<Person> GetPeople()
        {
            return _personRepo.GetAll();
        }

        public IEnumerable<Project> GetProjectsByPerson(Guid personId)
        {
            Dictionary<Guid, Project> projects = new Dictionary<Guid, Project>();

            var access = _projectAccessRepo
                .GetByPerson(personId)
                .ToList();

            foreach (var projectAccess in access)
            {
                if (!projects.ContainsKey(projectAccess.ProjectId))
                {
                    var project = _projectRepo.GetById(projectAccess.ProjectId);

                    projects.Add(project.Id, project);
                }
            }

            return projects.Values.ToList();
        }

        public IEnumerable<ProjectAccess> GetAccessRights(Guid personId)
        {
            Dictionary<Guid, ProjectAccess> projectAccesses =
                _projectAccessRepo
                    .GetByPerson(personId)
                    .ToDictionary(pa => pa.ProjectId);

            return projectAccesses.Values.ToList();
        }

        public bool HasAccessToProject(Guid personId, Guid projectId)
        {
            var access = _projectAccessRepo.Get(personId, projectId);

            if (access != null)
            {
                return true;
            }

            return false;
        }

        public Project GetProject(Guid projectId)
        {
            return _projectRepo.GetById(projectId);
        }

        public ProjectAccess GetProjectAccess(Guid personId, Guid projectId)
        {
            return _projectAccessRepo.Get(personId, projectId);
        }

        public IEnumerable<ProjectAccess> GetProjectAccessByProject(Guid projectId)
        {
            return _projectAccessRepo.GetByProject(projectId);
        }
        
        public void Save(Project project)
        {
            _projectRepo.Save(project);
        }

        public void Save(Person person)
        {
            _personRepo.Save(person);
        }

        public void Save(ProjectAccess projectAccess)
        {
            _projectAccessRepo.Save(projectAccess);
        }

        public void Save(ScriptElement element)
        {
            _scriptElementRepo.Save(element);
        }

        public void Save(Recording recording)
        {
            _recordingRepo.Save(recording);
        }

        public IEnumerable<Recording> GetRecordings(Guid characterDialogId)
        {
            return _recordingRepo.GetByCharacterDialog(characterDialogId);
        }

        public Recording GetRecording(Guid recordingId)
        {
            return _recordingRepo.GetById(recordingId);
        }

        public ScriptElement GetScriptElement(Guid elementId)
        {
            return _scriptElementRepo.GetById(elementId);
        }

        public IEnumerable<ScriptElement> GetScriptElements(Guid projectId, int? start, int? end)
        {
            return _scriptElementRepo.GetByProject(projectId, start, end);
        }

        public void Save(Character character)
        {
            _characterRepo.Save(character);
        }

        public IEnumerable<Character> GetCharactersByProject(Guid projectId)
        {
            return _characterRepo.GetByProject(projectId);
        }

        public Character GetCharacter(Guid characterId)
        {
            return _characterRepo.GetById(characterId);
        }

        public void DeleteScriptElement(Guid elementId)
        {
            _scriptElementRepo.Delete(elementId);
        }

        public void DeleteCharacter(Guid characterId)
        {
            _characterRepo.Delete(characterId);
        }

        public void DeleteProjectAccess(Guid projectAccessId)
        {
            _projectAccessRepo.Delete(projectAccessId);
        }

        public void DeleteProject(Guid projectId)
        {
            _projectRepo.Delete(projectId);
        }

        public void DeleteCharactersByProject(Guid projectId)
        {
            _characterRepo.DeleteByProject(projectId);
        }

        public void DeleteProjectAccessByProject(Guid projectId)
        {
            _projectAccessRepo.DeleteByProject(projectId);
        }

        public void DeleteScriptElementsByProject(Guid projectId)
        {
            _scriptElementRepo.DeleteByProject(projectId);
        }

        public void DeleteCharacterDialogByProject(Guid projectId)
        {
            _characterDialogRepo.DeleteByProject(projectId);
        }

        public void DeleteScenesByProject(Guid projectId)
        {
            _sceneRepo.DeleteByProject(projectId);
        }

        public void DeleteRecordingsByProject(Guid projectId)
        {
            _recordingRepo.DeleteByProject(projectId);
        }

        public void DeleteRecording(Guid recordingId)
        {
            _recordingRepo.Delete(recordingId);
        }

        public void ResequenceRecordings(Guid characterDialogId)
        {
            var recordings = _recordingRepo.GetByCharacterDialog(characterDialogId).ToList();

            int i = 1;
            foreach (var recording in recordings)
            {
                recording.SequenceNumber = i;

                _recordingRepo.Save(recording);
                i++;
            }
        }

        public void DeleteProjectScript(Guid projectId)
        {
            //Delete Elements for the scene
            DeleteScriptElementsByProject(projectId);

            //Delete All Character Dialog Links for project
            DeleteCharacterDialogByProject(projectId);

            //Delete All Characters
            DeleteCharactersByProject(projectId);

            //Delete All Scenes
            DeleteScenesByProject(projectId);

            //Delete all recordings
            DeleteRecordingsByProject(projectId);
        }

        public void Save(CharacterDialog characterDialog)
        {
            _characterDialogRepo.Save(characterDialog);
        }

        public CharacterDialog GetCharacterDialog(Guid characterDialogId)
        {
            return _characterDialogRepo.GetById(characterDialogId);
        }

        public IEnumerable<CharacterDialog> GetCharacterDialogsByCharacter(Guid characterId)
        {
            return _characterDialogRepo.GetByCharacter(characterId);
        }

        public CharacterDialog GetNextCharacterDialogByCharacter(
            Guid characterId, 
            int currentScriptSequenceNumber)
        {
            return _characterDialogRepo
                .GetNextByCharacter(characterId, currentScriptSequenceNumber)
                .FirstOrDefault();
        }

        public CharacterDialog GetPreviousCharacterDialogByCharacter(
            Guid characterId, 
            int currentScriptSequenceNumber)
        {
            return _characterDialogRepo
                .GetPreviousByCharacter(characterId, currentScriptSequenceNumber)
                .FirstOrDefault();
        }

        public IEnumerable<CharacterDialog> GetNextCharacterDialogsByCharacter(
            Guid characterId,
            int currentScriptSequenceNumber,
            int limit)
        {
            return _characterDialogRepo.GetNextByCharacter(characterId, currentScriptSequenceNumber, limit);
        }

        public IEnumerable<CharacterDialog> GetPreviousCharacterDialogsByCharacter(
            Guid characterId, 
            int currentScriptSequenceNumber,
            int limit)
        {
            return _characterDialogRepo.GetPreviousByCharacter(characterId, currentScriptSequenceNumber, limit);
        }

        public void Save(Scene scene)
        {
            _sceneRepo.Save(scene);
        }

        public IEnumerable<Scene> GetScenesByProject(Guid projectId)
        {
            return _sceneRepo.GetByProject(projectId);
        }

        public Scene GetSceneByProjectNumber(Guid projectId, int sceneNumber)
        {
            return _sceneRepo.GetByProjectNumber(projectId, sceneNumber);
        }

        public Scene GetScene(Guid sceneId)
        {
            return _sceneRepo.GetById(sceneId);
        }
    }
}
