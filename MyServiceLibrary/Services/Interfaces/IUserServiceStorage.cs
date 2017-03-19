using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Interfaces
{
    public interface IUserServiceStorage
    {
        void Add(User user);
        void Delete(User user);
        IEnumerable<User> SearchByPredicate(Func<User, bool> searchEngine);
        void Save();
        void Unload();
    }
}
