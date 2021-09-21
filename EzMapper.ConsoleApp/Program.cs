using System;
using System.Linq;
using System.Reflection;
using EzMapper.ConsoleApp.Models;

namespace EzMapper.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var person = new Person() { ID = 1, Age = 18, FirstName = "John", LastName = "Doe" };
            var student = new Student() { ID = 2, Age = 19, FirstName = "Jane", LastName = "Doe", School = "FH Technikum" };

            Class1.TestCrud(student);

            var props = student.GetType().GetProperties().ToList();
            props.ForEach(prop => Console.WriteLine($"{prop.PropertyType} {prop.Name} {{ {prop.GetGetMethod()}; {prop.GetSetMethod()}; }} {prop.CustomAttributes.FirstOrDefault()}"));

            Console.WriteLine();
            Console.WriteLine(person.GetType().BaseType);
            Console.WriteLine(student.GetType().BaseType);
        }
    }


}
