using System.Collections.Generic;

namespace VueCoreFramework.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Gets a version of this string with the first character converted to upper case.
        /// </summary>
        public static string ToInitialCaps(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var chars = str.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        /// <summary>
        /// Gets a version of this string with the first character converted to lower case.
        /// </summary>
        public static string ToInitialLower(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var chars = str.ToCharArray();
            chars[0] = char.ToLower(chars[0]);
            return new string(chars);
        }

        /// <summary>
        /// Gets a set of possible singularized versions of this string (including the unchanged string).
        /// </summary>
        /// <remarks>
        /// All replacement endings (e.g. 'y' for 'ies') will be in lower case, regardless of the
        /// case of the original string.
        /// </remarks>
        public static List<string> GetSingularForms(this string str)
        {
            List<string> singulars = new List<string> { str };
            var lower = str.ToLower();
            if (lower.EndsWith("s"))
            {
                singulars.Add(str.Substring(0, str.Length - 1));
                if (lower.EndsWith("es"))
                {
                    var less2 = str.Substring(0, str.Length - 2);
                    var less3 = str.Substring(0, str.Length - 3);
                    singulars.Add(less2);
                    singulars.Add(less2 + "is"); // Catches words like e.g. crises
                    singulars.Add(less3); // Catches words like e.g. quizzes
                    if (lower.EndsWith("ies"))
                    {
                        singulars.Add(less3 + "y");
                        singulars.Add(less3 + "ey"); // Catches words like e.g. monies
                    }
                    else if (lower.EndsWith("ices"))
                    {
                        var less4 = str.Substring(0, str.Length - 4);
                        singulars.Add(less4 + "ex"); // Catches words like e.g. vertices
                        singulars.Add(less4 + "ix"); // Catches words like e.g. matrices
                    }
                    else if (lower.EndsWith("odes")) // Catches words like e.g. octopodes
                    {
                        singulars.Add(str.Substring(0, str.Length - 4) + "us");
                    }
                    else if (lower.EndsWith("ves"))
                    {
                        singulars.Add(less3 + "f"); // Catches words like e.g. loaves
                        singulars.Add(less3 + "fe"); // Catches words like e.g. lives
                    }
                }
            }
            else if (lower.EndsWith("a"))
            {
                var less1 = str.Substring(0, str.Length - 1);
                singulars.Add(less1 + "um");
                singulars.Add(less1 + "us");
                if (lower.EndsWith("ina"))
                {
                    singulars.Add(str.Substring(0, str.Length - 3) + "en"); // Catches words like e.g. numina
                }
            }
            else if (lower.EndsWith("i"))
            {
                var less1 = str.Substring(0, str.Length - 1);
                singulars.Add(less1);
                singulars.Add(less1 + "o");
                singulars.Add(less1 + "s"); // Catches words like e.g. mythoi
                singulars.Add(less1 + "us"); // Catches words like e.g. cacti
                if (lower.EndsWith("ii")) // Catches words like e.g. radii
                {
                    singulars.Add(str.Substring(0, str.Length - 2) + "us");
                }
            }
            else if (lower.EndsWith("en"))
            {
                singulars.Add(str.Substring(0, str.Length - 2)); // Catches words like e.g. oxen
                if (lower.EndsWith("ren"))
                {
                    singulars.Add(str.Substring(0, str.Length - 3)); // Catches words like e.g. children
                }
                else if (lower.EndsWith("men"))
                {
                    singulars.Add(str.Substring(0, str.Length - 3) + "man");
                }
            }
            else if (lower.EndsWith("ice")) // Catches mice and lice
            {
                singulars.Add(str.Substring(0, str.Length - 3) + "ouse");
            }
            else if (lower.EndsWith("era")) // Catches words like e.g. genera
            {
                singulars.Add(str.Substring(0, str.Length - 3) + "us");
            }
            else if (lower.EndsWith("feet"))
            {
                singulars.Add(str.Substring(0, str.Length - 4) + "foot");
            }
            else if (lower.EndsWith("teeth"))
            {
                singulars.Add(str.Substring(0, str.Length - 5) + "tooth");
            }
            else if (lower.EndsWith("people"))
            {
                singulars.Add(str.Substring(0, str.Length - 5) + "person");
            }
            else if (lower.EndsWith("geese"))
            {
                singulars.Add(str.Substring(0, str.Length - 5) + "goose");
            }
            return singulars;
        }
    }
}
