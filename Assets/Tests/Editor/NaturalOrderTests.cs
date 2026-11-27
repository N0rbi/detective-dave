using DetectiveDave.EditorTools;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class NaturalOrderTests
    {
        [Test]
        public void Numbers_sort_by_value_not_by_digits()
        {
            var sorted = NaturalOrder.Sort(new[] { "Piano 10.wav", "Piano 2.wav", "Piano 1.wav", "Piano 11.wav" });
            CollectionAssert.AreEqual(new[] { "Piano 1.wav", "Piano 2.wav", "Piano 10.wav", "Piano 11.wav" }, sorted);
        }

        [Test]
        public void Names_without_numbers_sort_alphabetically()
        {
            var sorted = NaturalOrder.Sort(new[] { "b.wav", "a.wav" });
            CollectionAssert.AreEqual(new[] { "a.wav", "b.wav" }, sorted);
        }
    }
}
