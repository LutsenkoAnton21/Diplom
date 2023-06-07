using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Diplom.Entities
{
    public class User : IdentityUser
    {
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        //public DateTime DOB { get; set; }

    }
}
