using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DetectiveDave.KeyboardInput.Tests
{
    public class KeyboardKeySourceFrameTests : InputTestFixture
    {
        const string LayoutJson = @"{
  ""id"": ""test"", ""name"": ""Test"", ""form"": ""iso"", ""digit"": 3, ""unitsWide"": 1, ""unitsHigh"": 1,
  ""keys"": [
    { ""code"": ""A"", ""label"": ""A"", ""rects"": [ { ""x"": 0, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 0.5, ""y"": 0.5 } }
  ]
}";

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
            source.RawKeyPressed += press => events.Add($"Raw {press.Key} mapped={press.IsMapped}");
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(host);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator UnmappedKey_IsRawOnly_AndLoggedOnce()
        {
            source.Begin(KeyboardLayout.Parse(LayoutJson));
            LogAssert.Expect(LogType.Log, new Regex(@"^\[Keyboard\] Not in layout 'test': Key\.X"));

            Press(keyboard.xKey);
            yield return null;
            Release(keyboard.xKey);
            yield return null;
            Press(keyboard.xKey);
            yield return null;
            Release(keyboard.xKey);
            yield return null;

            Assert.That(events, Is.EqualTo(new[] { "Raw X mapped=False", "Raw X mapped=False" }));
        }
    }
}
