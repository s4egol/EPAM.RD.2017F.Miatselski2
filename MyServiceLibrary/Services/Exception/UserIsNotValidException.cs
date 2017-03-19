using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceLibrary.Exception
{
    public class UserIsNotValidException : System.Exception
    {
        public UserIsNotValidException(string message) : base(message)
        {
            
        }
    }
}
