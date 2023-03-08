using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsageTracker.Interfaces;

namespace TimeTracker.Entities
{
    public class User : IUser
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

        public int GetId()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public string GetLastName()
        {
            throw new NotImplementedException();
        }

        public string GetEmail()
        {
            return email;
        }

        public string GetPassword()
        {
           return password;
        }
    }
}
