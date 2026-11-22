using System.Linq;
using DetectiveDave.MusicPlay;
using NUnit.Framework;

namespace DetectiveDave.Tests
{
    public class NaturalNameOrderTests
    {
        static string[] Sort(params string[] names) => NaturalNameOrder.Sorted(names, name => name).ToArray();

        [Test]
        public void Numbers_one_to_twelve_sort_by_value_not_by_text()
        {
            var names = Enumerable.Range(1, 12).Select(number => $"Jazz - Piano - {number}").Reverse().ToArray();
            var expected = Enumerable.Range(1, 12).Select(number => $"Jazz - Piano - {number}").ToArray();
            CollectionAssert.AreEqual(expected, Sort(names));
        }

        [Test]
        public void Piano_2_comes_before_Piano_10()
        {
            CollectionAssert.AreEqual(new[] { "Piano 2", "Piano 10" }, Sort("Piano 10", "Piano 2"));
        }

        [Test]
        public void Upper_and_lower_case_sort_together()
        {
            CollectionAssert.AreEqual(new[] { "apple 1", "Banana 1", "cherry 1" }, Sort("cherry 1", "apple 1", "Banana 1"));
        }

        [Test]
        public void Leading_zeros_do_not_change_the_value()
        {
            CollectionAssert.AreEqual(new[] { "Note 2", "Note 03", "Note 10" }, Sort("Note 10", "Note 03", "Note 2"));
        }

        [Test]
        public void A_shorter_name_that_starts_the_other_comes_first()
        {
            CollectionAssert.AreEqual(new[] { "Piano", "Piano 1" }, Sort("Piano 1", "Piano"));
        }

        [Test]
        public void Equal_names_keep_their_original_order()
        {
            var items = new[] { (Name: "Piano 1", Tag: "first"), (Name: "piano 1", Tag: "second"), (Name: "Piano 1", Tag: "third") };
            var sorted = NaturalNameOrder.Sorted(items, item => item.Name).Select(item => item.Tag).ToArray();
            CollectionAssert.AreEqual(new[] { "first", "second", "third" }, sorted);
        }

        [Test]
        public void No_items_gives_an_empty_list()
        {
            Assert.IsEmpty(Sort());
        }

        [Test]
        public void One_item_stays_alone()
        {
            CollectionAssert.AreEqual(new[] { "Only" }, Sort("Only"));
        }

        [Test]
        public void A_missing_name_sorts_first_and_does_not_throw()
        {
            CollectionAssert.AreEqual(new string[] { null, "Piano 1" }, Sort("Piano 1", null));
        }
    }
}
