using DetectiveDave.OverworldPlay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Tests
{
    public class CaseFileTests
    {
        CaseFile file;
        SerializedObject serialized;

        [SetUp]
        public void SetUp()
        {
            file = ScriptableObject.CreateInstance<CaseFile>();
            file.name = "Jealous Wife";
            serialized = new SerializedObject(file);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(file);

        void Set(string field, string value)
        {
            serialized.FindProperty(field).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        void AddSuspect(string key, string value)
        {
            var list = serialized.FindProperty("suspects");
            list.arraySize++;
            var entry = list.GetArrayElementAtIndex(list.arraySize - 1);
            entry.FindPropertyRelative("key").stringValue = key;
            entry.FindPropertyRelative("value").stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void IdFallsBackToTheAssetName() => Assert.AreEqual("Jealous Wife", file.Id);

        [Test]
        public void ExplicitIdWins()
        {
            Set("caseId", "case-01");
            Assert.AreEqual("case-01", file.Id);
        }

        [Test]
        public void LettersStartWithTheClientAndEndWithTheSignature()
        {
            Set("clientName", "Ann");
            Set("acceptMessage", "I will look into this.");
            Set("declineMessage", "I cannot help.");
            StringAssert.StartsWith("Dear Ann,", file.ResolvedAcceptLetter);
            StringAssert.Contains("I will look into this.", file.ResolvedAcceptLetter);
            StringAssert.EndsWith("Detective Dave", file.ResolvedDeclineLetter);
        }

        [Test]
        public void ClientAndSuspectsAreSubstitutedInEveryText()
        {
            Set("clientName", "Ann");
            AddSuspect("husband", "Bob");
            Set("description", "{client} doubts {husband}.");
            Set("findings", "{husband} was at the club.");
            Set("response", "{client}, {husband} was faithful.");
            Assert.AreEqual("Ann doubts Bob.", file.ResolvedDescription);
            Assert.AreEqual("Bob was at the club.", file.ResolvedFindings);
            Assert.AreEqual("Ann, Bob was faithful.", file.ResolvedResponse);
        }

        [Test]
        public void RequiredIdsSkipEmptySlots()
        {
            var other = ScriptableObject.CreateInstance<CaseFile>();
            other.name = "First";
            var list = serialized.FindProperty("requires");
            list.arraySize = 2;
            list.GetArrayElementAtIndex(0).objectReferenceValue = other;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            CollectionAssert.AreEqual(new[] { "First" }, file.RequiredIds);
            Object.DestroyImmediate(other);
        }
    }
}
