using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Repository.Interfaces
{
    public interface IRepository<T> : IEnumerable<T>
    {
        void Add(T item);
        void Delete(T item);
        IEnumerable<T> SearchByPredicate(Func<User, bool> searchEngine);
        bool Contains(T item);
        long Count { get; }
        void SaveToXML(int id);
        int UnloadFromXML();
    }
}
