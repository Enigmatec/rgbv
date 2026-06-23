using System;
using System.Linq;

namespace Service.Extensions
{
    public static class StringExtension
    {
        public static string GetFirstLetterToUpper(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            var item = value.Trim();

            return item.FirstOrDefault().ToString().ToUpper();
        }

        public static string Concat(this string value, string next)
        {
            return $"{value.Trim()}{next.Trim()}";
        }

        public static string FirstLetterToUpper(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            var item = value.Trim();

            return item.FirstOrDefault().ToString().ToUpper() + item.Substring(1);
        }

        public static string ToTitleCase(this string str)
        {
            var firstword = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.Split(' ')[0].ToLower());
            str = str.Replace(str.Split(' ')[0], firstword);
            return str;
        }

        public static string FirstCharToUpper(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }
            return str?.First().ToString().ToUpper() + str?[1..].ToLower();
        }

        public static string RemoveExtraSpaces(this string input)
        {
            return string.Join(" ", input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}