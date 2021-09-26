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

            Assert.IsTrue(Class1.IsPrimitive(model, nameof(model.i)));
            Assert.IsTrue(Class1.IsPrimitive(model, nameof(model.k)));
            Assert.IsTrue(Class1.IsPrimitive(model, nameof(model.q)));
            Assert.IsTrue(Class1.IsPrimitive(model, nameof(model.u)));
            Assert.IsTrue(Class1.IsPrimitive(model, nameof(model.x)));
            Assert.IsTrue(Class1.IsPrimitive(model, nameof(model.y)));
            Assert.IsTrue(Class1.IsPrimitive(model, nameof(model.z)));

            Assert.IsFalse(Class1.IsPrimitive(model, nameof(model.l)));
            Assert.IsFalse(Class1.IsPrimitive(model, nameof(model.o)));
            Assert.IsFalse(Class1.IsPrimitive(model, nameof(model.o2)));
            Assert.IsFalse(Class1.IsPrimitive(model, nameof(model.obj)));
        }

        [Test]
        public void Check_For_Nullable_Types()
        {
            var model = new ModelWithDifferentDataTypes();

            Assert.IsTrue(Class1.IsNullable(model, nameof(model.i)));
            Assert.IsTrue(Class1.IsNullable(model, nameof(model.k)));
            Assert.IsTrue(Class1.IsNullable(model, nameof(model.l)));
            Assert.IsTrue(Class1.IsNullable(model, nameof(model.o)));
            Assert.IsTrue(Class1.IsNullable(model, nameof(model.o2)));
            Assert.IsTrue(Class1.IsNullable(model, nameof(model.u)));
            Assert.IsTrue(Class1.IsNullable(model, nameof(model.y)));
            Assert.IsTrue(Class1.IsNullable(model, nameof(model.obj)));

            Assert.IsFalse(Class1.IsNullable(model, nameof(model.z)));
            Assert.IsFalse(Class1.IsNullable(model, nameof(model.x)));
            Assert.IsFalse(Class1.IsNullable(model, nameof(model.q)));
        }
         
    }
}
