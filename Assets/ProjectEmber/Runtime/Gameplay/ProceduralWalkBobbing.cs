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
            leftArm ??= transform.Find("LeftArm");
            rightArm ??= transform.Find("RightArm");
            leftLeg ??= transform.Find("LeftLeg");
            rightLeg ??= transform.Find("RightLeg");
        }

        private void ApplyPart(Transform part, float direction)
        {
            if (part == null)
            {
                return;
            }

            var wave = Mathf.Sin(phase) * direction;
            var local = part.localPosition;
            local.y += wave * bobAmplitude * Time.deltaTime;
            part.localPosition = local;
            part.localRotation = Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
        }
    }
}
