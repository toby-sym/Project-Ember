using UnityEngine;

namespace ProjectEmber.Gameplay
{
    public sealed class ProceduralWalkBobbing : MonoBehaviour
    {
        [SerializeField] private PlayerTopDownController controller;
        [SerializeField] private float bobAmplitude = 0.06f;
        [SerializeField] private float rotationAmplitude = 12f;
        [SerializeField] private float frequency = 9f;

        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private Vector3 leftArmBasePosition;
        private Vector3 rightArmBasePosition;
        private Vector3 leftLegBasePosition;
        private Vector3 rightLegBasePosition;
        private Quaternion leftArmBaseRotation;
        private Quaternion rightArmBaseRotation;
        private Quaternion leftLegBaseRotation;
        private Quaternion rightLegBaseRotation;
        private float phase;

        private void Awake()
        {
            controller ??= GetComponent<PlayerTopDownController>();
            CacheParts();
        }

        private void Update()
        {
            CacheParts();
            var speed = controller != null ? controller.Velocity.magnitude : 0f;
            phase += Time.deltaTime * frequency * Mathf.Clamp01(speed);
            ApplyPart(leftArm, 1f);
            ApplyPart(rightArm, -1f);
            ApplyPart(leftLeg, -1f);
            ApplyPart(rightLeg, 1f);
        }

        private void CacheParts()
        {
            if (leftArm == null)
            {
                leftArm = transform.Find("LeftArm");
                if (leftArm != null)
                {
                    leftArmBasePosition = leftArm.localPosition;
                    leftArmBaseRotation = leftArm.localRotation;
                }
            }

            if (rightArm == null)
            {
                rightArm = transform.Find("RightArm");
                if (rightArm != null)
                {
                    rightArmBasePosition = rightArm.localPosition;
                    rightArmBaseRotation = rightArm.localRotation;
                }
            }

            if (leftLeg == null)
            {
                leftLeg = transform.Find("LeftLeg");
                if (leftLeg != null)
                {
                    leftLegBasePosition = leftLeg.localPosition;
                    leftLegBaseRotation = leftLeg.localRotation;
                }
            }

            if (rightLeg == null)
            {
                rightLeg = transform.Find("RightLeg");
                if (rightLeg != null)
                {
                    rightLegBasePosition = rightLeg.localPosition;
                    rightLegBaseRotation = rightLeg.localRotation;
                }
            }
        }

        private void ApplyPart(Transform part, float direction)
        {
            if (part == null)
            {
                return;
            }

            var wave = Mathf.Sin(phase) * direction;
            if (part == leftArm)
            {
                part.localPosition = leftArmBasePosition + new Vector3(0f, wave * bobAmplitude, 0f);
                part.localRotation = leftArmBaseRotation * Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
                return;
            }

            if (part == rightArm)
            {
                part.localPosition = rightArmBasePosition + new Vector3(0f, wave * bobAmplitude, 0f);
                part.localRotation = rightArmBaseRotation * Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
                return;
            }

            if (part == leftLeg)
            {
                part.localPosition = leftLegBasePosition + new Vector3(0f, wave * bobAmplitude, 0f);
                part.localRotation = leftLegBaseRotation * Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
                return;
            }

            if (part == rightLeg)
            {
                part.localPosition = rightLegBasePosition + new Vector3(0f, wave * bobAmplitude, 0f);
                part.localRotation = rightLegBaseRotation * Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
            }
        }
    }
}
