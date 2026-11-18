using UnityEngine;
using UnityEngine.EventSystems;

namespace FunnyElement
{
    public sealed class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private int player;
        private TouchAction action;
        public void Setup(int playerIndex, TouchAction buttonAction) { player = playerIndex; action = buttonAction; }
        public void OnPointerDown(PointerEventData eventData) => TouchInput.Set(player, action, true);
        public void OnPointerUp(PointerEventData eventData) => TouchInput.Set(player, action, false);
        public void OnPointerExit(PointerEventData eventData) => TouchInput.Set(player, action, false);
    }

    public sealed class ElementHazard : MonoBehaviour
    {
        public ElementKind SafeFor { get; set; }
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null && player.Element != SafeFor)
                player.transform.position = new Vector3(-5.5f + (player.name == "Ripple" ? 1f : 0f), -1f, 0f);
        }
    }

    public sealed class CoopGoal : MonoBehaviour
    {
        private readonly bool[] arrived = new bool[2];
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null) return;
            arrived[player.name == "Ember" ? 0 : 1] = true;
            if (arrived[0] && arrived[1]) Debug.Log("Уровень пройден! Оба героя добрались до выхода.");
        }
        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null) arrived[player.name == "Ember" ? 0 : 1] = false;
        }
    }
}
