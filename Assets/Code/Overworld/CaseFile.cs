using System.Collections.Generic;
using System.Linq;
using DetectiveDave.Levels;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [CreateAssetMenu(menuName = "Detective Dave/Case", fileName = "New Case.case.asset")]
    public sealed class CaseFile : ScriptableObject
    {
        [SerializeField] [Tooltip("Lock id: the stable name used in the save file. Filled once and never follows a rename of the asset. Do not change it once players have progress.")] string caseId;
        [SerializeField] [Tooltip("Who wrote to the detective. Replaces {client} in every text of this case.")] string clientName;
        [SerializeField] [Tooltip("More names for the texts: a suspect with key husband replaces {husband}.")] SuspectEntry[] suspects = new SuspectEntry[0];
        [SerializeField, TextArea(3, 8)] [Tooltip("Typed in real time on the left of the screen when the player stays on the case file.")] string description;
        [SerializeField] [Tooltip("Optional. Draws the description (the client's message) of this case in this font instead of the Request Font on Overworld Settings. The findings are not affected. Use a monospaced font.")] Font descriptionFontOverride;
        [SerializeField, TextArea(2, 4)] [Tooltip("The answer when the detective takes the case. It may be the start of the decline message: then the player presses Enter or Space after it to choose it.")] string acceptMessage;
        [SerializeField, TextArea(2, 4)] [Tooltip("The answer when the detective turns the case down. It may be the start of the accept message: then the player presses Enter or Space after the short one to choose it.")] string declineMessage;
        [SerializeField, TextArea(4, 12)] [Tooltip("What the detective found. Appears under the description on the left after the level is solved, typed in at the same speed.")] string findings;
        [SerializeField, TextArea(2, 8)] [Tooltip("The answer the player types on the right paper after reading the findings. Completing it solves the case.")] string response;
        [SerializeField] [Tooltip("The level played when the case is accepted.")] Level level;
        [SerializeField] [Tooltip("Cases that must be solved before this one opens.")] CaseFile[] requires = new CaseFile[0];
        [Header("Music")]
        [SerializeField] [Tooltip("Optional music that plays over the jazz loop while this case is open.")] CaseMusic music = new CaseMusic();

        [Header("Look")]
        [SerializeField] [Tooltip("Optional colours for the texts of this case. Each line takes its colour from Overworld Settings until you tick it.")] CaseLook look = new CaseLook();

#if UNITY_EDITOR
        void Reset() => caseId = CaseIdRules.NewId();
#endif

        public string Id => CaseIdRules.StoredOrAssetName(caseId, name);
        public string ClientName => clientName;
        public IReadOnlyList<SuspectEntry> Suspects => suspects;
        public Font DescriptionFontOverride => descriptionFontOverride;
        public CaseLook Look => look;
        public CaseMusic Music => music;
        public Level Level => level;
        public bool HasLevel => level;
        public IReadOnlyList<CaseFile> Requires => requires;
        public IReadOnlyList<string> RequiredIds => requires.Where(required => required).Select(required => required.Id).ToArray();

        public string ResolvedDescription => Resolve(description);
        public string ResolvedFindings => Resolve(findings);
        public string ResolvedResponse => Resolve(response);
        public string ResolvedAcceptMessage => Resolve(acceptMessage);
        public string ResolvedDeclineMessage => Resolve(declineMessage);
        public string ResolvedAcceptLetter => LetterText.Compose(clientName, Resolve(acceptMessage));
        public string ResolvedDeclineLetter => LetterText.Compose(clientName, Resolve(declineMessage));

        string Resolve(string text) => TextTemplate.Resolve(text, clientName, suspects);
    }
}
