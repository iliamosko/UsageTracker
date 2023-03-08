using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsageTracker.Interfaces
{
    internal interface IUser
    {
        public int GetId();

        public string GetName();

        public string GetLastName();

        public string GetEmail();

        public string GetPassword();
    }
}
