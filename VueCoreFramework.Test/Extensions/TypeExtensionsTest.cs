using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VueCoreFramework.Extensions;

namespace VueCoreFramework.Test.Extensions
{
    [TestClass]
    public class TypeExtensionsTest
    {
        [TestMethod]
        public void IsIntegralNumeric_True()
        {
            Assert.IsTrue(typeof(int).IsIntegralNumeric());
        }

        [TestMethod]
        public void IsIntegralNumeric_False()
        {
            Assert.IsFalse(typeof(double).IsIntegralNumeric());
        }

        [TestMethod]
        public void IsRealNumeric_True()
        {
            Assert.IsTrue(typeof(double).IsRealNumeric());
        }

        [TestMethod]
        public void IsRealNumeric_False()
        {
            Assert.IsFalse(typeof(int).IsRealNumeric());
        }

        [TestMethod]
        public void IsNumeric_True()
        {
            Assert.IsTrue(typeof(double).IsNumeric());
        }

        [TestMethod]
        public void IsNumeric_False()
        {
            Assert.IsFalse(typeof(string).IsNumeric());
        }
    }
}
