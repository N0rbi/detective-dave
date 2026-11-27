using System;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [Serializable]
    public sealed class CaseLook
    {
        [SerializeField] [Tooltip("The request: the client's message on the left paper.")] ColorOverride descriptionColor = new ColorOverride();
        [SerializeField] [Tooltip("The investigation findings on the left paper.")] ColorOverride findingsColor = new ColorOverride();
        [SerializeField] [Tooltip("The letters the player has typed.")] ColorOverride typedColor = new ColorOverride();
        [SerializeField] [Tooltip("The grey waiting text that is not typed yet.")] ColorOverride placeholderColor = new ColorOverride();
        [SerializeField] [Tooltip("The accept answer that is not typed yet, until the player chooses.")] ColorOverride acceptPendingColor = new ColorOverride();
        [SerializeField] [Tooltip("The decline answer that is not typed yet, until the player chooses.")] ColorOverride declinePendingColor = new ColorOverride();

        public CaseLook()
        {
        }

        public CaseLook(ColorOverride description, ColorOverride findings, ColorOverride typed, ColorOverride placeholder, ColorOverride acceptPending, ColorOverride declinePending)
        {
            descriptionColor = description;
            findingsColor = findings;
            typedColor = typed;
            placeholderColor = placeholder;
            acceptPendingColor = acceptPending;
            declinePendingColor = declinePending;
        }

        public TextColorSet Resolve(TextColorSet fromSettings) => new TextColorSet(
            descriptionColor.Resolve(fromSettings.Description),
            findingsColor.Resolve(fromSettings.Findings),
            typedColor.Resolve(fromSettings.Typed),
            placeholderColor.Resolve(fromSettings.Placeholder),
            acceptPendingColor.Resolve(fromSettings.AcceptPending),
            declinePendingColor.Resolve(fromSettings.DeclinePending));
    }
}
