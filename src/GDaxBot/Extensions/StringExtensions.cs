﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Extensions
{
    public static class StringExtensions
    {
        public static string FirstLetterCapital(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
