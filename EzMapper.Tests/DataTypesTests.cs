using EzMapper.Tests.Models;
using EzMapper.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
         
    }
}
