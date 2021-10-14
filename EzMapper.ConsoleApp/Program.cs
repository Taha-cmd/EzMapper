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

            EzMapper.Register<Student>();
            EzMapper.Register<Teacher>();
            EzMapper.Build();

            Person student = new Student()
            {
                ID = 1,
                Age = 23,
                FirstName = "John",
                LastName = "Doe",
                School = "TU Wien",
                Numbers = new List<int>() { 1, 2, 3, 4, 5, 6 },
                Hobbies = new string[] { "Reading", "Skating", "Gaming" },
                Books = new List<Book>
                       {
                           new Book(){ ID = 1, Title = "learn C"},
                           new Book(){ ID = 2, Title = "Animal Farm"}
                       },
                Car = new Car() { ModelNumber = 3, Brand = "Volvo" },
                Laptop = new Laptop() { ID = 2, Brand = "Lenovo", CPU = new Cpu() { ID = 1, Brand = "Intel", Alu = new AluUnit() { ID = 2, PlaceHolder = "out of ideas" } } },
                Phones = new List<Phone>()
                {
                    new Phone() { ID = 1, Brand = "Samsung", CPU = new Cpu() { ID = 2, Brand = "Snapdragon", Alu = new AluUnit() { ID = 1, PlaceHolder = "no idea" } } },
                    new Phone() { ID = 2, Brand = "IPhone", CPU = new Cpu() { ID = 3, Brand = "apple fancy processor", Alu = new AluUnit() { ID = 3, PlaceHolder = "really no idea" } } },
                }
            };

            var c1 = new Course() { ID = 1, Name = "Math" };
            var c2 = new Course() { ID = 2, Name = "Programming" };
            var c3 = new Course() { ID = 3, Name = "Physics" };


            Person teacher1 = new Teacher()
            {
                ID = 2,
                Age = 42,
                Car = new Car() { ModelNumber = 2, Brand = "BMW" },
                FirstName = "Jane",
                LastName = "Doe",
                Hobbies = new string[] {"Reading", "Watching TV", "Swimming"},
                Numbers = new List<int>() { 1, 2, 3, 4, 5, 6 },
                WorkingYears = 15,
                Courses = new List<Course>() { c1, c2}
            };

            Person teacher2 = new Teacher()
            {
                ID = 3,
                Age = 46,
                Car = null,
                FirstName = "Jack",
                LastName = "Doe",
                Hobbies = new string[] { "Reading" },
                Numbers = new List<int>() { 1, 2, 3, 4, 5, 6 },
                WorkingYears = 5,
                Courses = new List<Course>() {  c2, c3 }
            };

            EzMapper.Save(student);
            EzMapper.Save(teacher1);
            EzMapper.Save(teacher2);






            //var wewewe = Nullable.GetUnderlyingType(typeof(string));

            //var p1 = new Person() { ID = 1, Age = 18, FirstName = "John", LastName = "Doe" };
            //var p2 = new Student() { ID = 2, Age = 19, FirstName = "Jane", LastName = "Doe", School = "FH Technikum" };
            //var p3 = new Teacher() { ID = 2, Age = 19, FirstName = "Jane", LastName = "Doe", WorkingYears = 4 };

            //var course = new Course();

            //int[] intarr = new int[3];
            //Student[] objarr = new Student[3];

            //List<int> intlist = new();
            //List<Student> objlist = new();



            //Console.WriteLine(Class1.IsPrimitive(course, nameof(course.Students)));
            //Console.WriteLine(intarr.GetType().GetElementType());
            //Console.WriteLine(objarr.GetType().GetElementType());
            //Console.WriteLine(intlist.GetType().GenericTypeArguments[0]);
            //Console.WriteLine(objlist.GetType().GenericTypeArguments[0]);

            //dynamic obj = new ExpandoObject();
            //obj.X = 0;

            //Console.WriteLine(obj.GetType().GetProperties().Length);
            ////var x = Class1.GetParentModel(student);

            ////Class1.TestCrud(student);

            ////var props = student.GetType().GetProperties().ToList(); // get all properties (including inherited once)
            ////var props = student.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList(); // get all properties that were declared in this specific type (ignore inherited ones)
            ////props.ForEach(prop => Console.WriteLine($"{prop.PropertyType} {prop.Name} {{ {prop.GetGetMethod()}; {prop.GetSetMethod()}; }} {prop.CustomAttributes.FirstOrDefault()}; Primitve: {Class1.IsPrimitive(student, prop.Name)}"));



            //Console.WriteLine();
            //Class1.TestInheritance(student);
            //Class1.TestInheritance(person);

            //File.WriteAllText("test.txt", Class1.CreateTable(p2));
            //Console.WriteLine(Class1.HasAttribute<DefaultValueAttribute>(person.GetType().GetProperty("Age")));



        }

    }


}
