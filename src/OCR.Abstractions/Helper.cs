using System;
using System.Collections.Generic;
using System.Text;

namespace OCR.Abstractions
{
    public static class Helper
    {
        public static string GetExtension(string fullName)
        {
            int i = fullName.LastIndexOf('.');
            string rhs = i < 0 ? "" : fullName.Substring(i + 1);
            return rhs;
        }

        public static string GetFileName(string fullName)
        {
            int i = fullName.LastIndexOf('.');
            string lhs = i < 0 ? fullName : fullName.Substring(0, i);
            return lhs;
        }

        public static string GetBeforeChar(string source, char character)
        {
            int i = source.IndexOf(character);
            string lhs = i < 0 ? source : source.Substring(0, i);
            return lhs;
        }

        public static string GetAfter(string source, string delimeter)
        {
            int startIndex = source.IndexOf(delimeter);
            if (startIndex == -1) return "";

            string result = source.Substring(startIndex + delimeter.Length);
            return result;
        }

        public static string GetBeforeString(string source, string delimeter)
        {
            int i = source.IndexOf(delimeter);

            string result = i < 0 ? source : source.Substring(0, i);
            return result;
        }

        public static string GetBeforeLastIndexOf(string source, char delimeter)
        {
            int i = source.LastIndexOf(delimeter);
            string lhs = i < 0 ? source : source.Substring(0, i);
            return lhs;
        }

        public static string GetAfterLastIndexOf(string source, string delimeter)
        {
            int startIndex = source.LastIndexOf(delimeter);
            if (startIndex == -1) return "";

            string result = source.Substring(startIndex + delimeter.Length);
            return result;
        }

        public static string RemoveNewLine(string source)
        {
            return source.Replace('\n', ' ');
        }
    }
}
