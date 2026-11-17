using UnityEngine;
using UnityEngine.UI;

namespace Assignment4
{
    /// <summary>
    /// Displays the Phase 8 comparison table as an in-world UI canvas in front of the player.
    /// </summary>
    public class Assignment4ComparisonUI : MonoBehaviour
    {
        public static void CreateOrUpdateWorldCanvas(Vector3 position, Quaternion rotation)
        {
            var existing = GameObject.Find("Comparison_UI_Board");
            if (existing != null)
            {
                DestroyImmediate(existing);
            }

            var canvasObj = new GameObject("Comparison_UI_Board");
            canvasObj.transform.position = position;
            canvasObj.transform.rotation = rotation;

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 650);
            rect.localScale = new Vector3(0.005f, 0.005f, 0.005f); // ~4.5m x 3.25m board in world space

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Background Board (3D Quad backing for solid depth)
            var backing = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backing.name = "Board_Backing";
            backing.transform.SetParent(canvasObj.transform, false);
            backing.transform.localPosition = new Vector3(0, 0, 0.01f);
            backing.transform.localRotation = Quaternion.Euler(0, 180, 0); // Face reverse
            backing.transform.localScale = new Vector3(920, 670, 1);
            var backingMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
            backingMat.color = new Color(0.1f, 0.12f, 0.16f, 1f);
            backing.GetComponent<Renderer>().material = backingMat;

            // Background Panel
            var panelObj = new GameObject("Background_Panel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);
            var panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Header Text
            CreateTextElement(panelObj.transform, "Title", 
                "ASSIGNMENT 4: LOCOMOTION COMPARISON MATRIX", 
                26, FontStyle.Bold, TextAnchor.MiddleCenter, 
                new Color(0.3f, 0.9f, 1f), 
                new Vector2(0, 280), new Vector2(860, 50));

            // Subtitle
            CreateTextElement(panelObj.transform, "Subtitle", 
                "Teleportation vs Continuous Movement in VR (XR Interaction Toolkit 3.x)", 
                16, FontStyle.Italic, TextAnchor.MiddleCenter, 
                new Color(0.8f, 0.85f, 0.9f), 
                new Vector2(0, 245), new Vector2(860, 30));

            // Table Content
            string tableText = 
                "<b>FACTOR</b>                     | <b>TELEPORTATION</b>                    | <b>CONTINUOUS MOVEMENT</b>\n" +
                "--------------------------------------------------------------------------------------------------------------------------------\n" +
                "<b>Ease of control</b>           | High (Point & click, low sickness)     | Moderate (Thumbstick/WASD steering)\n" +
                "<b>Movement speed</b>           | Instantaneous (Zero transit time)      | Gradual (Fixed velocity: 3.0 m/s)\n" +
                "<b>Navigation</b>                   | Discontinuous (Discrete hops)          | Smooth & continuous presence\n" +
                "<b>Precision</b>                    | High landing on targeted spots         | High micro-adjustments around corners\n" +
                "<b>Obstacle interaction</b>    | Bypasses gaps, blocked by walls        | Physical collision via CharacterController\n\n" +
                "--------------------------------------------------------------------------------------------------------------------------------\n" +
                "<b>SUITABILITY / USE CASES:</b>\n" +
                "• <b>Teleportation:</b> Best for comfort-first VR, large distances, disjoint platforms, motion sickness mitigation.\n" +
                "• <b>Continuous Movement:</b> Best for high immersion, indoor navigation, dynamic combat/strafing, tight mazes.\n\n" +
                "<b>CONTROLS:</b>\n" +
                "• <b>Continuous Move:</b> WASD / Left Thumbstick (steers relative to headset direction)\n" +
                "• <b>Teleportation:</b> Space (Right Controller) + Mouse Aim -> Left Click / Trigger on valid floor\n" +
                "• <b>Invalid Areas:</b> Hazard Zone & Obstacle Walls prevent teleportation (ray turns red)";

            CreateTextElement(panelObj.transform, "ComparisonTable", 
                tableText, 
                15, FontStyle.Normal, TextAnchor.UpperLeft, 
                Color.white, 
                new Vector2(0, -30), new Vector2(840, 480));
        }

        private static void CreateTextElement(Transform parent, string name, string content, int fontSize, FontStyle style, TextAnchor alignment, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            var text = textObj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = textObj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
        }
    }
}
