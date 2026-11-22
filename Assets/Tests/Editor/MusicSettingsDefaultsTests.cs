using DetectiveDave.MusicPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class MusicSettingsDefaultsTests
    {
        MusicSettings settings;

        [SetUp]
        public void SetUp() => settings = ScriptableObject.CreateInstance<MusicSettings>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(settings);

        [Test]
        public void A_new_settings_asset_plays_chord_aware_notes_on_a_triplet_grid()
        {
            Assert.AreEqual(PianoOrder.ChordAware, settings.PianoOrder);
            Assert.AreEqual(QuantizeGrid.TripletEighth, settings.QuantizeGrid);
            Assert.AreEqual(0.25f, settings.MaxDelaySeconds, 1e-6f);
            Assert.AreEqual(3f, settings.ChordToneWeight, 1e-6f);
        }

        [Test]
        public void Without_a_chord_chart_the_settings_report_it_as_missing()
        {
            StringAssert.Contains("Chord Chart", settings.MissingReferences);
        }
    }
}
