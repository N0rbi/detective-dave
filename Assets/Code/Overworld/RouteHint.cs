using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace DetectiveDave.OverworldPlay
{
    public sealed class RouteHint
    {
        const int FewestKeysForALine = 2;
        static readonly IReadOnlyList<Key> NoKeys = new Key[0];

        readonly DeskRoute route;
        readonly RouteHintVisibility visibility;

        public RouteHint(DeskRoute route, RouteHintVisibility visibility)
        {
            this.route = route;
            this.visibility = visibility;
            Keys = NoKeys;
        }

        public IReadOnlyList<Key> Keys { get; private set; }

        public bool HasLine => Keys.Count >= FewestKeysForALine;

        public void Begin(Key startKey) => Keys = LineFrom(startKey);

        public void OnMoved(Key key) => Keys = visibility == RouteHintVisibility.UntilFirstMove ? NoKeys : LineFrom(key);

        IReadOnlyList<Key> LineFrom(Key key)
        {
            var path = route.ToNearestOpenCase(key);
            return path.Count >= FewestKeysForALine ? path : NoKeys;
        }
    }
}
