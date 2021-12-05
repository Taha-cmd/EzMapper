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

            NUnit.Framework.Assert.IsTrue(Types.IsPrimitive(model, nameof(model.i)));
            NUnit.Framework.Assert.IsTrue(Types.IsPrimitive(model, nameof(model.k)));
            NUnit.Framework.Assert.IsTrue(Types.IsPrimitive(model, nameof(model.q)));
            NUnit.Framework.Assert.IsTrue(Types.IsPrimitive(model, nameof(model.u)));
            NUnit.Framework.Assert.IsTrue(Types.IsPrimitive(model, nameof(model.x)));
            NUnit.Framework.Assert.IsTrue(Types.IsPrimitive(model, nameof(model.y)));
            NUnit.Framework.Assert.IsTrue(Types.IsPrimitive(model, nameof(model.z)));

            NUnit.Framework.Assert.IsFalse(Types.IsPrimitive(model, nameof(model.l)));
            NUnit.Framework.Assert.IsFalse(Types.IsPrimitive(model, nameof(model.o)));
            NUnit.Framework.Assert.IsFalse(Types.IsPrimitive(model, nameof(model.o2)));
            NUnit.Framework.Assert.IsFalse(Types.IsPrimitive(model, nameof(model.obj)));
        }

        [Test]
        public void Check_For_Nullable_Types()
        {
            var model = new ModelWithDifferentDataTypes();

            NUnit.Framework.Assert.IsTrue(Types.IsNullable(model, nameof(model.i)));
            NUnit.Framework.Assert.IsTrue(Types.IsNullable(model, nameof(model.k)));
            NUnit.Framework.Assert.IsTrue(Types.IsNullable(model, nameof(model.l)));
            NUnit.Framework.Assert.IsTrue(Types.IsNullable(model, nameof(model.o)));
            NUnit.Framework.Assert.IsTrue(Types.IsNullable(model, nameof(model.o2)));
            NUnit.Framework.Assert.IsTrue(Types.IsNullable(model, nameof(model.u)));
            NUnit.Framework.Assert.IsTrue(Types.IsNullable(model, nameof(model.y)));
            NUnit.Framework.Assert.IsTrue(Types.IsNullable(model, nameof(model.obj)));

            NUnit.Framework.Assert.IsFalse(Types.IsNullable(model, nameof(model.z)));
            NUnit.Framework.Assert.IsFalse(Types.IsNullable(model, nameof(model.x)));
            NUnit.Framework.Assert.IsFalse(Types.IsNullable(model, nameof(model.q)));
        }

        [Test]
        public void Check_For_Collections()
        {

            NUnit.Framework.Assert.IsTrue(Types.IsCollection(typeof(int[])));
            NUnit.Framework.Assert.IsTrue(Types.IsCollection(typeof(string[])));
            NUnit.Framework.Assert.IsTrue(Types.IsCollection(typeof(List<string>)));
            NUnit.Framework.Assert.IsTrue(Types.IsCollection(typeof(List<int>)));
            NUnit.Framework.Assert.IsTrue(Types.IsCollection(typeof(List<ModelWithDifferentDataTypes>)));
            NUnit.Framework.Assert.IsTrue(Types.IsCollection(typeof(List<ModelWithNoPrimaryKey>)));

            NUnit.Framework.Assert.IsFalse(Types.IsCollection(typeof(string)));
            NUnit.Framework.Assert.IsFalse(Types.IsCollection(typeof(Dictionary<int, string>)));
            NUnit.Framework.Assert.IsFalse(Types.IsCollection(typeof(Dictionary<string, ModelWithNoPrimaryKey>)));
        }

        [Test]
        public void Test_Collections_Of_Objects_Check()
        {
            NUnit.Framework.Assert.IsTrue(Types.HasCollectionOfType(new ModelWithDifferentDataTypes(), typeof(ModelWithNoAttribute)));
            NUnit.Framework.Assert.IsFalse(Types.HasCollectionOfType(new ModelWithDifferentDataTypes(), typeof(ModelWithNoPrimaryKey)));
        }

        [Test]
        public void Test_Collections_Of_Primitives_Check()
        {
            NUnit.Framework.Assert.IsTrue(Types.HasCollectionOfPrimitives(new ModelWithDifferentDataTypes()));
            NUnit.Framework.Assert.IsFalse(Types.HasCollectionOfPrimitives(new ModelWithNoPrimaryKey()));
        }

        [Test]
        public void Test_Child_Type_Checking()
        {
            NUnit.Framework.Assert.IsTrue(Types.HasObjectOfType(new ModelWithDifferentDataTypes(), typeof(string)));
            NUnit.Framework.Assert.IsFalse(Types.HasObjectOfType(new ModelWithDifferentDataTypes(), typeof(ModelWithNoAttribute)));
        }

    }
}
