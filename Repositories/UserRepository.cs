using Diplom.Entities;
using Diplom.Interfacec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationContext _dbContext;

        public UserRepository(ApplicationContext databaseOptions)
        {
            _dbContext = databaseOptions;
        }

        public void CreateUser(User user)
        {
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        public List<User> GetAllUsers()
        {
            return _dbContext.Users.ToList();
        }
        public User GetUserByEmail(string email)
        {
            return _dbContext.Users.FirstOrDefault(e => e.Email == email);
        }


    }
}
