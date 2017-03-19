using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServiceLibrary.Repository.Interfaces;
using System.Configuration;
using System.Xml.Serialization;
using MyServiceLibrary.Entities;
using System.IO;
using System.Collections;

namespace MyServiceLibrary.Repository
{
    [Serializable]
    public class UserRepository : IRepository<User>
    {
        private List<User> listUsers;

        private string pathToXml;// = "RepositoryXML.xml";

        public UserRepository(string pathToXml)
        {
            listUsers = new List<User>();
            this.pathToXml = pathToXml;
        }
             
        public long Count => listUsers.Count;

        public void Add(User item)
        {
            listUsers.Add(item);
        }

        public bool Contains(User item)
        {
            var foundEntity =
                listUsers.Where(
                    x =>
                        x.FirstName == item.FirstName && x.LastName == item.LastName &&
                        x.DateOfBirth == item.DateOfBirth);

            return foundEntity.Count() != 0;
        }

        public void Delete(User item)
        {
            listUsers.Remove(listUsers.FirstOrDefault(x =>
                x.FirstName == item.FirstName && x.LastName == item.LastName &&
                x.DateOfBirth == item.DateOfBirth));
        }

        public IEnumerable<User> SearchByPredicate(Func<User, bool> searchEngine)
        {
            return listUsers.Where(user => searchEngine.Invoke(user)).ToList();
        }

        public void SaveToXML(int id)
        {
            var formatter = new XmlSerializer(typeof(StateOfService));

            using (var fileStream = new FileStream(pathToXml, FileMode.Create))
            {
                formatter.Serialize(fileStream, new StateOfService() { GeneratorId = id, ListOfUsers = listUsers});
            }
        }

        public int UnloadFromXML()
        {
            int returnId;
            var formatter = new XmlSerializer(typeof(StateOfService));

            using (var fileStream = new FileStream(pathToXml, FileMode.Open))
            {
                var state = (StateOfService)formatter.Deserialize(fileStream);
                listUsers = new List<User>(state.ListOfUsers);
                returnId = state.GeneratorId;
            }

            return returnId;
        }

        public IEnumerator<User> GetEnumerator()
        {
            return listUsers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
