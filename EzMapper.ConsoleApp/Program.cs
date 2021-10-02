using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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

            var p1 = new Person() { ID = 1, Age = 18, FirstName = "John", LastName = "Doe" };
            var p2 = new Student() { ID = 2, Age = 19, FirstName = "Jane", LastName = "Doe", School = "FH Technikum" };
            var p3 = new Teacher() { ID = 2, Age = 19, FirstName = "Jane", LastName = "Doe", WorkingYears = 4 };

            var course = new Course();

            //int[] intarr = new int[3];
            //Student[] objarr = new Student[3];

            //List<int> intlist = new();
            //List<Student> objlist = new();



            //Console.WriteLine(Class1.IsPrimitive(course, nameof(course.Students)));
            //Console.WriteLine(intarr.GetType().GetElementType());
            //Console.WriteLine(objarr.GetType().GetElementType());
            //Console.WriteLine(intlist.GetType().GenericTypeArguments[0]);
            //Console.WriteLine(objlist.GetType().GenericTypeArguments[0]);

            dynamic obj = new ExpandoObject();
            obj.X = 0;

            Console.WriteLine(obj.GetType().GetProperties().Length);
            //var x = Class1.GetParentModel(student);

            //Class1.TestCrud(student);

            //var props = student.GetType().GetProperties().ToList(); // get all properties (including inherited once)
            //var props = student.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList(); // get all properties that were declared in this specific type (ignore inherited ones)
            //props.ForEach(prop => Console.WriteLine($"{prop.PropertyType} {prop.Name} {{ {prop.GetGetMethod()}; {prop.GetSetMethod()}; }} {prop.CustomAttributes.FirstOrDefault()}; Primitve: {Class1.IsPrimitive(student, prop.Name)}"));



            Console.WriteLine();
            //Class1.TestInheritance(student);
            //Class1.TestInheritance(person);

            //File.WriteAllText("test.txt", Class1.CreateTable(p2));

            string[] statements = EzMapper.CreateTable(p2);

            foreach (string statement in statements)
                Console.WriteLine(statement);
            EzMapper.printForeignKeys();
            //Console.WriteLine(Class1.HasAttribute<DefaultValueAttribute>(person.GetType().GetProperty("Age")));



        }

    }


}
