using EzMapper.Reflection;
using EzMapper.Tests.Models;
using NUnit.Framework;
using System.Collections.Generic;

namespace EzMapper.Tests
{
    public class DataTypesTests
    {
        [Test]
        public void Check_For_Primitive_Types()
        {
            var model = new ModelWithDifferentDataTypes();

            Assert.IsTrue(Types.IsPrimitive(model, nameof(model.i)));
            Assert.IsTrue(Types.IsPrimitive(model, nameof(model.k)));
            Assert.IsTrue(Types.IsPrimitive(model, nameof(model.q)));
            Assert.IsTrue(Types.IsPrimitive(model, nameof(model.u)));
            Assert.IsTrue(Types.IsPrimitive(model, nameof(model.x)));
            Assert.IsTrue(Types.IsPrimitive(model, nameof(model.y)));
            Assert.IsTrue(Types.IsPrimitive(model, nameof(model.z)));

            Assert.IsFalse(Types.IsPrimitive(model, nameof(model.l)));
            Assert.IsFalse(Types.IsPrimitive(model, nameof(model.o)));
            Assert.IsFalse(Types.IsPrimitive(model, nameof(model.o2)));
            Assert.IsFalse(Types.IsPrimitive(model, nameof(model.obj)));
        }

        [Test]
        public void Check_For_Nullable_Types()
        {
            var model = new ModelWithDifferentDataTypes();

            Assert.IsTrue(Types.IsNullable(model, nameof(model.i)));
            Assert.IsTrue(Types.IsNullable(model, nameof(model.k)));
            Assert.IsTrue(Types.IsNullable(model, nameof(model.l)));
            Assert.IsTrue(Types.IsNullable(model, nameof(model.o)));
            Assert.IsTrue(Types.IsNullable(model, nameof(model.o2)));
            Assert.IsTrue(Types.IsNullable(model, nameof(model.u)));
            Assert.IsTrue(Types.IsNullable(model, nameof(model.y)));
            Assert.IsTrue(Types.IsNullable(model, nameof(model.obj)));

            Assert.IsFalse(Types.IsNullable(model, nameof(model.z)));
            Assert.IsFalse(Types.IsNullable(model, nameof(model.x)));
            Assert.IsFalse(Types.IsNullable(model, nameof(model.q)));
        }

        [Test]
        public void Check_For_Collections()
        {

            Assert.IsTrue(Types.IsCollection(typeof(int[])));
            Assert.IsTrue(Types.IsCollection(typeof(string[])));
            Assert.IsTrue(Types.IsCollection(typeof(List<string>)));
            Assert.IsTrue(Types.IsCollection(typeof(List<int>)));
            Assert.IsTrue(Types.IsCollection(typeof(List<ModelWithDifferentDataTypes>)));
            Assert.IsTrue(Types.IsCollection(typeof(List<ModelWithNoPrimaryKey>)));

            Assert.IsFalse(Types.IsCollection(typeof(string)));
            Assert.IsFalse(Types.IsCollection(typeof(Dictionary<int, string>)));
            Assert.IsFalse(Types.IsCollection(typeof(Dictionary<string, ModelWithNoPrimaryKey>)));
        }

        [Test]
        public void Test_Collections_Of_Objects_Check()
        {
            Assert.IsTrue(Types.HasCollectionOfType(new ModelWithDifferentDataTypes(), typeof(ModelWithNoAttribute)));
            Assert.IsFalse(Types.HasCollectionOfType(new ModelWithDifferentDataTypes(), typeof(ModelWithNoPrimaryKey)));
        }

        [Test]
        public void Test_Collections_Of_Primitives_Check()
        {
            Assert.IsTrue(Types.HasCollectionOfPrimitives(new ModelWithDifferentDataTypes()));
            Assert.IsFalse(Types.HasCollectionOfPrimitives(new ModelWithNoPrimaryKey()));
        }

        [Test]
        public void Test_Child_Type_Checking()
        {
            Assert.IsTrue(Types.HasObjectOfType(new ModelWithDifferentDataTypes(), typeof(string)));
            Assert.IsFalse(Types.HasObjectOfType(new ModelWithDifferentDataTypes(), typeof(ModelWithNoAttribute)));
        }

    }
}
