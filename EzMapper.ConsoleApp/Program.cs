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

            //Class1.TestCrud(student);

            //var props = student.GetType().GetProperties().ToList(); // get all properties (including inherited once)
            var props = student.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).ToList(); // get all properties that were declared in this specific type (ignore inherited ones)
            //props.ForEach(prop => Console.WriteLine($"{prop.PropertyType} {prop.Name} {{ {prop.GetGetMethod()}; {prop.GetSetMethod()}; }} {prop.CustomAttributes.FirstOrDefault()}; Primitve: {Class1.IsPrimitive(student, prop.Name)}"));



            Console.WriteLine();
            //Class1.TestInheritance(student);
            //Class1.TestInheritance(person);

            Console.WriteLine(Class1.CreateTable(student));
            //Console.WriteLine(Class1.HasAttribute<DefaultValueAttribute>(person.GetType().GetProperty("Age")));



            var test = new TestClass();
            int x = 3;
            int? y = 4;
            string z = "we";
            string? u = "we";
            object o = new Student();
            object o2 = 3;
            object l = null;
            double q = 4.2;
            double? k = 5.6;
            double? i = null;

            //Console.WriteLine(Class1.IsNullable(test, "x"));
            //Console.WriteLine(Class1.IsNullable(test, "y"));
            Console.WriteLine(Class1.IsNullable(test, "z"));
            //Console.WriteLine(Class1.IsNullable(test, "u"));
            //Console.WriteLine(Class1.IsNullable(test, "o"));
            //Console.WriteLine(Class1.IsNullable(test, "o2"));
            //Console.WriteLine(Class1.IsNullable(test,"l"));
            //Console.WriteLine(Class1.IsNullable(test, "q"));
            //Console.WriteLine(Class1.IsNullable(test, "k"));
            //Console.WriteLine(Class1.IsNullable(test, "i"));
            //Console.WriteLine();






            Console.WriteLine(test.GetType().GetProperty("i").PropertyType);
            Console.WriteLine(Nullable.GetUnderlyingType(test.GetType().GetProperty("i").PropertyType));






            //string x = "wewe";
            //decimal y = 3;
            //Class1.IsPrimitive(true);
            //Class1.IsPrimitive(DateTime.Now);
            //Class1.IsPrimitive(2);
            //Class1.IsPrimitive("wewe");
            //Class1.IsPrimitive(person);
            //Class1.IsPrimitive(student);
            //Class1.IsPrimitive(x);
            //Class1.IsPrimitive(y);
            //Class1.IsPrimitive((IModel)student);
        }

        class TestClass
        {
            public int x { get; set; } = 3;
            public int? y { get; set; } = 4;
            [NotNull]
            public string z { get; set; } = "we";
            public string? u { get; set; } = "we";
            public object o { get; set; } = new Student();
            public object o2 { get; set; } = 3;
            public object l { get; set; } = null;
            public double q { get; set; } = 4.2;
            public double? k { get; set; } = 5.6;
            public double? i { get; set; } = null;
        }
    }


}
