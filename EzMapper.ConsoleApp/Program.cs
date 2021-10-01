using System;
using System.Linq;
using System.Reflection;
using EzMapper.Attributes;
using EzMapper.ConsoleApp.Models;

namespace EzMapper.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var wewewe = Nullable.GetUnderlyingType(typeof(string));

            var person = new Person() { ID = 1, Age = 18, FirstName = "John", LastName = "Doe" };
            var student = new Student() { ID = 2, Age = 19, FirstName = "Jane", LastName = "Doe", School = "FH Technikum" };

            //var x = Class1.GetParentModel(student);

            //Class1.TestCrud(student);

            //var props = student.GetType().GetProperties().ToList(); // get all properties (including inherited once)
            //var props = student.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList(); // get all properties that were declared in this specific type (ignore inherited ones)
            //props.ForEach(prop => Console.WriteLine($"{prop.PropertyType} {prop.Name} {{ {prop.GetGetMethod()}; {prop.GetSetMethod()}; }} {prop.CustomAttributes.FirstOrDefault()}; Primitve: {Class1.IsPrimitive(student, prop.Name)}"));



            Console.WriteLine();
            //Class1.TestInheritance(student);
            //Class1.TestInheritance(person);

            Console.WriteLine(Class1.CreateTable(student));
            Class1.printForeignKeys();
            //Console.WriteLine(Class1.HasAttribute<DefaultValueAttribute>(person.GetType().GetProperty("Age")));



        }

    }


}
