using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VueCoreFramework.Core.Extensions
{
    /// <summary>
    /// Custom extensions for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets an Attribute of the given type for an enum.
        /// </summary>
        public static T GetAttribute<T>(this object value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo.FirstOrDefault()?.GetCustomAttributes(typeof(T), false);
            return attributes?.FirstOrDefault() as T;
        }

        private static HashSet<Type> integralTypes = new HashSet<Type>
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong)
        };

        private static HashSet<Type> realTypes = new HashSet<Type>
        {
            typeof(float), typeof(double), typeof(decimal)
        };

        /// <summary>
        /// Determines if the <see cref="Type"/> is an integer-type numeric type.
        /// </summary>
        /// <remarks>
        /// Integer-type numeric types are considered to include <see cref="byte"/>, <see
        /// cref="sbyte"/>, <see cref="short"/>, <see cref="ushort"/>, <see cref="int"/>, <see
        /// cref="uint"/>, <see cref="long"/>, and <see cref="ulong"/>.
        /// </remarks>
        public static bool IsIntegralNumeric(this Type type)
            => integralTypes.Contains(type)
            || integralTypes.Contains(Nullable.GetUnderlyingType(type));

        /// <summary>
        /// Determines if the Type is a numeric type.
        /// </summary>
        public static bool IsNumeric(this Type type)
            => IsIntegralNumeric(type)
            || IsRealNumeric(type);

        /// <summary>
        /// Determines if the <see cref="Type"/> is a real-type numeric type.
        /// </summary>
        /// <remarks>
        /// Real-type numeric types are considered to include <see cref="float"/>, <see
        /// cref="double"/>, and <see cref="decimal"/>.
        /// </remarks>
        public static bool IsRealNumeric(this Type type)
            => realTypes.Contains(type)
            || realTypes.Contains(Nullable.GetUnderlyingType(type));
    }
}
