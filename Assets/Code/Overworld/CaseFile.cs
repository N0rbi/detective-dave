using System.Collections.Generic;
using System.Linq;
using DetectiveDave.Levels;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Case", fileName = "New Case.case")]
    public sealed class CaseFile : ScriptableObject
    {
        [SerializeField] [Tooltip("Stable name used in the save file. Leave empty to use the asset name. Do not change it once players have progress.")] string caseId;
        [SerializeField] [Tooltip("Who wrote to the detective. Replaces {client} in every text of this case.")] string clientName;
        [SerializeField] [Tooltip("More names for the texts: a suspect with key husband replaces {husband}.")] SuspectEntry[] suspects = new SuspectEntry[0];
        [SerializeField, TextArea(3, 8)] [Tooltip("Typed in real time on the left of the screen when the player stays on the case file.")] string description;
        [SerializeField, TextArea(2, 4)] [Tooltip("The answer when the detective takes the case. It must start differently from the decline message.")] string acceptMessage;
        [SerializeField, TextArea(2, 4)] [Tooltip("The answer when the detective turns the case down. It must start differently from the accept message.")] string declineMessage;
        [SerializeField, TextArea(4, 12)] [Tooltip("The story the player types after solving the level.")] string findings;
        [SerializeField] [Tooltip("The level played when the case is accepted.")] Level level;
        [SerializeField] [Tooltip("Cases that must be solved before this one opens.")] CaseFile[] requires = new CaseFile[0];

        public string Id => string.IsNullOrWhiteSpace(caseId) ? name : caseId;
        public string ClientName => clientName;
        public IReadOnlyList<SuspectEntry> Suspects => suspects;
        public Level Level => level;
        public IReadOnlyList<CaseFile> Requires => requires;
        public IReadOnlyList<string> RequiredIds => requires.Where(required => required).Select(required => required.Id).ToArray();

        public string ResolvedDescription => Resolve(description);
        public string ResolvedFindings => Resolve(findings);
        public string ResolvedAcceptMessage => Resolve(acceptMessage);
        public string ResolvedDeclineMessage => Resolve(declineMessage);
        public string ResolvedAcceptLetter => LetterText.Compose(clientName, Resolve(acceptMessage));
        public string ResolvedDeclineLetter => LetterText.Compose(clientName, Resolve(declineMessage));

        string Resolve(string text) => TextTemplate.Resolve(text, clientName, suspects);
    }
}
