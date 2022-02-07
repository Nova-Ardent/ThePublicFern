using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Asparagus_Fern.Tools
{
    public static class EnumExtensions
    {
        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            return type.GetField(name) // I prefer to get attributes this way
                .GetCustomAttribute<TAttribute>();
        }

        public static int IndexOf<T>(this IList<T> source, Func<T, bool> condition)
        {
            for (int i = 0; i < source.Count; i++)
                if (condition(source[i]))
                    return i;

            return -1;
        }

        public static bool Any<T>(this IList<T> source, Func<T, bool> condition, out int index)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (condition(source[i]))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public static IEnumerable<int> Repeat(this int n)
        {
            for (int i = 0; i < n; i++)
                yield return n;
        }
    }
}