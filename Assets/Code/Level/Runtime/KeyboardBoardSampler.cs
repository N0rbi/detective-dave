using System.Collections.Generic;
using System.Linq;
using DetectiveDave.KeyboardGrid;
using DetectiveDave.KeyboardInput;
using DetectiveDave.Levels;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DetectiveDave.LevelPlay
{
    /// <summary>
    /// A test bench for the board and the level, nothing more. Pressing a key logs its tile and its neighbours' tiles and drops a
    /// marker (in the tile type's editor colour) on it until the key comes up. Every few seconds an "enemy" appears on a random
    /// walkable key and follows a path to another one. Listens to the key source; never calls into it.
    /// </summary>
    [RequireComponent(typeof(KeyboardBoard), typeof(KeyboardGridView), typeof(TileMapKeyboardMapper))]
    public sealed class KeyboardBoardSampler : MonoBehaviour
    {
        const float MarkerSize = 0.5f;
        const float EnemySize = 0.3f;
        const int MarkerSortingOrder = 5;

        static readonly Color EnemyColor = Color.white;
        static readonly Color EmptyColor = Color.gray;

        [SerializeField] Material markerMaterial;
        [SerializeField, Min(0.1f)] float enemyInterval = 2f;
        [SerializeField, Min(0.1f)] float enemySpeed = 3f;
        [SerializeField, Min(0)] int maxEnemies = 4;

        readonly Dictionary<Key, GameObject> markers = new Dictionary<Key, GameObject>();
        readonly Dictionary<PathWalker, GameObject> enemies = new Dictionary<PathWalker, GameObject>();
        readonly List<PathWalker> finished = new List<PathWalker>();
        IKeyboardBoard board;
        KeyboardGridView view;
        TileMapKeyboardMapper mapper;
        IKeyEventSource keys;
        Sprite square;
        float nextEnemyTime;

        void Awake()
        {
            board = GetComponent<KeyboardBoard>();
            view = GetComponent<KeyboardGridView>();
            mapper = GetComponent<TileMapKeyboardMapper>();
            keys = TryGetComponent(out KeyboardKeySource source) ? source : null;   // a real null: an interface hides Unity's fake null
            square = SpriteFactory.CreateUnitSquare();
        }

        void OnEnable()
        {
            if (keys == null) return;
            keys.KeyPressed += OnKeyPressed;
            keys.KeyReleased += OnKeyReleased;
        }

        void OnDisable()
        {
            if (keys == null) return;
            keys.KeyPressed -= OnKeyPressed;
            keys.KeyReleased -= OnKeyReleased;
        }

        void Update()
        {
            if (view.Layout == null) return;
            if (Time.time >= nextEnemyTime && enemies.Count < maxEnemies)
            {
                nextEnemyTime = Time.time + enemyInterval;
                SpawnEnemy();
            }
            MoveEnemies();
        }

        void OnKeyPressed(Key key)
        {
            var tile = mapper.GetTile(key);
            var around = string.Join(", ", board.GetNeighbours(key).Select(other => $"{other}:{TypeName(mapper.GetTile(other))}"));
            Debug.Log($"[Board] pressed {key}: {TypeName(tile)} (cell {tile.Cell}), {board.GetNeighbours(key).Count} neighbours -> {around}");

            if (!board.TryGetPosition(key, out var world)) return;
            Despawn(markers, key);
            markers[key] = Spawn($"Marker {key}", world, MarkerSize, tile.Type ? tile.Type.EditorColor : EmptyColor);
        }

        void OnKeyReleased(Key key)
        {
            Debug.Log($"[Board] released {key}");
            Despawn(markers, key);
        }

        void SpawnEnemy()
        {
            var walkable = view.Cells.Keys.Where(key => mapper.GetTile(key).IsWalkable).ToList();
            if (walkable.Count < 2) return;
            var origin = walkable[Random.Range(0, walkable.Count)];
            var target = walkable[Random.Range(0, walkable.Count)];
            board.TryGetPosition(origin, out var start);
            board.TryGetPosition(target, out var goal);

            var path = mapper.GetPath(start, goal);
            Debug.Log(path.Count > 1
                ? $"[Board] enemy {origin} -> {target}: {path.Count} tiles"
                : $"[Board] enemy {origin} -> {target}: no path");
            if (path.Count < 2) return;

            var walker = new PathWalker(path);
            enemies[walker] = Spawn($"Enemy {origin}->{target}", walker.Position, EnemySize, EnemyColor);
        }

        void MoveEnemies()
        {
            finished.Clear();
            foreach (var entry in enemies)
            {
                entry.Key.Advance(enemySpeed * Time.deltaTime);
                entry.Value.transform.position = entry.Key.Position;
                if (entry.Key.Done) finished.Add(entry.Key);
            }
            foreach (var walker in finished) Despawn(enemies, walker);
        }

        GameObject Spawn(string objectName, Vector2 world, float size, Color color)
        {
            var go = new GameObject(objectName);
            go.transform.position = world;
            go.transform.localScale = new Vector3(size, size, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = square;
            sr.color = color;
            sr.sortingOrder = MarkerSortingOrder;
            if (markerMaterial) sr.sharedMaterial = markerMaterial;
            return go;
        }

        static void Despawn<TKey>(Dictionary<TKey, GameObject> spawned, TKey key)
        {
            if (!spawned.TryGetValue(key, out var go)) return;
            Destroy(go);
            spawned.Remove(key);
        }

        static string TypeName(MapTile tile) => tile.Type ? tile.Type.name : "empty";
    }
}
