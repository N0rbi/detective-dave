using System.Collections.Generic;

namespace DetectiveDave.EditorTools
{
    public static class MarkerParentChoice
    {
        public static T Pick<T>(T context, T openInPrefabMode, T selected, IReadOnlyList<T> inScene) where T : class
        {
            if (context != null) return context;
            if (openInPrefabMode != null) return openInPrefabMode;
            if (selected != null) return selected;
            return inScene.Count == 1 ? inScene[0] : null;
        }
    }
}
