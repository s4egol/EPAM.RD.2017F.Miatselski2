using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyServiceLibrary.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace MyServiceLibrary.Services
{
    public class SlaveUserService : MarshalByRefObject, IUserServiceStorage
    {
        private List<User> users = new List<User>();

        private static readonly TraceSource traceSource = new TraceSource("CustomTraceSource");

        private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

        private string ServiceIp { get; set; }

        private int ServicePort { get; set; }

        private TcpListener tcpListener;

        public SlaveUserService()
        {

        }

        /// <summary>
        /// Initialize of slave service
        /// </summary>
        /// <param name="serviceIp">Ip of slave service</param>
        /// <param name="servicePort">Port of slave service</param>
        public SlaveUserService(string serviceIp, int servicePort)
        {
            if (string.IsNullOrEmpty(serviceIp))
            {
                traceSource.TraceInformation($"Argument null exception in constructor at SlaveService");
                throw new ArgumentNullException();
            }
            ServiceIp = serviceIp;
            ServicePort = servicePort;
        }

        /// <summary>
        /// Create new slave service in new domain
        /// </summary>
        /// <param name="domainName">Name of domain</param>
        /// <returns>New slave service in new domain</returns>
        public SlaveUserService CreateSlaveServiceInNewDomain(string domainName)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                traceSource.TraceInformation($"Argument null exception in CreateSlaveServiceInNewDomain request at SlaveService");
                throw new ArgumentNullException();
            }
            var result = (SlaveUserService)AppDomain.CreateDomain(domainName)
                .CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(SlaveUserService).FullName);
            result.ServiceIp = this.ServiceIp;
            result.ServicePort = this.ServicePort;
            result.StartWork();
            return result;
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
                traceSource.TraceInformation($"Argument null exception in SearchByPredicate request at SlaveService");
                throw new ArgumentNullException();
            }
            readerWriterLockSlim.EnterReadLock();
            try
            {
                traceSource.TraceInformation($"SearchByPredicate request in SlaveService at {DateTime.Now}");
                return users.Where(user => searchEngine.Invoke(user)).ToList();
            }
            finally
            {
                readerWriterLockSlim.ExitReadLock();
            }
        }

        /// <summary>
        /// Add new user in repository
        /// </summary>
        /// <param name="user">New user</param>
        private void AddedUser(User user)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                users.Add(user);
                traceSource.TraceInformation($"AddedUser request in SlaveService at {DateTime.Now} in {AppDomain.CurrentDomain.FriendlyName}");
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Remove user from storage
        /// </summary>
        /// <param name="user">Deleted user</param>
        private void DeletedUser(User user)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                users.Remove(users.FirstOrDefault(x => x.FirstName == user.FirstName &&
                                                       x.LastName == user.LastName && x.DateOfBirth == user.DateOfBirth));
                traceSource.TraceInformation($"DeletedUser request in SlaveService at {DateTime.Now} in {AppDomain.CurrentDomain.FriendlyName}");
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// Update datastore
        /// </summary>
        /// <param name="allUsers">All added users</param>
        private void UpdatedData(IEnumerable<User> allUsers)
        {
            readerWriterLockSlim.EnterWriteLock();
            try
            {
                allUsers.ToList().ForEach(x => users.Add(new User() {Id = x.Id, DateOfBirth = x.DateOfBirth, FirstName = x.FirstName, LastName = x.LastName}));
                traceSource.TraceInformation($"UpdatedData request in SlaveService at {DateTime.Now} in {AppDomain.CurrentDomain.FriendlyName}");
            }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        private void StartWork()
        {
            tcpListener = new TcpListener(IPAddress.Parse(ServiceIp), ServicePort);
            StartListener();
        }

        /// <summary>
        /// Waiting for notifications from Master
        /// </summary>
        private void StartListener()
        {
            ThreadPool.QueueUserWorkItem(async (val) =>
            {
                tcpListener.Start();

                while (true)
                {
                    TcpClient client = null;
                    try
                    {
                        client = await tcpListener.AcceptTcpClientAsync();
                        NetworkStream networkStream = client.GetStream();
                        Message message = await GetMessage(networkStream);
                        if (message.MethodImplamentation == "AddedUser")
                            AddedUser(message.User);
                        if (message.MethodImplamentation == "DeletedUser")
                            DeletedUser(message.User);
                        if (message.MethodImplamentation == "UpdatedData")
                            UpdatedData(message.AllUsers);
                    }
                    finally
                    {
                        client?.Close();
                    }
                }
            });
        }

        private Task<Message> GetMessage(Stream stream)
        {
            var formatter = new BinaryFormatter();
            return Task.FromResult(formatter.Deserialize(stream) as Message);
        }

        #region NotImplemented
        public void Add(User user)
        {
            throw new NotImplementedException();
        }

        public void Delete(User user)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
