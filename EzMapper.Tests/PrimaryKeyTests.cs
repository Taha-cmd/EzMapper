using EzMapper.Database;
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

            NUnit.Framework.Assert.Throws<Exception>(() => ModelParser.GetPrimaryKeyPropertyName(model.GetType()));
        }

        [Test]
        public void Model_Has_PrimaryKey_Attribute()
        {
            var model = new ModelWithPrimaryKeyAttribute();

            var actual = nameof(model.X);
            var returned = ModelParser.GetPrimaryKeyPropertyName(model.GetType());

            NUnit.Framework.Assert.IsTrue(actual == returned);
        }

        [Test]
        public void Model_Has_No_PrimaryKey_Attribute()
        {
            var model = new ModelWithNoAttribute();

            var returned = ModelParser.GetPrimaryKeyPropertyName(model.GetType());

            NUnit.Framework.Assert.IsTrue("ID" == returned.ToUpper());
        }

        [Test]
        public void Model_Has_No_PrimaryKey()
        {
            var model = new ModelWithNoPrimaryKey();

            NUnit.Framework.Assert.Throws<Exception>(() => ModelParser.GetPrimaryKeyPropertyName(model.GetType()));
        }

        [Test]
        public void Model_Has_Wrong_Data_Type_For_PrimaryKey()
        {
            var model = new ModelWithWrongPrimaryKeyDataType();

            NUnit.Framework.Assert.Throws<Exception>(() => ModelParser.GetPrimaryKeyPropertyName(model.GetType()));
        }
        
    }
}