using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace DetectiveDave.PlayerPlay
{
    public sealed class SpiralScreenWipe : MonoBehaviour, IScreenWipe
    {
        const int SortingOrder = 30000;
        const string UrpSpriteShader = "Universal Render Pipeline/2D/Sprite-Unlit-Default";
        const string BuiltInSpriteShader = "Sprites/Default";

        readonly List<GameObject> wedges = new List<GameObject>();
        readonly List<Mesh> meshes = new List<Mesh>();
        PlayerMovementSettings settings;
        ICameraZoom zoom;
        Material material;
        Tween sweep;
        int visibleCount;

        public void Configure(PlayerMovementSettings movementSettings, ICameraZoom cameraZoom)
        {
            settings = movementSettings;
            zoom = cameraZoom;
        }

        public void Out(Vector2 centre, Action done) => Sweep(centre, zoom.CurrentBounds(), 0f, 1f, done);

        public void In(Vector2 centre, Action done) => Sweep(centre, zoom.ReachableBounds(), 1f, 0f, done);

        public void Cancel()
        {
            KillSweep();
            ShowProgress(0f);
        }

        void OnDestroy()
        {
            KillSweep();
            DestroyWedges();
            if (material) Destroy(material);
        }

        void Sweep(Vector2 centre, ScreenRect screen, float from, float to, Action done)
        {
            KillSweep();
            BuildWedges(centre, screen);
            ShowProgress(from);
            sweep = DOVirtual.Float(from, to, settings.WipeSeconds, ShowProgress)
                .SetEase(Ease.Linear)
                .SetLink(gameObject)
                .OnComplete(() => Finish(done));
        }

        void Finish(Action done)
        {
            sweep = null;
            done?.Invoke();
        }

        void KillSweep()
        {
            sweep?.Kill();
            sweep = null;
        }

        void ShowProgress(float progress)
        {
            int target = SpiralGeometry.VisibleCount(progress, wedges.Count);
            for (int index = 0; index < wedges.Count; index++) wedges[index].SetActive(index < target);
            visibleCount = target;
        }

        void BuildWedges(Vector2 centre, ScreenRect screen)
        {
            DestroyWedges();
            foreach (var strip in SpiralGeometry.StripsOverScreen(centre, screen, settings.WipeSegmentsPerTurn, settings.WipeTurns))
                CreateWedge(centre, strip);
            visibleCount = 0;
        }

        void CreateWedge(Vector2 centre, Vector2[] strip)
        {
            var wedge = new GameObject("Wedge " + wedges.Count);
            wedge.transform.SetParent(transform, false);
            wedge.transform.position = new Vector3(centre.x, centre.y, 0f);
            wedge.AddComponent<MeshFilter>().sharedMesh = CreateStripMesh(strip);
            var renderer = wedge.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = WipeMaterial();
            renderer.sortingOrder = SortingOrder;
            wedge.SetActive(false);
            wedges.Add(wedge);
        }

        Mesh CreateStripMesh(Vector2[] strip)
        {
            var mesh = new Mesh { name = "Wipe strip" };
            mesh.vertices = Array.ConvertAll(strip, corner => new Vector3(corner.x, corner.y, 0f));
            mesh.triangles = FanTriangles(strip);
            mesh.colors = Array.ConvertAll(strip, _ => settings.WipeColor);
            mesh.RecalculateBounds();
            meshes.Add(mesh);
            return mesh;
        }

        static int[] FanTriangles(Vector2[] strip)
        {
            bool counterClockwise = SignedArea(strip) > 0f;
            var indices = new List<int>();
            for (int corner = 1; corner < strip.Length - 1; corner++)
            {
                indices.Add(0);
                indices.Add(counterClockwise ? corner : corner + 1);
                indices.Add(counterClockwise ? corner + 1 : corner);
            }
            return indices.ToArray();
        }

        static float SignedArea(Vector2[] polygon)
        {
            float area = 0f;
            for (int index = 0; index < polygon.Length; index++)
            {
                var current = polygon[index];
                var next = polygon[(index + 1) % polygon.Length];
                area += current.x * next.y - next.x * current.y;
            }
            return area;
        }

        Material WipeMaterial()
        {
            if (material) return material;
            var shader = Shader.Find(UrpSpriteShader) ?? Shader.Find(BuiltInSpriteShader);
            material = new Material(shader);
            return material;
        }

        void DestroyWedges()
        {
            foreach (var wedge in wedges) Destroy(wedge);
            foreach (var mesh in meshes) Destroy(mesh);
            wedges.Clear();
            meshes.Clear();
        }
    }
}
