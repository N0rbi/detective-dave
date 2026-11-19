using System.Collections.Generic;
using DetectiveDave.LevelPlay;
using DetectiveDave.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class LevelSequenceTests
    {
        readonly List<Object> created = new List<Object>();
        LevelSequence sequence;
        Level first, second;

        [SetUp]
        public void SetUp()
        {
            first = NewLevel("First");
            second = NewLevel("Second");
            sequence = ScriptableObject.CreateInstance<LevelSequence>();
            created.Add(sequence);
            var serialized = new SerializedObject(sequence);
            var levels = serialized.FindProperty("levels");
            levels.arraySize = 3;
            levels.GetArrayElementAtIndex(0).objectReferenceValue = null;      // an empty entry is skipped
            levels.GetArrayElementAtIndex(1).objectReferenceValue = first;
            levels.GetArrayElementAtIndex(2).objectReferenceValue = second;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var item in created) Object.DestroyImmediate(item);
            created.Clear();
        }

        Level NewLevel(string name)
        {
            var go = new GameObject(name);
            created.Add(go);
            return go.AddComponent<Level>();
        }

        [Test]
        public void Levels_KeepTheirOrder()
        {
            Assert.That(sequence.Levels, Is.EqualTo(new Level[] { null, first, second }));
        }

        [Test]
        public void First_SkipsEmptyEntries()
        {
            Assert.That(sequence.First, Is.SameAs(first));
        }

        [Test]
        public void After_GoesToTheNextLevel_AndEndsWithNull()
        {
            Assert.That(sequence.After(first), Is.SameAs(second));
            Assert.That(sequence.After(second), Is.Null);
        }

        [Test]
        public void IndexOf_IsMinusOneForALevelNotInTheSequence()
        {
            Assert.That(sequence.IndexOf(second), Is.EqualTo(2));
            Assert.That(sequence.IndexOf(NewLevel("Other")), Is.EqualTo(-1));
            Assert.That(sequence.IndexOf(null), Is.EqualTo(-1));
        }

        [Test]
        public void EmptySequence_HasNoFirstLevel()
        {
            var empty = ScriptableObject.CreateInstance<LevelSequence>();
            created.Add(empty);
            Assert.That(empty.First, Is.Null);
        }
    }
}
