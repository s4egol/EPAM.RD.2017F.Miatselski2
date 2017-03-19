using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using MyServiceLibrary.Exception;
using MyServiceLibrary.Interfaces;
using MyServiceLibrary.Repository;
using MyServiceLibrary.Repository.Interfaces;

namespace MyServiceLibrary
{
    // Rename this class. Give the class an appropriate name that will allow all other developers understand it's purpose.
    public class MasterUserService : MarshalByRefObject, IUserServiceStorage
    {
        public IRepository<User> listUsersRepository; 

        private Func<int, int> GeneratorId = (val) => val + 1;

        private int lastId = 0;

        private static readonly TraceSource traceSource = new TraceSource("CustomTraceSource");

        private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

        private List<KeyValuePair<int, string>> PortsAndHosts { get; set; }

        public IEnumerable<User> Users
        {
            get
            {
                var returnUsers = new List<User>();
                listUsersRepository.ToList().ForEach(x => returnUsers.Add(x));
                return returnUsers;
            }
        }

        /// <summary>
        /// Master service initialization
        /// </summary>
        /// <param name="portsAndHosts">Data about slave services</param>
        /// <param name="pathToRepository">Path to the repository</param>
        public MasterUserService(List<KeyValuePair<int, string>> portsAndHosts, string pathToRepository)
        {
            if (portsAndHosts == null || string.IsNullOrEmpty(pathToRepository))
            {
                traceSource.TraceInformation($"Argument null exception in constructor at MasterService");
                throw new ArgumentNullException();
            }
            PortsAndHosts = new List<KeyValuePair<int, string>>();
            portsAndHosts.ToList().ForEach(x => PortsAndHosts.Add(x));
            listUsersRepository = new UserRepository(pathToRepository);
        }

        public MasterUserService()
        {

        }

        /// <summary>
        /// Create new master service in new domain
        /// </summary>
        /// <param name="domainName">Name of domain</param>
        /// <returns>new master service</returns>
        public MasterUserService CreateMasterServiceInNewDomain(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                traceSource.TraceInformation($"Argument null exception in CreateMasterServiceInNewDomain request at MasterService");
                throw new ArgumentNullException();
            }
            var result = (MasterUserService)
                AppDomain.CreateDomain(domainName).CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                    typeof(MasterUserService).FullName);
            result.listUsersRepository = this.listUsersRepository;
            result.PortsAndHosts = this.PortsAndHosts;
            return result;
        }

        /// <summary>
        /// Add new user in repository
        /// </summary>
        /// <param name="user">New user</param>
        public void Add(User user)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                ValidateUser(user);
                if (listUsersRepository.Contains(user))
                {
                    traceSource.TraceInformation($"User already exists exception in Add request at MasterServicer");
                    throw new UserAlreadyExistsException("User already exists in storage");
                }
                user.Id = lastId = GeneratorId(lastId);

                traceSource.TraceInformation($"Add request in MasterService at {DateTime.Now}");
                listUsersRepository.Add(user);

                EditingData(new Message("AddedUser", user, null));
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Delete user from storage
        /// </summary>
        /// <param name="user">Deleted user</param>
        public void Delete(User user)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                ValidateUser(user);
                if (listUsersRepository.Contains(user))
                    listUsersRepository.Delete(user);

                traceSource.TraceInformation($"Delete request in MasterService at {DateTime.Now}");
                EditingData(new Message("DeletedUser", user, null));
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Search users in repository by predicate
        /// </summary>
        /// <param name="searchEngine">Function for search criteria</param>
        /// <returns>Found users</returns>
        public IEnumerable<User> SearchByPredicate(Func<User, bool> searchEngine)
        {
            if (searchEngine == null)
            {
                traceSource.TraceInformation($"Argument null exception in SearchByPredicate request at MasterService");
                throw new ArgumentNullException();
            }
            readerWriterLockSlim.EnterReadLock();
            try
            {
                traceSource.TraceInformation($"SearchByPredicate request in MasterService at {DateTime.Now}");
                return listUsersRepository.SearchByPredicate(searchEngine);
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        /// <summary>
        /// Save data to the disk
        /// </summary>
        public void Save()
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                traceSource.TraceInformation($"Save request in MasterService at {DateTime.Now}");
                listUsersRepository.SaveToXML(lastId);
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Unload data from the disk
        /// </summary>
        public void Unload()
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                traceSource.TraceInformation($"Unload request in MasterService at {DateTime.Now}");
                lastId = listUsersRepository.UnloadFromXML();

                EditingData(new Message("UpdatedData", null, listUsersRepository.ToList()));
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Check user for correctness
        /// </summary>
        /// <param name="user">Verifiable user</param>
        private void ValidateUser(User user)
        {
            if (user == null)
            {
                traceSource.TraceInformation($"Argument null exception in ChangeRepository in MasterService");
                throw new ArgumentNullException();
            }
            if (string.IsNullOrEmpty(user.FirstName) || (string.IsNullOrEmpty(user.LastName)) ||
                user.DateOfBirth == null)
            {
                traceSource.TraceInformation($"User is not valid exception in ChangeRepository in MasterService");
                throw new UserIsNotValidException("The user didn't pass validation!");
            }
        }

        /// <summary>
        /// Sending messages to slave services
        /// </summary>
        /// <param name="message">Event message</param>
        private async void EditingData(Message message)
        {
            var binaryFormatter = new BinaryFormatter();

            foreach (var slave in PortsAndHosts)
            {
                var client = new TcpClient();
                await client.ConnectAsync(slave.Value, slave.Key);

                using (NetworkStream netStream = client.GetStream())
                {
                    if (netStream.CanWrite)
                        binaryFormatter.Serialize(netStream, message);
                }

                client.Close();
            }
        }
    }
}
