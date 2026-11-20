using DetectiveDave.KeyboardGrid;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    [DisallowMultipleComponent]
    public sealed class CaseMarker : MonoBehaviour
    {
        const string ShadeName = "Locked Shade";
        const int ArtSortingOrder = 5;
        const int ShadeSortingOffset = 1;

        [SerializeField] [Tooltip("The case this file on the desk opens. Place and rotate and scale this object freely; the keys under it belong to the case.")] CaseFile caseFile;
        [SerializeField] [Tooltip("The picture of the file. Its size and the scale of this object decide which keys it covers.")] SpriteRenderer art;

        SpriteRenderer shade;

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
            ShowShade(state == CaseState.Locked, lockedShade);
        }

        void ShowShade(bool shown, float alpha)
        {
            if (!shade) shade = CreateShade();
            shade.gameObject.SetActive(shown);
            shade.color = new Color(0f, 0f, 0f, alpha);
        }

        SpriteRenderer CreateShade()
        {
            var shadeObject = new GameObject(ShadeName);
            shadeObject.transform.SetParent(transform, false);
            shadeObject.transform.localScale = new Vector3(Art.sprite.bounds.size.x, Art.sprite.bounds.size.y, 1f);
            var renderer = shadeObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.CreateUnitSquare();
            renderer.sortingOrder = Art.sortingOrder + ShadeSortingOffset;
            return renderer;
        }
    }
}
