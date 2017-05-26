using System;
using System.Collections.Generic;

namespace MVCCoreVue.Extensions
{
    public static class TypeExtensions
    {
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
