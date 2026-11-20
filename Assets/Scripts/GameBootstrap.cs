using UnityEngine;
using UnityEngine.UI;

namespace FunnyElement
{
    /// <summary>Builds a small original co-op platforming prototype when Play is pressed.</summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateGame()
        {
            if (FindFirstObjectByType<GameBootstrap>() != null) return;
            new GameObject("Funny Element - Prototype").AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            BuildWorld();
            BuildMobileControls();
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 2.3f, -12f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.backgroundColor = new Color(0.10f, 0.13f, 0.20f);
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            cameraObject.AddComponent<AudioListener>();
            RenderSettings.ambientLight = Color.white;
        }

        private void BuildWorld()
        {
            MakeBlock("Starting island", new Vector2(-5.3f, -2.1f), new Vector2(4.4f, 0.7f), new Color(0.34f, 0.39f, 0.48f));
            MakeBlock("Middle platform", new Vector2(0f, -0.5f), new Vector2(3.2f, 0.55f), new Color(0.34f, 0.39f, 0.48f));
            MakeBlock("Upper platform", new Vector2(4.6f, 1.1f), new Vector2(3.7f, 0.55f), new Color(0.34f, 0.39f, 0.48f));
            MakeBlock("Exit island", new Vector2(8.7f, -1.0f), new Vector2(3.5f, 0.7f), new Color(0.34f, 0.39f, 0.48f));
            MakeBlock("Step", new Vector2(-1.9f, -1.1f), new Vector2(1.0f, 0.35f), new Color(0.42f, 0.47f, 0.55f));
            MakeBlock("Step", new Vector2(2.2f, 0.1f), new Vector2(1.0f, 0.35f), new Color(0.42f, 0.47f, 0.55f));

            MakeHazard("Warm hazard", new Vector2(-4.2f, -1.55f), new Vector2(1.1f, 0.16f), ElementKind.Water, new Color(0.96f, 0.30f, 0.12f));
            MakeHazard("Cool hazard", new Vector2(8.1f, -0.45f), new Vector2(1.0f, 0.16f), ElementKind.Fire, new Color(0.15f, 0.62f, 0.95f));

            MakePlayer("Ember", ElementKind.Fire, new Vector2(-6.0f, -1.0f), Color.red, 0);
            MakePlayer("Ripple", ElementKind.Water, new Vector2(-4.8f, -1.0f), Color.blue, 1);
            MakeGoal(new Vector2(8.7f, -0.05f));
        }

        private static void MakeBlock(string name, Vector2 position, Vector2 size, Color color)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = new Vector3(size.x, size.y, 1f);
            block.GetComponent<Renderer>().material.color = color;
        }

        private static void MakeHazard(string name, Vector2 position, Vector2 size, ElementKind safeFor, Color color)
        {
            var hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazard.name = name;
            hazard.transform.position = position;
            hazard.transform.localScale = new Vector3(size.x, size.y, 1f);
            hazard.GetComponent<Renderer>().material.color = color;
            hazard.GetComponent<BoxCollider>().isTrigger = true;
            hazard.AddComponent<ElementHazard>().SafeFor = safeFor;
        }

        private static void MakePlayer(string name, ElementKind element, Vector2 position, Color color, int playerIndex)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = name;
            player.transform.position = position;
            player.transform.localScale = new Vector3(0.72f, 0.9f, 0.7f);
            player.GetComponent<Renderer>().material.color = color;
            var body = player.AddComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            player.AddComponent<PlayerController>().Initialize(element, playerIndex);
        }

        private static void MakeGoal(Vector2 position)
        {
            var goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goal.name = "Shared exit";
            goal.transform.position = position;
            goal.transform.localScale = new Vector3(0.65f, 1.8f, 0.35f);
            goal.GetComponent<Renderer>().material.color = new Color(0.72f, 0.45f, 1f);
            goal.GetComponent<BoxCollider>().isTrigger = true;
            goal.AddComponent<CoopGoal>();
        }

        private void BuildMobileControls()
        {
            var canvasObject = new GameObject("Touch controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

            AddButton(canvas.transform, "P1 ◀", 0, TouchAction.Left, new Vector2(0.06f, 0.12f));
            AddButton(canvas.transform, "P1 ▶", 0, TouchAction.Right, new Vector2(0.15f, 0.12f));
            AddButton(canvas.transform, "P1 ▲", 0, TouchAction.Jump, new Vector2(0.24f, 0.12f));
            AddButton(canvas.transform, "P2 ◀", 1, TouchAction.Left, new Vector2(0.76f, 0.12f));
            AddButton(canvas.transform, "P2 ▶", 1, TouchAction.Right, new Vector2(0.85f, 0.12f));
            AddButton(canvas.transform, "P2 ▲", 1, TouchAction.Jump, new Vector2(0.94f, 0.12f));
        }

        private static void AddButton(Transform parent, string label, int player, TouchAction action, Vector2 anchor)
        {
            var button = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TouchButton));
            button.transform.SetParent(parent, false);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(92f, 72f);
            button.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.42f);
            button.GetComponent<TouchButton>().Setup(player, action);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(button.transform, false);
            var text = textObject.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.fontSize = 22;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
        }
    }
}
