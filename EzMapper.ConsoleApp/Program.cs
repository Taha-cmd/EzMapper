using System;
using System.Reflection;
using EzMapper.ConsoleApp.Models;

namespace EzMapper.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var person = new Person() { ID = 1, Age = 18, FirstName = "John", LastName = "Doe" };

            Class1.Test(person);
        }
    }


}
