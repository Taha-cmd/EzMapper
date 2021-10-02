using EzMapper.Tests.Models;
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

            Assert.IsTrue(EzMapper.IsPrimitive(model, nameof(model.i)));
            Assert.IsTrue(EzMapper.IsPrimitive(model, nameof(model.k)));
            Assert.IsTrue(EzMapper.IsPrimitive(model, nameof(model.q)));
            Assert.IsTrue(EzMapper.IsPrimitive(model, nameof(model.u)));
            Assert.IsTrue(EzMapper.IsPrimitive(model, nameof(model.x)));
            Assert.IsTrue(EzMapper.IsPrimitive(model, nameof(model.y)));
            Assert.IsTrue(EzMapper.IsPrimitive(model, nameof(model.z)));

            Assert.IsFalse(EzMapper.IsPrimitive(model, nameof(model.l)));
            Assert.IsFalse(EzMapper.IsPrimitive(model, nameof(model.o)));
            Assert.IsFalse(EzMapper.IsPrimitive(model, nameof(model.o2)));
            Assert.IsFalse(EzMapper.IsPrimitive(model, nameof(model.obj)));
        }

        [Test]
        public void Check_For_Nullable_Types()
        {
            var model = new ModelWithDifferentDataTypes();

            Assert.IsTrue(EzMapper.IsNullable(model, nameof(model.i)));
            Assert.IsTrue(EzMapper.IsNullable(model, nameof(model.k)));
            Assert.IsTrue(EzMapper.IsNullable(model, nameof(model.l)));
            Assert.IsTrue(EzMapper.IsNullable(model, nameof(model.o)));
            Assert.IsTrue(EzMapper.IsNullable(model, nameof(model.o2)));
            Assert.IsTrue(EzMapper.IsNullable(model, nameof(model.u)));
            Assert.IsTrue(EzMapper.IsNullable(model, nameof(model.y)));
            Assert.IsTrue(EzMapper.IsNullable(model, nameof(model.obj)));

            Assert.IsFalse(EzMapper.IsNullable(model, nameof(model.z)));
            Assert.IsFalse(EzMapper.IsNullable(model, nameof(model.x)));
            Assert.IsFalse(EzMapper.IsNullable(model, nameof(model.q)));
        }

        [Test]
        public void Check_For_Collections()
        {

            Assert.IsTrue(EzMapper.IsCollection(typeof(int[])));
            Assert.IsTrue(EzMapper.IsCollection(typeof(string[])));
            Assert.IsTrue(EzMapper.IsCollection(typeof(List<string>)));
            Assert.IsTrue(EzMapper.IsCollection(typeof(List<int>)));
            Assert.IsTrue(EzMapper.IsCollection(typeof(List<ModelWithDifferentDataTypes>)));
            Assert.IsTrue(EzMapper.IsCollection(typeof(List<ModelWithNoPrimaryKey>)));

            Assert.IsFalse(EzMapper.IsCollection(typeof(string)));
            Assert.IsFalse(EzMapper.IsCollection(typeof(Dictionary<int, string>)));
            Assert.IsFalse(EzMapper.IsCollection(typeof(Dictionary<string, ModelWithNoPrimaryKey>)));
        }
         
    }
}
