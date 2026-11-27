using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class RequestFontChoiceTests
    {
        Font caseFont;
        Font settingsFont;

        [SetUp]
        public void SetUp()
        {
            caseFont = new Font("case");
            settingsFont = new Font("settings");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(caseFont);
            Object.DestroyImmediate(settingsFont);
        }

        [Test]
        public void A_case_without_an_override_uses_the_request_font_of_the_settings()
        {
            Assert.AreSame(settingsFont, RequestFontChoice.For(null, settingsFont));
        }

        [Test]
        public void A_case_with_an_override_uses_its_own_font()
        {
            Assert.AreSame(caseFont, RequestFontChoice.For(caseFont, settingsFont));
        }

        [Test]
        public void A_new_case_has_no_description_font_override()
        {
            var file = ScriptableObject.CreateInstance<CaseFile>();
            Assert.IsNull(file.DescriptionFontOverride);
            Object.DestroyImmediate(file);
        }

        [Test]
        public void The_override_set_on_a_case_is_read_back()
        {
            var file = ScriptableObject.CreateInstance<CaseFile>();
            var serialized = new SerializedObject(file);
            serialized.FindProperty("descriptionFontOverride").objectReferenceValue = caseFont;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreSame(caseFont, file.DescriptionFontOverride);
            Object.DestroyImmediate(file);
        }

        [Test]
        public void The_findings_slot_never_follows_the_description_override()
        {
            Assert.AreEqual(FontSlot.Findings, FontRoles.SlotOf(TextRole.Findings));
            Assert.AreEqual(FontSlot.Request, FontRoles.SlotOf(TextRole.ClientMessage));
            Assert.AreNotEqual(FontRoles.SlotOf(TextRole.Findings), FontRoles.SlotOf(TextRole.ClientMessage));
        }
    }
}
