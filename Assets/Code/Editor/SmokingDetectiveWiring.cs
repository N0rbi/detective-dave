using System.Linq;
using DetectiveDave.OverworldPlay;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DetectiveDave.EditorTools
{
    static class SmokingDetectiveWiring
    {
        const string ObjectName = "Smoking Detective";
        const string ReplacedSceneObjectName = "stealth_detective_walk_cycle_2";
        const string ArmObjectName = "Arm";
        const string ArmPivotName = "Arm Pivot";
        const int BodySortingOrder = 10000;
        static readonly Vector3 DefaultPosition = new Vector3(7.04f, -12.06f, 0f);
        const float DefaultScale = 10.95f;

        public static void Ensure(Scene scene, SmokingSprites sprites)
        {
            var existing = scene.GetRootGameObjects().Select(root => root.GetComponentInChildren<SmokingDetective>(true)).FirstOrDefault(found => found);
            var detective = existing ? existing.gameObject : Create(scene);
            FillSprites(detective, sprites);
            RemoveReplacedSceneObject(scene, detective);
        }

        static GameObject Create(Scene scene)
        {
            var replaced = FindReplaced(scene);
            var detective = new GameObject(ObjectName);
            SceneManager.MoveGameObjectToScene(detective, scene);
            detective.transform.position = replaced ? replaced.transform.position : DefaultPosition;
            detective.transform.localScale = replaced ? replaced.transform.localScale : Vector3.one * DefaultScale;
            var body = detective.AddComponent<SpriteRenderer>();
            body.sortingOrder = replaced && replaced.TryGetComponent<SpriteRenderer>(out var old) ? old.sortingOrder : BodySortingOrder;
            var pivot = new GameObject(ArmPivotName).transform;
            pivot.SetParent(detective.transform, false);
            var arm = new GameObject(ArmObjectName).AddComponent<SpriteRenderer>();
            arm.transform.SetParent(pivot, false);
            arm.sortingOrder = body.sortingOrder + 1;
            var component = detective.AddComponent<SmokingDetective>();
            var serialized = new SerializedObject(component);
            serialized.FindProperty("armPivot").objectReferenceValue = pivot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return detective;
        }

        static void FillSprites(GameObject detective, SmokingSprites sprites)
        {
            var body = detective.GetComponent<SpriteRenderer>();
            if (body && !body.sprite) body.sprite = sprites.Body;
            var armRenderer = detective.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(renderer => renderer.gameObject != detective);
            if (!armRenderer || armRenderer.sprite) return;
            armRenderer.sprite = sprites.Arm;
            PlaceArmPivotAtSleeve(armRenderer.transform.parent, armRenderer);
        }

        // The arm art is a long sleeve ending at the bottom of the picture, so it turns best around its bottom edge.
        static void PlaceArmPivotAtSleeve(Transform pivot, SpriteRenderer arm)
        {
            var halfHeight = arm.sprite.bounds.size.y * 0.5f;
            pivot.localPosition = new Vector3(0f, -halfHeight, 0f);
            arm.transform.localPosition = new Vector3(0f, halfHeight, 0f);
        }

        static GameObject FindReplaced(Scene scene) =>
            scene.GetRootGameObjects().FirstOrDefault(root => root.name == ReplacedSceneObjectName);

        static void RemoveReplacedSceneObject(Scene scene, GameObject detective)
        {
            var replaced = FindReplaced(scene);
            if (replaced && replaced != detective) Object.DestroyImmediate(replaced);
        }
    }
}
