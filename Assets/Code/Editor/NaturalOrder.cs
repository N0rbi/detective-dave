using System.Linq;
using System.Text.RegularExpressions;

namespace DetectiveDave.EditorTools
{
    public static class NaturalOrder
    {
        const int DigitWidth = 8;

        public static string[] Sort(string[] paths) => paths.OrderBy(Key).ThenBy(path => path).ToArray();

        static string Key(string path) => Regex.Replace(path, @"\d+", match => match.Value.PadLeft(DigitWidth, '0'));
    }
}
