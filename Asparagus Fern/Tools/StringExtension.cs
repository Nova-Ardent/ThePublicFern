using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Asparagus_Fern.Tools
{
    public static class StringExtension
    {
        public static string ToTitleCase(this string str)
        {
            bool afterSpace = true;
            return String.Concat(str.Select(x =>
            {
                if (afterSpace && Char.IsLetter(x))
                {
                    afterSpace = false;
                    return Char.ToUpper(x);
                }
                afterSpace = char.IsWhiteSpace(x);
                return x;
            }));
        }
    }
}
