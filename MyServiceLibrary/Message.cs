using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary
{
    [Serializable]
    public class Message
    {
        public string MethodImplamentation { get; }
        public User User { get; }
        public IEnumerable<User> AllUsers { get; }

        public Message(string methodImplamentation, User user, IEnumerable<User> allUsers)
        {
            MethodImplamentation = methodImplamentation;
            User = user;
            AllUsers = allUsers;
        }
    }
}
