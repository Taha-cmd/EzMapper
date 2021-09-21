using System;
using System.Data.SQLite;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace EzMapper
{
    public class Class1
    {
        private readonly DbCommandBuilder builder;
        private readonly DbDataAdapter adapter;
        public Class1()
        {
            builder = new SQLiteCommandBuilder();
            adapter = new SQLiteDataAdapter();
        }

        public static void Test(object model)
        {
            var props = model.GetType().GetProperties();
            props.ToList().ForEach(prop => Console.WriteLine($"{prop.PropertyType} {prop.Name} {{ {prop.GetGetMethod()}; {prop.GetSetMethod()}; }}"));

            var sb1 = new StringBuilder($"SELECT * FROM {model.GetType().Name};");
            var sb2 = new StringBuilder($"INSERT INTO {model.GetType().Name} (");

            props.ToList().ForEach(prop => sb2.Append(prop.Name + ","));
            sb2.Replace(",", "", sb2.Length - 1, 1); // get rid of trailing comma
            sb2.Append(") VALUES ();");

            Console.WriteLine(sb1);
            Console.WriteLine(sb2);



        }
    }
}
