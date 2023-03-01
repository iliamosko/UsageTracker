using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Entities
{
    internal class User
    {
        int id;
        string firstName;
        string lastName;
        string email;
        string password;
        string passwordHash;

        public User(int id, string firstName, string lastName, string email, string password)
        {
            this.id = id;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.password = password;
        }


    }
}
