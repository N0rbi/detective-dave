using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.Levels.Tests
{
    /// <summary>
    /// After the 0.2.0 migration: no scene or prefab in the project has a missing script, every script reference in any asset
    /// (ScriptableObjects included) resolves, and no asset still points at a file the migration removed. These read the real project, so they guard against a half-done migration on any machine.
    /// </summary>
    public class ProjectIntegrityTests
    {
        // GUIDs of files removed by 0.2.0. Any reference to them is dangling.
        static readonly Dictionary<string, string> RemovedGuids = new Dictionary<string, string>
        {
            ["10b4356668214dd5859817766c56f79e"] = "TerrainTile.cs",
            ["fd9efa57ae214a5b9c1b0e3ea4c4be18"] = "TileMapGroup.cs",
            ["3cf745e30c7104a5e96dc07c0b1eb064"] = "Assets/Level/Grid.prefab",
            ["2f479883eedc4cd4a35184000c308c4b"] = "SampleLevel.asset",
            ["614e065ccc83541249016690d1bfc997"] = "TestLevel.asset",
            ["9b35d47bc25746b2b3931761b1920a96"] = "Data/Walkable.asset",
            ["7470d87e8d55429e863144240be998bb"] = "Data/Gap.asset",
            ["69a65fd9d2b543ebb814ba1301e29d7b"] = "Data/Wall.asset",
            ["95ac8a0e30a14e61b75e95702418446c"] = "Data/WallGutter.asset",
            ["3ee672c23f272499c9d9780738a0d402"] = "Data/Start.asset",
            ["33e2a864fc2f44d37b97b98fade09118"] = "Data/Goal.asset",
        };

        static IEnumerable<string> ProjectFiles(params string[] extensions) =>
            Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories)
                .Where(path => extensions.Contains(Path.GetExtension(path)))
                .Select(path => path.Replace('\\', '/'));

        [Test]
        public void NoAssetReferencesARemovedFile()
        {
            var dangling = new List<string>();
            foreach (var path in ProjectFiles(".unity", ".prefab", ".asset"))
            {
                var text = File.ReadAllText(path);
                dangling.AddRange(RemovedGuids.Where(removed => text.Contains(removed.Key)).Select(removed => $"{path} -> {removed.Value}"));
            }
            CollectionAssert.IsEmpty(dangling);
        }

        // m_Script lines of MonoBehaviours and ScriptableObjects in text-serialized assets.
        static readonly System.Text.RegularExpressions.Regex ScriptReference =
            new System.Text.RegularExpressions.Regex(@"m_Script: \{fileID: -?\d+, guid: ([0-9a-f]{32}), type: 3\}");

        [Test]
        public void EveryScriptReferenceResolves()
        {
            var missing = new List<string>();
            foreach (var path in ProjectFiles(".unity", ".prefab", ".asset"))
                foreach (System.Text.RegularExpressions.Match match in ScriptReference.Matches(File.ReadAllText(path)))
                    if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(match.Groups[1].Value)))
                        missing.Add($"{path} -> script {match.Groups[1].Value}");
            CollectionAssert.IsEmpty(missing.Distinct().ToList());
        }

        [Test]
        public void NoPrefabHasAMissingScript()
        {
            var broken = ProjectFiles(".prefab")
                .Select(path => (path, root: AssetDatabase.LoadAssetAtPath<GameObject>(path)))
                .Where(entry => entry.root && MissingScripts(entry.root) > 0)
                .Select(entry => entry.path)
                .ToList();
            CollectionAssert.IsEmpty(broken);
        }

        [Test]
        public void NoSceneHasAMissingScript()
        {
            var broken = new List<string>();
            foreach (var path in ProjectFiles(".unity"))
            {
                var scene = SceneManager.GetSceneByPath(path);
                bool openedHere = !scene.isLoaded;
                if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    if (scene.GetRootGameObjects().Sum(MissingScripts) > 0) broken.Add(path);
                }
                finally
                {
                    if (openedHere) EditorSceneManager.CloseScene(scene, true);
                }
            }
            CollectionAssert.IsEmpty(broken);
        }

        static int MissingScripts(GameObject root) =>
            root.GetComponentsInChildren<Transform>(true).Sum(child => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject));
    }
}
