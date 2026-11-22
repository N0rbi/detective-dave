using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectiveDave.MusicPlay
{
    public static class NaturalNameOrder
    {
        static readonly NaturalNameComparer Comparer = new NaturalNameComparer();

        public static IReadOnlyList<T> Sorted<T>(IEnumerable<T> items, Func<T, string> nameOf) =>
            items.OrderBy(item => nameOf(item) ?? string.Empty, Comparer).ToList();

        sealed class NaturalNameComparer : IComparer<string>
        {
            public int Compare(string left, string right)
            {
                var leftPosition = 0;
                var rightPosition = 0;
                while (leftPosition < left.Length && rightPosition < right.Length)
                {
                    var result = char.IsDigit(left[leftPosition]) && char.IsDigit(right[rightPosition])
                        ? CompareNumbers(left, ref leftPosition, right, ref rightPosition)
                        : CompareLetters(left, ref leftPosition, right, ref rightPosition);
                    if (result != 0) return result;
                }
                return (left.Length - leftPosition).CompareTo(right.Length - rightPosition);
            }

            static int CompareLetters(string left, ref int leftPosition, string right, ref int rightPosition)
            {
                var result = char.ToUpperInvariant(left[leftPosition]).CompareTo(char.ToUpperInvariant(right[rightPosition]));
                leftPosition++;
                rightPosition++;
                return result;
            }

            static int CompareNumbers(string left, ref int leftPosition, string right, ref int rightPosition)
            {
                var leftDigits = ReadDigits(left, ref leftPosition);
                var rightDigits = ReadDigits(right, ref rightPosition);
                var byLength = leftDigits.Length.CompareTo(rightDigits.Length);
                return byLength != 0 ? byLength : string.CompareOrdinal(leftDigits, rightDigits);
            }

            static string ReadDigits(string text, ref int position)
            {
                var start = position;
                while (position < text.Length && char.IsDigit(text[position])) position++;
                return text.Substring(start, position - start).TrimStart('0');
            }
        }
    }
}
