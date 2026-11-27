using System.Collections.Generic;

namespace DetectiveDave.EditorTools
{
    public readonly struct CaseNode
    {
        public string Id { get; }
        public IReadOnlyList<string> RequiredIds { get; }
        public bool IsOnDesk { get; }
        public bool HasLevel { get; }

        public CaseNode(string id, IReadOnlyList<string> requiredIds, bool isOnDesk, bool hasLevel)
        {
            Id = id;
            RequiredIds = requiredIds;
            IsOnDesk = isOnDesk;
            HasLevel = hasLevel;
        }
    }
}
