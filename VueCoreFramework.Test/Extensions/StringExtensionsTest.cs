using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using VueCoreFramework.Extensions;

namespace VueCoreFramework.Test.Extensions
{
    [TestClass]
    public class StringExtensionsTest
    {
        private const string capitalizedTest = "Capitalized string";
        private const string capitalizedTestLowercased = "capitalized string";
        private const string lowercaseTest = "lowercase string";
        private const string lowercaseTestCapitalized = "Lowercase string";

        private Dictionary<string, string> singularizedWords = new Dictionary<string, string>
        {
            { "countries", "country" },
            { "airlines", "airline" },
            { "mixes", "mix" },
            { "leaders", "leader" },
            { "quizzes", "quiz" },
            { "crises", "crisis" },
            { "monies", "money" },
            { "vertices", "vertex" },
            { "matrices", "matrix" },
            { "octopodes", "octopus" },
            { "loaves", "loaf" },
            { "numina", "numen" },
            { "mythoi", "mythos" },
            { "cacti", "cactus" },
            { "radii", "radius" },
            { "oxen", "ox" },
            { "children", "child" },
            { "men", "man" },
            { "women", "woman" },
            { "mice", "mouse" },
            { "genera", "genus" },
            { "feet", "foot" },
            { "teeth", "tooth" },
            { "people", "person" },
            { "geese", "goose" }
        };

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
