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
    }
}