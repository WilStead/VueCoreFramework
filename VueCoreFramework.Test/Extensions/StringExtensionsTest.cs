using Microsoft.VisualStudio.TestTools.UnitTesting;
using VueCoreFramework.Core.Extensions;

namespace VueCoreFramework.Test.Extensions
{
    [TestClass]
    public class StringExtensionsTest
    {
        private const string capitalizedTest = "Capitalized string";
        private const string capitalizedTestLowercased = "capitalized string";
        private const string lowercaseTest = "lowercase string";
        private const string lowercaseTestCapitalized = "Lowercase string";

        [TestMethod]
        public void ToInitialCaps_AlreadyCap()
        {
            Assert.AreEqual(capitalizedTest, capitalizedTest.ToInitialCaps());
        }

        [TestMethod]
        public void ToInitialCaps_LowerToCap()
        {
            Assert.AreEqual(lowercaseTestCapitalized, lowercaseTest.ToInitialCaps());
        }

        [TestMethod]
        public void ToInitialLower_AlreadyLower()
        {
            Assert.AreEqual(lowercaseTest, lowercaseTest.ToInitialLower());
        }

        [TestMethod]
        public void ToInitialLower_CapToLower()
        {
            Assert.AreEqual(capitalizedTestLowercased, capitalizedTest.ToInitialLower());
        }
    }
}
