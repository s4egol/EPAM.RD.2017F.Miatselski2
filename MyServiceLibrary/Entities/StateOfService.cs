using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Entities
{
    public class StateOfService
    {
        public List<User> ListOfUsers { get; set; }
        public int GeneratorId { get; set; }
    }
}
