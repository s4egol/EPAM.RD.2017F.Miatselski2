using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyServiceLibrary.Exception;

namespace MyServiceLibrary.Tests
{
    [TestClass]
    public class MyServiceTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullUser_ExceptionThrown()
        {
            MasterUserService service = new MasterUserService();
            service.Add(null);
        }

        [TestMethod]
        [ExpectedException(typeof(UserIsNotValidException))]
        public void Add_NullFirstnameUser_ExceptionThrown()
        {
            MasterUserService service = new MasterUserService();
            var user = new User() {LastName = "Metelsky", DateOfBirth = DateTime.Now};

            service.Add(user);
        }

        [TestMethod]
        [ExpectedException(typeof(UserIsNotValidException))]
        public void Add_NullLastnameUser_ExceptionThrown()
        {
            MasterUserService service = new MasterUserService();
            var user = new User() { FirstName = "Ivan", DateOfBirth = DateTime.Now };

            service.Add(user);
        }

        [TestMethod]
        [ExpectedException(typeof(UserAlreadyExistsException))]
        public void Add_ExistsUser_ExceptionThrown()
        {
            MasterUserService service = new MasterUserService();
            var user = new User() { FirstName = "Ivan", LastName = "Metelsky", DateOfBirth = DateTime.Now };

            service.Add(user);
            service.Add(user);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Delete_NullUser_ExceptionThrown()
        {
            MasterUserService service = new MasterUserService();
            service.Delete(null);
        }

        [TestMethod]
        public void SearchByPredicate_UserExists_User()
        {
            MasterUserService service = new MasterUserService();
            var user = new User() { FirstName = "Ivan", LastName = "Metelsky", DateOfBirth = DateTime.Now };

            service.Add(user);
            var listUsers = service.SearchByPredicate((x) =>
            {
                if (x == user)
                    return true;
                return false;
            });

            Assert.AreEqual(listUsers.First(), user);
        }

        [TestMethod]
        public void SearchByPredicate_UserNotFound_EmptyCollection()
        {
            MasterUserService service = new MasterUserService();
            var user = new User() { FirstName = "Ivan", LastName = "Metelsky", DateOfBirth = DateTime.Now };

            service.Add(user);
            var listUsers = service.SearchByPredicate((x) =>
            {
                if (x == new User())
                    return true;
                return false;
            });

            Assert.AreEqual(listUsers.Count(), 0);
        }
    }
}
