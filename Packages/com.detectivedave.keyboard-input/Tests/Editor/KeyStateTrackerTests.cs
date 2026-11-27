using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace DetectiveDave.KeyboardInput.Tests
{
    public class KeyStateTrackerTests
    {
        /// <summary>A hand-set frame of physical keys: which are down and which went down this frame.</summary>
        sealed class FakeReading : IKeyReading
        {
            readonly HashSet<Key> down = new HashSet<Key>();
            readonly HashSet<Key> tapped = new HashSet<Key>();

            public FakeReading Down(params Key[] keys) { down.UnionWith(keys); return this; }
            public FakeReading Tapped(params Key[] keys) { tapped.UnionWith(keys); return this; }

            public void Read(Key key, out bool isDown, out bool pressedThisFrame)
            {
                isDown = down.Contains(key);
                pressedThisFrame = tapped.Contains(key);
            }
        }

        KeyStateTracker tracker;

        [SetUp]
        public void SetUp() => tracker = new KeyStateTracker(TestLayouts.Load());

        string[] Step(FakeReading reading)
        {
            var output = new List<KeyTransition>();
            tracker.Step(reading, output);
            return output.Select(transition => transition.ToString()).ToArray();
        }

        static FakeReading Frame() => new FakeReading();

        [Test]
        public void KeyDown_IsPressedHeldThenReleased()
        {
            Assert.That(Step(Frame().Down(Key.A).Tapped(Key.A)), Is.EqualTo(new[] { "Pressed A", "Held A" }));
            Assert.That(Step(Frame().Down(Key.A)), Is.EqualTo(new[] { "Held A" }));
            Assert.That(Step(Frame()), Is.EqualTo(new[] { "Released A" }));
            Assert.That(Step(Frame()), Is.Empty);
        }

        [Test]
        public void KeyFoundDown_WithoutAPressThisFrame_IsStillPressed()
        {
            Assert.That(Step(Frame().Down(Key.A)), Is.EqualTo(new[] { "Pressed A", "Held A" }));
        }

        [Test]
        public void TapShorterThanAFrame_IsPressedAndReleasedInTheSameFrame_WithoutHeld()
        {
            Assert.That(Step(Frame().Tapped(Key.A)), Is.EqualTo(new[] { "Pressed A", "Released A" }));
            Assert.That(Step(Frame()), Is.Empty);
        }

        [Test]
        public void KeysOfOneFrame_ComeInLayoutOrder()
        {
            Assert.That(Step(Frame().Down(Key.S, Key.A)), Is.EqualTo(new[] { "Pressed A", "Held A", "Pressed S", "Held S" }));
        }

        [Test]
        public void Alias_CountsAsItsKey()
        {
            Assert.That(Step(Frame().Down(Key.NumpadEnter)), Is.EqualTo(new[] { "Pressed Enter", "Held Enter" }));
        }

        [Test]
        public void Alias_KeyStaysDownWhileAnySourceIsDown()
        {
            Step(Frame().Down(Key.Enter, Key.NumpadEnter));
            Assert.That(Step(Frame().Down(Key.NumpadEnter)), Is.EqualTo(new[] { "Held Enter" }));
            Assert.That(Step(Frame()), Is.EqualTo(new[] { "Released Enter" }));
        }

        [Test]
        public void InertKey_NeverGoesDown()
        {
            Assert.That(Step(Frame().Down(Key.Backquote).Tapped(Key.Backquote)), Has.None.EndsWith(" Backquote"));
        }

        [Test]
        public void InertKey_StillStandsInForTheKeyThatListsIt()
        {
            // Hungarian macOS: the physical Backquote key types "í", so the layout makes it inert and lists it under OEM1.
            Assert.That(Step(Frame().Down(Key.Backquote)), Is.EqualTo(new[] { "Pressed OEM1", "Held OEM1" }));
        }

        [Test]
        public void KeyNotInTheLayout_RaisesNothing()
        {
            Assert.That(Step(Frame().Down(Key.X).Tapped(Key.X)), Is.Empty);
        }

        [Test]
        public void CapsLock_AlreadyOnAtFirstLook_IsNotAPress()
        {
            Assert.That(Step(Frame().Down(Key.CapsLock)), Is.Empty);
            Assert.That(Step(Frame().Down(Key.CapsLock)), Is.Empty);
        }

        [Test]
        public void CapsLock_EverySwitch_IsAPressThatLastsUntilAnotherKeyIsPressed()
        {
            Step(Frame());                                                                       // first look: off
            Assert.That(Step(Frame().Down(Key.CapsLock)), Is.EqualTo(new[] { "Pressed CapsLock", "Held CapsLock" }));   // on
            Assert.That(Step(Frame().Down(Key.CapsLock)), Is.EqualTo(new[] { "Held CapsLock" }));
            Assert.That(Step(Frame().Down(Key.CapsLock).Down(Key.A)),
                Is.EqualTo(new[] { "Pressed A", "Held A", "Released CapsLock" }));               // another key: toggle keys come last
            Step(Frame().Down(Key.CapsLock));                                                    // A up
            Assert.That(Step(Frame()), Is.EqualTo(new[] { "Pressed CapsLock", "Held CapsLock" }));                       // off is a press too
        }

        [Test]
        public void CapsLock_AKeyThatWasAlreadyHeld_DoesNotEndTheLatch()
        {
            Step(Frame().Down(Key.A));
            Step(Frame().Down(Key.A).Down(Key.CapsLock));
            Assert.That(Step(Frame().Down(Key.A).Down(Key.CapsLock)), Is.EqualTo(new[] { "Held A", "Held CapsLock" }));
        }

        [Test]
        public void CapsLock_ATapEndsTheLatch()
        {
            Step(Frame());
            Step(Frame().Down(Key.CapsLock));
            Assert.That(Step(Frame().Down(Key.CapsLock).Tapped(Key.S)),
                Is.EqualTo(new[] { "Pressed S", "Released S", "Released CapsLock" }));
        }

        [Test]
        public void Reset_StartsOver_AKeyStillDownIsANewPress()
        {
            Step(Frame().Down(Key.A));
            tracker.Reset();
            Assert.That(Step(Frame().Down(Key.A)), Is.EqualTo(new[] { "Pressed A", "Held A" }));
        }

        [Test]
        public void Reset_MakesTheNextCapsLockReadingAFirstLook()
        {
            Step(Frame());
            Step(Frame().Down(Key.CapsLock));
            tracker.Reset();
            Assert.That(Step(Frame().Down(Key.CapsLock)), Is.Empty);
        }
    }
}
