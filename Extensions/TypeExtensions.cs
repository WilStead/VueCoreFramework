using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MVCCoreVue.Extensions
{
    public static class TypeExtensions
    {
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

        public static bool IsIntegralNumeric(this Type type)
            => integralTypes.Contains(type)
            || integralTypes.Contains(Nullable.GetUnderlyingType(type));

        public static bool IsNumeric(this Type type)
            => IsIntegralNumeric(type)
            || IsRealNumeric(type);

        public static bool IsRealNumeric(this Type type)
            => realTypes.Contains(type)
            || realTypes.Contains(Nullable.GetUnderlyingType(type));
    }
}
