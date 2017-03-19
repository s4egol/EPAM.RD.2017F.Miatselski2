using System;
using System.Collections;
using System.Collections.Generic;

namespace MyServiceLibrary
{
    // Rename this class. Give the class an appropriate name that will allow all other developers understand it's purpose.
    public class UserStorageService
    {
        private readonly List<User> usersList;

        public UserStorageService()
        {
            usersList = new List<User>();
        } 

        public void Add(User user)
        {
            if (user == null)
                throw new ArgumentNullException();
            if (String.IsNullOrEmpty(user.FirstName) || String.IsNullOrEmpty(user.LastName))
                throw new ArgumentNullException();
        }

        public void Delete(User user)
        {

        }

        public IEnumerable<User> SearchByPredicate(Predicate<User> predicate)
        {
            List<User> lists = new List<User>();
            return lists;
        }

        private bool ValidateUser(User user)
        {

        }
    }
}
