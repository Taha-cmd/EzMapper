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
