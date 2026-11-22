using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class CaseLookTests
    {
        static readonly Color SettingsDescription = new Color(0.1f, 0.1f, 0.1f);
        static readonly Color SettingsFindings = new Color(0.2f, 0.2f, 0.2f);
        static readonly Color SettingsTyped = new Color(0.3f, 0.3f, 0.3f);
        static readonly Color SettingsPlaceholder = new Color(0.4f, 0.4f, 0.4f);
        static readonly Color SettingsAccept = new Color(0f, 0.5f, 0f);
        static readonly Color SettingsDecline = new Color(0.5f, 0f, 0f);

        static TextColorSet Settings() => new TextColorSet(SettingsDescription, SettingsFindings, SettingsTyped, SettingsPlaceholder, SettingsAccept, SettingsDecline);

        static ColorOverride Off() => new ColorOverride(false, Color.magenta);

        static ColorOverride On(Color color) => new ColorOverride(true, color);

        [Test]
        public void A_look_without_overrides_gives_every_colour_of_the_settings()
        {
            var look = new CaseLook(Off(), Off(), Off(), Off(), Off(), Off());

            var colors = look.Resolve(Settings());

            Assert.AreEqual(SettingsDescription, colors.Description);
            Assert.AreEqual(SettingsFindings, colors.Findings);
            Assert.AreEqual(SettingsTyped, colors.Typed);
            Assert.AreEqual(SettingsPlaceholder, colors.Placeholder);
            Assert.AreEqual(SettingsAccept, colors.AcceptPending);
            Assert.AreEqual(SettingsDecline, colors.DeclinePending);
        }

        [Test]
        public void A_default_look_changes_nothing()
        {
            var colors = new CaseLook().Resolve(Settings());

            Assert.AreEqual(SettingsDescription, colors.Description);
            Assert.AreEqual(SettingsDecline, colors.DeclinePending);
        }

        [Test]
        public void An_unticked_override_ignores_its_own_colour()
        {
            Assert.AreEqual(SettingsTyped, new ColorOverride(false, Color.red).Resolve(SettingsTyped));
        }

        [Test]
        public void A_ticked_override_replaces_only_its_own_colour()
        {
            var look = new CaseLook(On(Color.red), Off(), Off(), Off(), Off(), Off());

            var colors = look.Resolve(Settings());

            Assert.AreEqual(Color.red, colors.Description);
            Assert.AreEqual(SettingsFindings, colors.Findings);
            Assert.AreEqual(SettingsTyped, colors.Typed);
        }

        [Test]
        public void Every_override_reaches_its_own_colour()
        {
            var look = new CaseLook(On(Color.red), On(Color.green), On(Color.blue), On(Color.yellow), On(Color.cyan), On(Color.white));

            var colors = look.Resolve(Settings());

            Assert.AreEqual(Color.red, colors.Description);
            Assert.AreEqual(Color.green, colors.Findings);
            Assert.AreEqual(Color.blue, colors.Typed);
            Assert.AreEqual(Color.yellow, colors.Placeholder);
            Assert.AreEqual(Color.cyan, colors.AcceptPending);
            Assert.AreEqual(Color.white, colors.DeclinePending);
        }

        [Test]
        public void Each_font_slot_takes_the_colour_of_its_text()
        {
            var colors = Settings();

            Assert.AreEqual(SettingsDescription, colors.ForSlot(FontSlot.Request));
            Assert.AreEqual(SettingsFindings, colors.ForSlot(FontSlot.Findings));
            Assert.AreEqual(SettingsTyped, colors.ForSlot(FontSlot.Typed));
            Assert.AreEqual(SettingsPlaceholder, colors.ForSlot(FontSlot.Placeholder));
        }

        [Test]
        public void The_notice_takes_the_placeholder_colour()
        {
            Assert.AreEqual(SettingsPlaceholder, Settings().ForSlot(FontSlot.Notice));
        }
    }
}
