using Diplom.Entities;
using Diplom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Interfacec
{
    interface IUserRepository
    {
        void CreateUser(User user);
        public List<User> GetAllUsers();
        User GetUserByEmail(string email);
    }
}
