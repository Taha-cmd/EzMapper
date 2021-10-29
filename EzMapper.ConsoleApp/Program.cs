﻿using System;
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
            File.Delete("db.sqlite");

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
                WorkingYears = 5,
                Courses = new List<Course>() {  c2, c3 }
            };

            EzMapper.Save(student, teacher1, teacher2);

            var students = EzMapper.Get<Student>();
            var teachers = EzMapper.Get<Teacher>();

            var John = EzMapper.Get<Student>(1);
            var Jane = EzMapper.Get<Teacher>(2);
            var Jack = EzMapper.Get<Teacher>(3);

            int affectedRows = EzMapper.Delete<Student>(1);
        }

    }


}
