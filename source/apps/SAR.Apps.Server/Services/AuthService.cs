using System;
using SAR.Apps.Server.Objects;
using SAR.Libraries.Common.Helpers;
using SAR.Libraries.Common.Interfaces;
using SAR.Modules.Script.Constants;
using SAR.Modules.Script.Objects;
using SAR.Modules.Script.Services;
using SAR.Modules.Server.Constants;
using SAR.Modules.Server.Objects;
using SAR.Modules.Server.Services;

namespace SAR.Apps.Server.Services
{
    public class AuthService
    {
        private readonly ServerService _serverService;
        private readonly ScriptService _scriptService;
        private readonly ISarLogger _logger;

        public AuthService(
            ServerService serverService,
            ScriptService scriptService,
            ISarLogger logger)
        {
            _serverService = serverService;
            _scriptService = scriptService;
            _logger = logger;
        }

        public IWebSession CreateSession(string email, string password)
        {
            var user = _serverService.AuthenticateUser(email, password);

            if (user != null)
            {
                var person = _scriptService.GetPerson(user.PersonId);

                if (person != null)
                {
                    var session = _serverService.CreateSession(user);

                    var webSession = new WebSession
                    {
                        Id = session.Id,
                        Email = user.Email,
                        FamilyName = person.FamilyName,
                        GivenName = person.GivenName,
                        PersonId = person.Id,
                        UserId = user.Id,
                        UserType = user.UserType
                    };

                    return webSession;
                }
            }

            return null;
        }

        public void CloseSession(Guid sessionId)
        {
            var session = _serverService.GetSession(sessionId);
            session.IsDeleted = true;
            _serverService.Save(session);
        }

        public bool IsSetup()
        {
            int userCount = _serverService.GetUserCount();

            if (userCount > 0)
            {
                return true;
            }

            return false;
        }

        public void Setup(
            string ownerEmail, 
            string ownerPassword, 
            string ownerGivenName,
            string ownerFamilyName,
            string initialProjectTitle)
        {
            Person p = new Person
            {
                FamilyName = ownerFamilyName,
                GivenName = ownerGivenName
            };

            User u = new User
            {
                Email = ownerEmail,
                PersonId = p.Id,
                UserType = UserTypes.Owner
            };

            var salt = PasswordHelper.GenerateSalt();

            PasswordHash ph = new PasswordHash
            {
                Salt = salt,
                UserId = u.Id,
                Hash = PasswordHelper.Hash(ownerPassword, salt)
            };

            Project proj = new Project
            {
                Title = initialProjectTitle
            };

            ProjectAccess pa = new ProjectAccess
            {
                ProjectId = proj.Id,
                PersonId = p.Id,
            };
            pa.AccessTypes.Add(AccessTypes.Owner);

            _scriptService.Save(p);    //Person
            _serverService.Save(u);    //User
            _serverService.Save(ph);   //PasswordHash
            _scriptService.Save(proj); //Project
            _scriptService.Save(pa);   //Project Access
        }

    }
}
