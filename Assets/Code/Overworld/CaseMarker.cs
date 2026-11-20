using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [DisallowMultipleComponent]
    public sealed class CaseMarker : MonoBehaviour
    {
        const int ArtSortingOrder = 5;

        [SerializeField] [Tooltip("The case this file on the desk opens. Place and rotate and scale this object freely; the keys under it belong to the case.")] CaseFile caseFile;
        [SerializeField] [Tooltip("The picture of the file. Its size and the scale of this object decide which keys it covers.")] SpriteRenderer art;

        public CaseFile CaseFile => caseFile;

        SpriteRenderer Art => art ? art : art = GetComponent<SpriteRenderer>();

        public DeskMarker ToDeskMarker(Transform grid)
        {
            var size = Vector2.Scale(Art.sprite.bounds.size, transform.lossyScale) / grid.lossyScale.x;
            var centre = grid.InverseTransformPoint(transform.position);
            return new DeskMarker(caseFile.Id, centre, size, transform.eulerAngles.z - grid.eulerAngles.z);
        }

        public void Show(CaseState state, Sprite folder, Sprite solved, float lockedShade)
        {
            Art.sprite = state == CaseState.Solved && solved ? solved : folder;
            Art.sortingOrder = ArtSortingOrder;
            Art.color = state == CaseState.Locked ? ShadeTint(lockedShade) : Color.white;
        }

        static Color ShadeTint(float shade)
        {
            float brightness = 1f - Mathf.Clamp01(shade);
            return new Color(brightness, brightness, brightness, 1f);
        }
    }
}
