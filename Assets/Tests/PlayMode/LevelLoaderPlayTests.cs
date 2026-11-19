using System;
using System.Collections;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace DetectiveDave.Tests
{
    /// <summary>
    /// The game scene with a level handed over before Start, the way Play This Level does it: the keyboard's key source is started exactly once,
    /// the loader plays the handed level instead of its sequence, and reloading the level leaves the keyboard alone.
    /// Needs Game.unity in the build list (the editor adds it before Play).
    /// </summary>
    public class LevelLoaderPlayTests
    {
        const string GameScene = "Game";

        // A small 15 x 4 layout so the test does not depend on the layout files.
        const string LayoutJson = @"{ ""id"": ""play-test"", ""name"": ""Play Test"", ""form"": ""ansi"", ""digit"": 9, ""unitsWide"": 15, ""unitsHigh"": 4,
  ""keys"": [ { ""code"": ""KeyA"", ""label"": ""A"", ""rects"": [ { ""x"": 0, ""y"": 0, ""w"": 1, ""h"": 1 } ], ""center"": { ""x"": 0.5, ""y"": 0.5 } } ] }";

        LevelLoader loader;
        Level handed;

        [UnitySetUp]
        public IEnumerator LoadGameScene()
        {
            if (!KeyboardSession.IsLocked) KeyboardSession.Lock(KeyboardLayout.Parse(LayoutJson));
            handed = new GameObject("Handed Level").AddComponent<Level>();     // a level with no shape: plays as "shape is empty"
            handed.gameObject.SetActive(false);                                // a stand-in for a prefab asset: only instantiated
            Object.DontDestroyOnLoad(handed.gameObject);

            SceneManager.sceneLoaded += HandOver;
            yield return SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Single);
            yield return null;     // Start has run
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.sceneLoaded -= HandOver;
            if (handed) Object.Destroy(handed.gameObject);
        }

        // After Awake, before Start: exactly where Play This Level hands its level over.
        void HandOver(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= HandOver;
            loader = Object.FindFirstObjectByType<LevelLoader>();
            Assert.That(loader, Is.Not.Null, "Game.unity has a Level Loader");
            loader.Load(handed);
        }

        [Test]
        public void HandedLevel_IsPlayed_InsteadOfTheSequence()
        {
            Assert.That(loader.Source, Is.SameAs(handed));
            Assert.That(loader.Current, Is.Not.Null);
            Assert.That(loader.Current.transform.parent, Is.SameAs(loader.transform), "the level sits on the keyboard");
        }

        [Test]
        public void KeySource_IsStartedExactlyOnce_EvenAfterReloads()
        {
            loader.Reload();
            loader.Load(handed);
            var source = loader.GetComponent<KeyboardKeySource>();
            Assert.That(source, Is.Not.Null);
            // Begin throws on a second call: so it ran exactly once if it throws now and nothing threw before (an exception during
            // the scene's Start or the reloads would have failed this test through the log).
            Assert.Throws<InvalidOperationException>(() => source.Begin(KeyboardLayout.Parse(LayoutJson)));
        }

        [Test]
        public void Reload_ReplacesOnlyTheLevel()
        {
            var view = loader.GetComponent<KeyboardGridView>();
            int cells = view.Cells.Count;
            var before = loader.Current;
            loader.Reload();
            Assert.That(loader.Current, Is.Not.SameAs(before));
            Assert.That(view.Cells.Count, Is.EqualTo(cells), "the keyboard was not rebuilt");
        }
    }
}
