using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VueCoreFramework.Extensions;

namespace VueCoreFramework.Test.Extensions
{
    [Flags]
    public enum TestEnum
    {
        None = 0,
        [System.ComponentModel.Description(EnumExtensionsTest.testValue1Description)]
        TestValue1 = 1,
        TestValue2 = 2
    }

    [TestClass]
    public class EnumExtensionsTest
    {
        public const string testValue1Description = "Test Value 1";

        [TestMethod]
        public void GetDescription_EnumHasDescription()
        {
            var enumVal = TestEnum.TestValue1;
            var enumValDesc = EnumExtensions.GetDescription(typeof(TestEnum), enumVal);
            Assert.AreEqual(testValue1Description, enumValDesc);
        }


        [TestMethod]
        public void GetDescription_EnumHasNoDescription()
        {
            var enumVal = TestEnum.TestValue2;
            var enumValDesc = EnumExtensions.GetDescription(typeof(TestEnum), enumVal);
            Assert.AreEqual(Enum.GetName(typeof(TestEnum), enumVal), enumValDesc);
        }

        [TestMethod]
        public void GetDescription_EnumIsFlagCombination()
        {
            var enumVal = TestEnum.TestValue1 | TestEnum.TestValue2;
            var enumValDesc = EnumExtensions.GetDescription(typeof(TestEnum), enumVal);
            Assert.AreEqual(null, enumValDesc);
        }
    }
}
