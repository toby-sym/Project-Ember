using UnityEngine;

namespace ProjectEmber.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerTopDownController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D body;
        private Vector2 input;

        public Vector2 Velocity => body != null ? body.linearVelocity : Vector2.zero;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void Update()
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);
        }

        private void FixedUpdate()
        {
            body.linearVelocity = input * moveSpeed;
        }
    }
}
