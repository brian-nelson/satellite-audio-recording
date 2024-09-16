using System;
using System.Collections;
using System.Collections.Generic;
using SAR.Libraries.Common.Helpers;
using SAR.Modules.Server.Objects;
using SAR.Modules.Server.Repos;

namespace SAR.Modules.Server.Services
{
    public class ServerService
    {
        private readonly PasswordHashRepo _passwordHashRepo;
        private readonly UserRepo _userRepo;
        private readonly UserSessionRepo _userSessionRepo;

        public ServerService(
            PasswordHashRepo passwordHashRepo,
            UserRepo userRepo,
            UserSessionRepo userSessionRepo)
        {
            _passwordHashRepo = passwordHashRepo;
            _userRepo = userRepo;
            _userSessionRepo = userSessionRepo;
        }

        public User GetUserByEmail(string email)
        {
            var user = _userRepo.GetByEmail(email);
            return user;
        }

        public User GetUser(Guid userId)
        {
            var user = _userRepo.GetById(userId);
            return user;
        }

        public IEnumerable<User> GetUsers()
        {
            return _userRepo.GetAll();
        }

        public User GetUserByPerson(Guid personId)
        {
            var user = _userRepo.GetByPerson(personId);
            return user;
        }

        public User AuthenticateUser(string email, string password)
        {
            var user = _userRepo.GetByEmail(email);

            if (user != null)
            {
                var passwordHash = _passwordHashRepo.GetByUser(user.Id);

                if (passwordHash != null)
                {
                    var testHash = PasswordHelper.Hash(password, passwordHash.Salt);

                    if (testHash.Equals(passwordHash.Hash))
                    {
                        return user;
                    }
                }
            }

            return null;
        }

        public UserSession CreateSession(User user)
        {
            var now = DateTime.UtcNow;

            UserSession session = new UserSession
            {
                CreatedOn = now,
                ExpiresOn = now.AddDays(1),
                IsDeleted = false,
                UserId = user.Id
            };

            _userSessionRepo.Save(session);

            return session;
        }

        public UserSession GetSession(Guid userSessionId)
        {
            return _userSessionRepo.GetById(userSessionId);
        }

        public void Save(UserSession session)
        {
            _userSessionRepo.Save(session);
        }

        public void Save(User user)
        {
            _userRepo.Save(user);
        }

        public void SetPassword(Guid userId, string password)
        {
            var passwordHash = _passwordHashRepo.GetByUser(userId);

            if (passwordHash == null)
            {
                passwordHash = new PasswordHash
                {
                    UserId = userId
                };
            }

            passwordHash.Salt = PasswordHelper.GenerateSalt();
            passwordHash.Hash = PasswordHelper.Hash(password, passwordHash.Salt);

            _passwordHashRepo.Save(passwordHash);
        }

        public void Save(PasswordHash passwordHash)
        {
            _passwordHashRepo.Save(passwordHash);
        }

        public int GetUserCount()
        {
            return _userRepo.GetUserCount();
        }
    }
}
