using EzMapper.Reflection;
using EzMapper.Tests.Models;
using NUnit.Framework;

namespace EzMapper.Tests
{
    class ReflectionTests
    {

        [Test]
        public void Test_Read_Id_Value()
        {
            object model = new ModelWithNoAttribute() { ID = 3 };

            int id = ModelParser.GetModelId(model);

            NUnit.Framework.Assert.IsTrue(id == 3);
        }

        [Test]
        public void Detect_Inheritance()
        {
            object model = new Child<object>();

            NUnit.Framework.Assert.IsTrue(Types.HasParentModel(model));
            NUnit.Framework.Assert.IsTrue(Types.HasParentModel(model.GetType()));
        }

        [Test]
        public void Detect_Collection_Types()
        {
            var model = new Child<double>();

            var elementType = Types.GetElementType(model.Collection.GetType());

            NUnit.Framework.Assert.IsTrue(elementType == typeof(double));
        }

        [Test]
        public void Detect_Collections()
        {
            var model1 = new Child<double>();
            var model2 = new Child<ModelWithNoAttribute>();


            NUnit.Framework.Assert.IsTrue(Types.HasCollectionOfPrimitives(model1));
            NUnit.Framework.Assert.IsFalse(Types.HasCollectionOfPrimitives(model2));

            NUnit.Framework.Assert.IsTrue(Types.HasCollectionOfType(model1.GetType(), typeof(double)));
            NUnit.Framework.Assert.IsTrue(Types.HasCollectionOfType(model2.GetType(), typeof(ModelWithNoAttribute)));

            NUnit.Framework.Assert.IsFalse(Types.HasCollectionOfType(model1.GetType(), typeof(int)));
            NUnit.Framework.Assert.IsFalse(Types.HasCollectionOfType(model2.GetType(), typeof(int)));

        }
    }
}
