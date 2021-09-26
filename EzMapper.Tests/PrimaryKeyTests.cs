using EzMapper.Tests.Models;
using NUnit.Framework;
using System;

namespace EzMapper.Tests
{
    public class PrimaryKeyTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Model_Has_More_Than_One_PrimaryKey()
        {
            var model = new ModelWithTwoPrimaryKeys();

            Assert.Throws<Exception>(() => Class1.GetPrimaryKeyPropertyName(model));
        }

        [Test]
        public void Model_Has_PrimaryKey_Attribute()
        {
            var model = new ModelWithPrimaryKeyAttribute();

            var actual = nameof(model.X);
            var returned = Class1.GetPrimaryKeyPropertyName(model);

            Assert.IsTrue(actual == returned);
        }

        [Test]
        public void Model_Has_No_PrimaryKey_Attribute()
        {
            var model = new ModelWithNoAttribute();

            var returned = Class1.GetPrimaryKeyPropertyName(model);

            Assert.IsTrue("ID" == returned.ToUpper());
        }

        [Test]
        public void Model_Has_No_PrimaryKey()
        {
            var model = new ModelWithNoPrimaryKey();

            Assert.Throws<Exception>(() => Class1.GetPrimaryKeyPropertyName(model));
        }

        [Test]
        public void Model_Has_Wrong_Data_Type_For_PrimaryKey()
        {
            var model = new ModelWithWrongPrimaryKeyDataType();

            Assert.Throws<Exception>(() => Class1.GetPrimaryKeyPropertyName(model));
        }
        
    }
}