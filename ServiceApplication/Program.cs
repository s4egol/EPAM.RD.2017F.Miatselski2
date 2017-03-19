using System;
using MyServiceLibrary;
using MyServiceLibrary.Services;
using System.Collections.Generic;
using System.Configuration;

namespace ServiceApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var appSettings = ConfigurationManager.AppSettings;
            var myappSet = (dynamic)ConfigurationManager.GetSection("Slaves");

            List<KeyValuePair<int, string>> PortsAndHosts = new List<KeyValuePair<int, string>>();
            for (int i = 0; i < Int32.Parse(myappSet["slavesNum"]); i++)
            {
                var temp = myappSet[$"Slave{i}"].Split(' ');
                PortsAndHosts.Add(new KeyValuePair<int, string>(int.Parse(temp[1]), temp[0]));
            }
            var slaveServices = new List<SlaveUserService>();
            int counter = 0;
            foreach (var slaveSettings in PortsAndHosts)
            {
                var slave = new SlaveUserService(slaveSettings.Value, slaveSettings.Key);
                slaveServices.Add(slave.CreateSlaveServiceInNewDomain($"Slave{counter}"));
                counter++;
            }
            Func<User, bool> searchEngine1 = SearchByPredicate;
            var service = new MasterUserService(PortsAndHosts, ConfigurationManager.AppSettings["pathToXML"]);
            var master = service.CreateMasterServiceInNewDomain("Master");
            master.Unload();

            Func<User, bool> searchEngine = SearchByPredicate;
            var foundValues = master.SearchByPredicate(searchEngine);
            var foundValues2 = slaveServices[0].SearchByPredicate(searchEngine);
            Console.ReadKey();
        }

        private static bool SearchByPredicate(User user)
        {
            if (user.FirstName == "Ivan")
                return true;
            return false;
        }
    }
}
