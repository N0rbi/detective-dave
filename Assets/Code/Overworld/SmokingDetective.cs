using DG.Tweening;
using UnityEngine;

namespace DetectiveDave.OverworldPlay
{
    /// <summary>
    /// The detective smoking on the overworld. Put it on the object that holds the body sprite, which stays still; the arm sprite lives under Arm Pivot.
    /// Every Seconds Between Puffs the pivot moves and turns to the mouth, waits Smoke Seconds with a small tilt, then goes back.
    /// Move the Arm Pivot object (not the arm sprite under it) to change where the arm turns.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Detective Dave/Smoking Detective")]
    public sealed class SmokingDetective : MonoBehaviour
    {
        public const string ArmPivotName = "Arm Pivot";

        [Header("Parts")]
        [SerializeField] [Tooltip("The parent of the arm sprite. This is what moves and turns, so its position is the arm's pivot.")] Transform armPivot;

        [Header("Timing")]
        [SerializeField, Min(0f)] [Tooltip("Seconds the arm rests between two puffs.")] float secondsBetweenPuffs = 4f;
        [SerializeField, Min(0f)] [Tooltip("Seconds the arm stays at the mouth.")] float smokeSeconds = 1.5f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds the arm takes to come up to the mouth.")] float raiseSeconds = 0.5f;
        [SerializeField, Min(0.01f)] [Tooltip("Seconds the arm takes to go back down.")] float lowerSeconds = 0.6f;

        [Header("Pose at the mouth")]
        [SerializeField] [Tooltip("How far the pivot moves from where it rests, in the detective's own units.")] Vector2 raisedOffset = new Vector2(0f, 0.15f);
        [SerializeField] [Tooltip("Degrees the pivot turns from its resting angle. Positive turns counter-clockwise.")] float raisedAngle = 25f;
        [SerializeField] [Tooltip("Extra degrees the arm tilts back and forth while smoking, like drawing on the cigarette.")] float puffTiltAngle = -8f;

        Sequence cycle;
        Vector3 restPosition;
        float restAngle;

        void Reset()
        {
            var child = transform.Find(ArmPivotName);
            if (child) armPivot = child;
        }

        void OnEnable()
        {
            if (!armPivot)
            {
                Debug.LogError("SmokingDetective needs an Arm Pivot. Run Window > Detective Dave > Set Up Smoking Detective.", this);
                enabled = false;
                return;
            }
            restPosition = armPivot.localPosition;
            restAngle = Mathf.DeltaAngle(0f, armPivot.localEulerAngles.z);
            cycle = BuildCycle();
        }

        void OnDisable()
        {
            if (cycle == null) return;
            cycle.Kill();
            cycle = null;
            armPivot.localPosition = restPosition;
            armPivot.localRotation = Quaternion.Euler(0f, 0f, restAngle);
        }

        Sequence BuildCycle()
        {
            var raised = restAngle + raisedAngle;
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(secondsBetweenPuffs);
            sequence.Append(armPivot.DOLocalMove(restPosition + (Vector3)raisedOffset, raiseSeconds).SetEase(Ease.OutQuad));
            sequence.Join(armPivot.DOLocalRotate(new Vector3(0f, 0f, raised), raiseSeconds).SetEase(Ease.OutQuad));
            sequence.Append(armPivot.DOLocalRotate(new Vector3(0f, 0f, raised + puffTiltAngle), smokeSeconds * 0.5f).SetEase(Ease.InOutSine));
            sequence.Append(armPivot.DOLocalRotate(new Vector3(0f, 0f, raised), smokeSeconds * 0.5f).SetEase(Ease.InOutSine));
            sequence.Append(armPivot.DOLocalMove(restPosition, lowerSeconds).SetEase(Ease.InOutQuad));
            sequence.Join(armPivot.DOLocalRotate(new Vector3(0f, 0f, restAngle), lowerSeconds).SetEase(Ease.InOutQuad));
            return sequence.SetLoops(-1).SetLink(gameObject);
        }
    }
}
