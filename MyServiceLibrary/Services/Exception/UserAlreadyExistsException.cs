using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Exception
{
    public class UserAlreadyExistsException : System.Exception
    {
        public UserAlreadyExistsException(string message) : base(message)
        {
            
        }
    }
}
