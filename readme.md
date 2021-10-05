# EzMapper

A code first object relational mapping framework for c# that uses SQLite under the hood.

## Features

- Basic CRUD
- Inheritance
- 1:1, 1:m and m:n relationships

## Conventions and Defaults

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

- EzMapper takes care of 1:1, 1:n and inheritance relationships for you! you only need to register the concrete class/aggregate root, the framework will figure out the dependencies recursively. Consider the following example:

  ```c#
  public class Book
  {
      public int ID {get; set;}
      public string AuthorName {get;set;}
  }

  public class Phone
  {
      public int ID {get;set;}
      public string Brand {get;set;}
  }

  public class Person
  {
      public int ID {get;set;}
      public string Name {get;set;}
      public List<Book> Books {get;set;}
  }

  public class Student : Person
  {
      public int Age {get;set;}
      public Phone Phone {get;set;}
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

  public class Order
  {
      public int ID {get;set;}
      public DateTime DateTime {get;set;}

      [Shared]
      public List<Product> Products {get;set;}
  }

  ```

  In this example, you only need to register the Order class, but you need to mark the Products Property with the Shared Attribute if you wish to set up a m:n relationship. Otherwise, it will be considered a 1:n relationship
