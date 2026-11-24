using System.Linq;
using DetectiveDave.EditorTools;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class KeyArtSetupTests
    {
        [Test]
        public void The_rim_of_the_painted_box_is_a_share_of_its_short_side()
        {
            Assert.AreEqual(65, KeyArtBorder.RimPixels(872, 872));
        }

        [Test]
        public void The_rim_follows_the_shorter_side_of_a_tall_sprite()
        {
            Assert.AreEqual(49, KeyArtBorder.RimPixels(654, 1744));
        }

        [Test]
        public void The_rim_is_never_less_than_one_pixel()
        {
            Assert.AreEqual(1, KeyArtBorder.RimPixels(4, 4));
        }

        [Test]
        public void A_file_only_in_staging_needs_to_move()
        {
            Assert.AreEqual(KeyArtSlotState.NeedsMove, KeyArtSlot.Classify(true, false));
        }

        [Test]
        public void A_file_only_in_the_target_folder_is_already_in_place()
        {
            Assert.AreEqual(KeyArtSlotState.InPlace, KeyArtSlot.Classify(false, true));
        }

        [Test]
        public void A_file_nowhere_is_missing()
        {
            Assert.AreEqual(KeyArtSlotState.Missing, KeyArtSlot.Classify(false, false));
        }

        [Test]
        public void A_file_in_both_places_is_a_conflict()
        {
            Assert.AreEqual(KeyArtSlotState.Conflict, KeyArtSlot.Classify(true, true));
        }

        [Test]
        public void The_box_border_is_the_rim_on_every_side_when_there_is_no_taller_top()
        {
            Assert.AreEqual(new Vector4(65f, 65f, 65f, 65f), KeyArtBorder.BoxBorder(872, 872, 0));
        }

        [Test]
        public void A_taller_top_rim_keeps_the_pipe_strip_out_of_the_stretched_middle()
        {
            Assert.AreEqual(new Vector4(65f, 65f, 65f, 97f), KeyArtBorder.BoxBorder(872, 872, KeyArtTileTable.PipeStripHeightPixels));
        }

        [Test]
        public void A_top_rim_thinner_than_the_rim_is_ignored()
        {
            Assert.AreEqual(new Vector4(65f, 65f, 65f, 65f), KeyArtBorder.BoxBorder(872, 872, 10));
        }

        [Test]
        public void Only_the_climb_art_gets_the_pipe_strip_as_its_top_rim()
        {
            var withTop = KeyArtTileTable.Boxes.Where(row => row.TopBorderPixels > 0).Select(row => row.TileName);

            CollectionAssert.AreEqual(new[] { "WallGutter" }, withTop);
        }

        [Test]
        public void The_table_names_each_tile_type_and_staged_file_once()
        {
            Assert.AreEqual(KeyArtTileTable.Boxes.Count, KeyArtTileTable.Boxes.Select(row => row.TileName).Distinct().Count());
            Assert.AreEqual(KeyArtTileTable.Boxes.Count, KeyArtTileTable.Boxes.Select(row => row.StagedName).Distinct().Count());
            Assert.AreEqual(KeyArtTileTable.Boxes.Count, KeyArtTileTable.Boxes.Select(row => row.TargetPath).Distinct().Count());
        }

        [Test]
        public void The_table_never_assigns_the_walkable_tile_or_the_loose_pipe()
        {
            CollectionAssert.DoesNotContain(KeyArtTileTable.Boxes.Select(row => row.TileName), "Walkable");
            CollectionAssert.DoesNotContain(KeyArtTileTable.Boxes.Select(row => row.StagedName), KeyArtTileTable.UnassignedStagedName);
        }

        [Test]
        public void A_row_reads_its_paths_from_its_names()
        {
            var row = KeyArtTileTable.Boxes.First(candidate => candidate.TileName == "WallGutter");

            Assert.AreEqual("Assets/Tiles/WallGutter.asset", row.TilePath);
            Assert.AreEqual("Assets/Staging/Tiles/keyboard_stealth_climb.png", row.StagedPath);
            Assert.AreEqual("Assets/Key Art/WallGutter/wallgutter_box.png", row.TargetPath);
        }

        [Test]
        public void Only_the_shapes_of_single_rectangle_keys_are_removed_and_the_enter_stays()
        {
            var flags = new[] { false, false, false, true, false };

            CollectionAssert.AreEqual(new[] { 4, 2, 1, 0 }, KeyArtShapeFilter.SingleKeyIndicesLastFirst(flags));
        }

        [Test]
        public void A_style_with_only_an_enter_shape_loses_nothing()
        {
            CollectionAssert.IsEmpty(KeyArtShapeFilter.SingleKeyIndicesLastFirst(new[] { true }));
        }
    }
}
