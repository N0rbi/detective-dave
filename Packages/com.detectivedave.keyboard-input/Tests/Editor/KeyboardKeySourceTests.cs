using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DetectiveDave.KeyboardInput.Tests
{
    /// <summary>The component against a virtual keyboard. Edit Mode does not run Update / OnDisable, so the tests call Tick / ReleaseHeld.</summary>
    public class KeyboardKeySourceTests : InputTestFixture
    {
        Keyboard keyboard;
        GameObject host;
        KeyboardKeySource source;
        List<string> events;

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            host = new GameObject("Key source");
            source = host.AddComponent<KeyboardKeySource>();
            events = new List<string>();
            source.KeyPressed += key => events.Add($"Pressed {key}");
            source.KeyHeld += key => events.Add($"Held {key}");
            source.KeyReleased += key => events.Add($"Released {key}");
            source.RawKeyPressed += press => events.Add($"Raw {press.Key} mapped={press.IsMapped}");
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(host);
            base.TearDown();
        }

        [Test]
        public void BeforeBegin_NothingIsRaised()
        {
            Press(keyboard.aKey);
            source.Tick();
            Assert.That(events, Is.Empty);
        }

        [Test]
        public void Begin_RejectsNullAndASecondLayout()
        {
            Assert.Throws<ArgumentNullException>(() => source.Begin(null));
            source.Begin(TestLayouts.Load());
            Assert.Throws<InvalidOperationException>(() => source.Begin(TestLayouts.Load()));
        }

        [Test]
        public void PressHoldRelease_AndTheRawPressAfterTheLayoutEvents()
        {
            source.Begin(TestLayouts.Load());
            Press(keyboard.aKey);
            source.Tick();
            Assert.That(events, Is.EqualTo(new[] { "Pressed A", "Held A", "Raw A mapped=True" }));

            events.Clear();
            InputSystem.Update();
            source.Tick();
            Assert.That(events, Is.EqualTo(new[] { "Held A" }));

            events.Clear();
            Release(keyboard.aKey);
            source.Tick();
            Assert.That(events, Is.EqualTo(new[] { "Released A" }));
        }

        [Test]
        public void RawPress_CarriesTheLayoutEntry()
        {
            source.Begin(TestLayouts.Load());
            KeyPress raw = null;
            source.RawKeyPressed += press => raw = press;
            Press(keyboard.sKey);
            source.Tick();

            Assert.That(raw.Key, Is.EqualTo(Key.S));
            Assert.That(raw.Def.Label, Is.EqualTo("S"));
            Assert.That(raw.DisplayName, Is.Not.Null);
        }

        [Test]
        public void ThrowingListener_DoesNotStopTheOthers()
        {
            source.Begin(TestLayouts.Load());
            source.KeyPressed += _ => throw new InvalidOperationException("listener failed");
            var after = new List<Key>();
            source.KeyPressed += key => after.Add(key);
            LogAssert.Expect(LogType.Exception, new Regex("listener failed"));
            LogAssert.Expect(LogType.Exception, new Regex("listener failed"));

            Press(keyboard.aKey);
            Press(keyboard.sKey);
            source.Tick();

            Assert.That(after, Is.EqualTo(new[] { Key.A, Key.S }));
            Assert.That(events, Does.Contain("Held S"));
        }

        [Test]
        public void ReleaseHeld_ReleasesEveryKeyThatIsDown()
        {
            source.Begin(TestLayouts.Load());
            Press(keyboard.aKey);
            source.Tick();
            events.Clear();

            source.ReleaseHeld();
            Assert.That(events, Is.EqualTo(new[] { "Released A" }));

            events.Clear();
            InputSystem.Update();
            source.Tick();
            Assert.That(events, Is.EqualTo(new[] { "Pressed A", "Held A" }), "still down after re-enabling: a new press");
        }

        [Test]
        public void ListenerDisablingTheSource_StopsTheRestOfTheFrame()
        {
            source.Begin(TestLayouts.Load());
            source.KeyPressed += key => { if (key == Key.A) source.ReleaseHeld(); };
            Press(keyboard.aKey);
            Press(keyboard.sKey);
            source.Tick();

            Assert.That(events, Is.EqualTo(new[] { "Pressed A", "Released A" }));
        }

        [Test]
        public void NoKeyboard_ReleasesWhatWasDown()
        {
            source.Begin(TestLayouts.Load());
            Press(keyboard.aKey);
            source.Tick();
            events.Clear();

            InputSystem.RemoveDevice(keyboard);
            source.Tick();
            Assert.That(events, Is.EqualTo(new[] { "Released A" }));
        }
    }
}
