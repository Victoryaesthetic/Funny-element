using UnityEngine;

namespace FunnyElement
{
    public enum ElementKind { Fire, Water }
    public enum TouchAction { Left, Right, Jump }

    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpImpulse = 7f;
        private Rigidbody body;
        private int playerIndex;
        private ElementKind element;
        private bool jumpRequested;

        public ElementKind Element => element;

        public void Initialize(ElementKind playerElement, int index)
        {
            element = playerElement;
            playerIndex = index;
            body = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            float horizontal = playerIndex == 0
                ? (Input.GetKey(KeyCode.A) ? -1f : 0f) + (Input.GetKey(KeyCode.D) ? 1f : 0f)
                : (Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f) + (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f);
            bool jump = playerIndex == 0 ? Input.GetKeyDown(KeyCode.W) : Input.GetKeyDown(KeyCode.UpArrow);
            horizontal += TouchInput.Horizontal(playerIndex);
            jump |= TouchInput.ConsumeJump(playerIndex);
            if (jump) jumpRequested = true;
            var velocity = body.velocity;
            velocity.x = Mathf.Clamp(horizontal, -1f, 1f) * moveSpeed;
            body.velocity = velocity;
        }

        private void FixedUpdate()
        {
            if (!jumpRequested) return;
            jumpRequested = false;
            if (Physics.Raycast(transform.position, Vector3.down, 0.62f, ~0, QueryTriggerInteraction.Ignore))
                body.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }
    }

    public static class TouchInput
    {
        private static readonly bool[,] Held = new bool[2, 2];
        private static readonly bool[] Jump = new bool[2];
        public static void Set(int player, TouchAction action, bool value)
        {
            if (action == TouchAction.Jump) { if (value) Jump[player] = true; return; }
            Held[player, action == TouchAction.Left ? 0 : 1] = value;
        }
        public static float Horizontal(int player) => (Held[player, 1] ? 1f : 0f) - (Held[player, 0] ? 1f : 0f);
        public static bool ConsumeJump(int player) { bool value = Jump[player]; Jump[player] = false; return value; }
    }
}
