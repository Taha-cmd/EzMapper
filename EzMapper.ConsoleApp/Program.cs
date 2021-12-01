using EzMapper.ConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EzMapper.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            File.Delete("db.sqlite");

            //EzMapper.Register<Student>();
            //EzMapper.Register<Teacher>();
            EzMapper.Register(typeof(Student), typeof(Teacher)); //TODO: assembly scan and auto register with marker interface
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
                Laptop = new Laptop() { ID = 2, Brand = "Lenovo", CPU = new Cpu() { ID = 1, Brand = "Intel", Alu = new AluUnit() { ID = 2, PlaceHolder = "out of ideas", ListOfStuff = new string[] { "one", "two", "three" } } } },
                Phones = new List<Phone>()
                {
                    new Phone() { ID = 1, Brand = "Samsung", CPU = new Cpu() { ID = 2, Brand = "Snapdragon", Alu = new AluUnit() { ID = 1, PlaceHolder = "no idea", ListOfStuff = new string[] {"four", "five", "six"} } } },
                    new Phone() { ID = 2, Brand = "IPhone", CPU = new Cpu() { ID = 3, Brand = "apple fancy processor", Alu = new AluUnit() { ID = 3, PlaceHolder = "really no idea", ListOfStuff = new string[] {"seven", "eight", "nine"} } } },
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
                Numbers = new List<int>() { 1, 2, 3 },
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
                Numbers = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 15 },
                WorkingYears = 20,
                Courses = new List<Course>() {  c2, c3 }
            };

            Person teacher3 = new Teacher()
            {
                ID = 4,
                Age = 24,
                FirstName = "Robert",
                LastName = "De Niro",
                WorkingYears = 25,
            };

            Person teacher4 = new Teacher()
            {
                ID = 5,
                Age = 30,
                FirstName = "Will",
                LastName = "Smith",
                WorkingYears = 30,
                Retired = true
            };



            await EzMapper.SaveAsync(student, teacher1, teacher2, teacher3, teacher4);

            var students = await EzMapper.GetAsync<Student>();
            var teachers = EzMapper.Get<Teacher>();

            var John = EzMapper.Get<Student>(1);
            var Jane = EzMapper.Get<Teacher>(2);
            var Jack = await EzMapper.GetAsync<Teacher>(3);


            int num = 25; // 
            var robert = EzMapper.Query<Teacher>(t => t.WorkingYears >= num && t.FirstName != "Will");
            var allButWill = EzMapper.Query<Teacher>(t => !(t.FirstName == "Will" && t.Age == 30));
            var teachersWithNoCar = EzMapper.Query<Teacher>(t => t.Car == null);
            var teachersWithNoCarYoungerThan30 = EzMapper.Query<Teacher>(t => t.Car == null && t.Age < 30);


            // no filtering based on collections
            //var test = EzMapper.Query<Teacher>(t => t.Hobbies.Contains("swimming")); // won't work

            //EzMapper.Delete(Jane);
            //EzMapper.Delete(John);


            John.Phones.Add(new Phone() { Brand = "Xiaomi", ID = 5, CPU = new Cpu() { Brand = "xiaomiCpu", ID = 15 } });
            John.School = "Fh Technikum";
            John.Car = new Car() { Brand = "Tesla", ModelNumber = 4 };
            John.Numbers.Add(500);

            Jack.Car = new Car() { Brand = "VW Golf", ModelNumber = 20 };

            EzMapper.Update(John);
            await EzMapper.UpdateAsync(Jack);

            John = EzMapper.Get<Student>(1);
            Jack = EzMapper.Get<Teacher>(Jack.ID);

            //int affectedRows = EzMapper.Delete(Jack, John, Jane);

            Console.WriteLine();
        }
    }
}
