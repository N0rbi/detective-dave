using System.Collections.Generic;

namespace DetectiveDave.EditorTools
{
    static class KeyArtShapeFilter
    {
        public static List<int> SingleKeyIndicesLastFirst(IReadOnlyList<bool> wholeKeyFlags)
        {
            var indices = new List<int>();
            for (int index = wholeKeyFlags.Count - 1; index >= 0; index--)
                if (!wholeKeyFlags[index]) indices.Add(index);
            return indices;
        }
    }
}
