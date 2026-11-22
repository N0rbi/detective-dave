using System.Linq;
using DetectiveDave.Levels.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DetectiveDave.Levels.Tests
{
    public class LightingTests
    {
        LevelTestUtility make;

        [SetUp]
        public void SetUp() => make = new LevelTestUtility();

        [TearDown]
        public void TearDown() => make.Dispose();

        static LevelLightingSettings Settings(float intensity = 1f, bool flicker = false, float min = 4f, float max = 9f, params FlickerPattern[] patterns)
        {
            var settings = new LevelLightingSettings();
            Set(settings, "intensity", intensity);
            Set(settings, "flicker", flicker);
            Set(settings, "secondsBetweenFlickersMin", min);
            Set(settings, "secondsBetweenFlickersMax", max);
            Set(settings, "patterns", new System.Collections.Generic.List<FlickerPattern>(patterns));
            return settings;
        }

        static void Set<T>(LevelLightingSettings settings, string field, T value) =>
            typeof(LevelLightingSettings).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(settings, value);

        static FlickerPattern Pattern(float duration = 0.4f, float weight = 1f, params FlickerPoint[] points) =>
            new FlickerPattern("p", duration, points.Length == 0 ? new[] { new FlickerPoint(0f, 1f), new FlickerPoint(0.2f, 0f) } : points, weight);

        [Test]
        public void ALevelStartsWithLightingThatChangesNothing()
        {
            var lighting = make.Level().Lighting;
            Assert.AreEqual(1f, lighting.Intensity);
            Assert.AreEqual(Color.white, lighting.ColorTint);
            Assert.IsFalse(lighting.Flicker);
            Assert.IsEmpty(LightingProblems.Find(lighting));
        }

        [Test]
        public void TheDefaultLightingLeavesALightAsItIs()
        {
            var lighting = new LevelLightingSettings();
            var own = new Color(0.9f, 0.8f, 0.7f, 1f);
            Assert.AreEqual(0.6f, LightingMath.LightIntensity(0.6f, lighting, 1f), 0.0001f);
            Assert.AreEqual(own, LightingMath.LightColor(own, lighting));
        }

        [Test]
        public void IntensityAndFlickerBrightnessMultiplyTheLightsOwnIntensity() =>
            Assert.AreEqual(0.6f, LightingMath.LightIntensity(2f, Settings(intensity: 0.6f), 0.5f), 0.0001f);

        [Test]
        public void TheFlickerBrightnessIsKeptBetweenZeroAndOne() =>
            Assert.AreEqual(1f, LightingMath.LightIntensity(1f, Settings(), 3f), 0.0001f);

        [Test]
        public void ADarkLevelIsAWarning()
        {
            var problem = LightingProblems.Find(Settings(intensity: 0f)).Single();
            Assert.AreEqual(LightingProblemKind.LightsOff, problem.Kind);
            Assert.IsFalse(problem.IsError);
        }

        [Test]
        public void FlickerOnWithoutPatternsIsAnError()
        {
            var problem = LightingProblems.Find(Settings(flicker: true)).Single();
            Assert.AreEqual(LightingProblemKind.FlickerWithoutPatterns, problem.Kind);
            Assert.IsTrue(problem.IsError);
        }

        [Test]
        public void PatternsAreNotCheckedWhileFlickerIsOff() =>
            Assert.IsEmpty(LightingProblems.Find(Settings(flicker: false, patterns: new[] { Pattern(duration: 0f) })));

        [Test]
        public void AReversedWaitRangeIsAnError() =>
            Assert.IsTrue(LightingProblems.Find(Settings(flicker: true, min: 9f, max: 4f, patterns: new[] { Pattern() })).Any(problem => problem.Kind == LightingProblemKind.WaitRangeReversed && problem.IsError));

        [Test]
        public void EveryWeightAtZeroIsAnError() =>
            Assert.IsTrue(LightingProblems.Find(Settings(flicker: true, patterns: new[] { Pattern(weight: 0f) })).Any(problem => problem.Kind == LightingProblemKind.NoPatternCanBePicked));

        [Test]
        public void APatternWithoutDurationIsAnErrorThatNamesIt()
        {
            var problem = LightingProblems.Find(Settings(flicker: true, patterns: new[] { Pattern(), Pattern(duration: 0f) })).Single(found => found.Kind == LightingProblemKind.PatternWithoutDuration);
            Assert.AreEqual(1, problem.PatternIndex);
        }

        [Test]
        public void APatternWithoutPointsIsAWarning()
        {
            var empty = new FlickerPattern("empty", 0.4f, new FlickerPoint[0]);
            var problem = LightingProblems.Find(Settings(flicker: true, patterns: new[] { empty })).Single();
            Assert.AreEqual(LightingProblemKind.PatternWithoutPoints, problem.Kind);
            Assert.IsFalse(problem.IsError);
        }

        [Test]
        public void APointAfterTheEndIsAWarning() =>
            Assert.IsTrue(LightingProblems.Find(Settings(flicker: true, patterns: new[] { Pattern(0.4f, 1f, new FlickerPoint(0f, 1f), new FlickerPoint(0.9f, 0f)) })).Any(problem => problem.Kind == LightingProblemKind.PointAfterTheEnd));

        [Test]
        public void AGoodFlickerHasNoProblems() =>
            Assert.IsEmpty(LightingProblems.Find(Settings(flicker: true, patterns: new[] { Pattern() })));

        [Test]
        public void TheLevelChecksReportFlickerWithoutPatternsAndTheFixAddsAPattern()
        {
            var level = make.Level(keyArt: make.KeyArt());
            var serialized = new SerializedObject(level);
            serialized.FindProperty("lighting.flicker").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var issue = LevelValidator.Validate(level).Single(found => found.Message.StartsWith("Flicker is on but there is no flicker pattern"));
            Assert.AreEqual(MessageType.Error, issue.Severity);
            issue.Fix();

            Assert.AreEqual(1, level.Lighting.Patterns.Count);
            Assert.IsFalse(LevelValidator.Validate(level).Any(found => found.Message.StartsWith("Flicker is on")));
        }

        [Test]
        public void TheSwapFixPutsTheWaitsTheRightWayRound()
        {
            var level = make.Level(keyArt: make.KeyArt());
            var serialized = new SerializedObject(level);
            serialized.FindProperty("lighting.flicker").boolValue = true;
            serialized.FindProperty("lighting.secondsBetweenFlickersMin").floatValue = 9f;
            serialized.FindProperty("lighting.secondsBetweenFlickersMax").floatValue = 4f;
            FlickerPresets.Append(serialized.FindProperty("lighting.patterns"), FlickerPresets.All[0]);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            LevelValidator.Validate(level).Single(found => found.Message.StartsWith("The shortest wait")).Fix();

            Assert.AreEqual(4f, level.Lighting.SecondsBetweenFlickersMin);
            Assert.AreEqual(9f, level.Lighting.SecondsBetweenFlickersMax);
        }

        [Test]
        public void EveryPresetIsAGoodPattern()
        {
            foreach (var preset in FlickerPresets.All)
            {
                var problems = LightingProblems.Find(Settings(flicker: true, patterns: new[] { preset }));
                Assert.IsEmpty(problems, preset.Name);
                Assert.Greater(preset.Points.Count, 1, preset.Name);
            }
        }

        [Test]
        public void AppendingAPresetCopiesItsPointsAndDuration()
        {
            var level = make.Level(keyArt: make.KeyArt());
            var serialized = new SerializedObject(level);
            var preset = FlickerPresets.All[1];
            FlickerPresets.Append(serialized.FindProperty("lighting.patterns"), preset);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var added = level.Lighting.Patterns.Single();
            Assert.AreEqual(preset.Name, added.Name);
            Assert.AreEqual(preset.Duration, added.Duration);
            Assert.AreEqual(preset.Points.Count, added.Points.Count);
            Assert.AreEqual(preset.Points[2].Brightness, added.Points[2].Brightness);
        }
    }
}
