# EzMapper

A code first object relational mapping framework for c# that uses SQLite under the hood.

## Features

- Basic CRUD
- Inheritance
- 1:1, 1:m and m:n relationships

## Getting started

- An object should have an integer property named ID. This property will be used as the primary key. If you wish the property to have a different name, then mark it with the PrimaryKey Attribute located in the EzMapper.Attributes namespace

  ```c#
  public class Person
  {
      public int ID {get; set;}
  }
  ```

  or

  ```c#
  public class Person
  {
      [PrimaryKey]
      public int PersonID {get;set;}
  }
  ```

- EzMapper takes care of 1:1, 1:n and inheritance relationships for you! you only need to register the concrete class/aggregate root, the framework will figure out the dependencies recursively. When dealing with colletions,
  you need to specify the action on deleting the parent record.
  When dealing with collections, you can use both lists and arrays. More complex types like dictionaries are not supported.
  Consider the following example:

  ```c#
  public class Book
  {
      public int ID {get; set;}
      public string AuthorName {get;set;}
  }

  public class Phone
  {
      public int ID {get;set;}

      [DefaultValue("Samsung")]
      public string Brand {get;set;}
  }

  public class Person
  {
      public int ID {get;set;}
      public string Name {get;set;}

      [OnDelete(DeleteAction.SetNull)]
      public Book[] Books {get;set;}
  }

  public class Student : Person, IEzModel
  {
      public int Age {get;set;}
      public Phone Phone {get;set;}

      [OnDelete(DeleteAction.Cascade)]
      public List<string> Hobbies {get;set;}
  }
  ```

  In this example, you only need to register the student class. The Framework will recognize that a Student is an inherited class, so it will try to create the Person class first. When iterating through the properties of a person, the Framework will recognize that the Books property is a collection of another class, so the Book class will also be handled. Similarly, the Phone class will also be created. Notice that we can use collections of both primitive and complex types.

- EzMapper can handle m:n relationships too, but only with the help of the Shared Attribute. Consider the following example:

  ```c#
  public class Product
  {
      public int ID {get;set;}
      public string Name {get;set;}
  }

  public class Order : IEzModel
  {
      public int ID {get;set;}
      public DateTime DateTime {get;set;}

      [Shared]
      public List<Product> Products {get;set;}
  }
  ```

  In this example, you only need to register the Order class, but you need to mark the Products Property with the Shared Attribute if you wish to set up a m:n relationship. Otherwise, it will be considered a 1:n relationship

- After defining your data model, you can get started with EzMapper by registering your types and building the database. EzMapper follows a similiar appraoch to the repository pattern, meaning you only need to register aggregate rootes and concrete types

  ```c#
  EzMapper.Register<Order>(); // you can register each type via a generic method
  EzMapper.Register<Student>();

  EzMapper.Register(typeof(Order), typeof(Student)); // or pass all types via a single call

  EzMapper.RegisterTypesFromAssembly(Assembly.GetExecutingAssembly()); // or scan the assembly for types.
  //For this to work, you need to mark the types with the IEzModel interface

  EzMapper.Build(); // will build the database after registering all types

  //this code must always run at the beginning of the program, even if the databsae allready exists
  ```

- Saving, updating and deleting data

  ```c#
  var person1 = new Student() { /*values*/ }; // notice that we are creating a student as a person
  var person2 = new Student() { /*values*/ };
  var order = new Order() { /*values*/ };

  EzMapper.Save(person1, person2, order); // you can pass as many objects as you want to the save method
  await EzMapper.SaveAsync(person1, person2, order); // you can save your data async as well

  //values changed
  person1.Age = /*new value*/
  order.Products.Add(new Product() { /* values */ });

  EzMapper.Update(person1, order); //similar to the save method
  await EzMapper.UpdateAsync(person1, order); //same api for saving data


  //suppose both students want to delete their account
  EzMapper.Delete(person1, person2); // pass as many object as you like
  await EzMapper.DeleteAsync(person1, person2);

  // or specify type and id
  EzMapper.Delete<Student>(person1.ID);
  await EzMapper.DeleteAsync<Student>(person2.ID);
  ```

- Retrieving and querying data

  ```c#
  var students = EzMapper.Get<Student>(); // will get all the students
  var students = await EzMapper.GetAsync<Student>();

  //EzMapper can handle polymorphism as well!
  var people = EzMapper.Get<Person>(); // all students and other subtypes inheriting from person


  //polymorphic read
  var student1 = EzMapper.Get<Person>(1); // will return student1 as a person
  var student2 = await EzMapper.GetAsync<Person>(2); // will return student1 as a person

  // concrete type read
  var student1 = EzMapper.Get<Studnet>(1); // pass the id
  var student1 = await EzMapper.GetAsync<Student>(1);

  //polymorphism is only supported in Get methods but not in Query

  // if you wish to perform queries, then use the query method
  var adults = EzMapper.Query<Studnet>(studnet => student.Age > 18);
  var adults = await EzMapper.QueryAsync<Studnet>(studnet => student.Age > 18);

  //more complex queries
  var minorsWithNoPhone = EzMapper.Query<Student>(student => student.Age < 18 && student.Phone == null);
  var minorsWithNoPhone = await EzMapper.QueryAsync<Student>(student => student.Age < 18 && student.Phone == null);

  //you can use the Contains method to filter based on collections of PRIMITIVES
  var readers = EzMapper.Query<Student>(s => s.Hobbies.Contains("reading"));


  //limitations:
  //for now, it not possible to query based on complex collection or nested properties. for example:
  EzMapper.Query<Student>(s => s.Phone.ID = 23); // THROWS
  EzMapper.Query<Student>(s => s.Books.Contains(new Book(){ID = 3})); // THROWS
  ```

- EzMapper is not a fully featured ORM, it rather tries to offer a very easy and fast persistence layer by fully abstracting the database. The user has a cheap way of persisting objects without worrying about sql, connection strings, installation etc. EzMapper is obviously not suited for applications where you need the absolute control over the database and the data model. It creates an Sqlite database located in the binary folder. So it is best suited for applications that need an installation specific local storage, like desktop applications. EzMapper does not support migrations yet. If you wish to refactor you data model, simply delete the database created in the binary folder from the file system. Upon starting the application, a new one will be created with the new data model.
