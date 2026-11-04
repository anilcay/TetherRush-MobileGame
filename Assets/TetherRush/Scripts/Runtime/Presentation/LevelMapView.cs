using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TetherRush.Presentation
{
    /// <summary>Numbered energy-facility route. Coordinates are bottom-up in a 540-wide portrait canvas.</summary>
    public sealed class LevelMapView : MonoBehaviour
    {
        private const float Step = 156f, EndPadding = 130f;
        private readonly List<GameObject> nodes = new List<GameObject>();
        private Font font;
        private ScrollRect scroll;
        private RectTransform content, viewport;
        private LevelMapCircuit circuit;
        private Text progress, footer, footerBonus;
        private Image footerCrystal;
        private int[] bestBonuses;
        internal static readonly Color Background = new Color32(13, 23, 34, 255);
        internal static readonly Color Surface = new Color32(26, 42, 59, 255);
        internal static readonly Color Paper = new Color32(245, 242, 233, 255);
        internal static readonly Color Muted = new Color32(131, 147, 163, 255);
        internal static readonly Color RouteLine = new Color32(49, 67, 86, 255);
        private Action<int> play;
        private bool[] completed;
        private int unlocked, current;
        private int focusIndex = -1;
        private Vector2 focusedViewportSize;
        public int NodeCount => nodes.Count;
        public float ScrollPosition => scroll != null ? scroll.verticalNormalizedPosition : 0f;

        public void Initialize(Font textFont, Sprite coreSprite, Sprite haloSprite, Action home, Action<int> start)
        {
            // Legacy sprite arguments remain API-compatible; map sockets use the shared flat disc.
            font = UiResources.Body != null ? UiResources.Body : textFont; play = start;
            // The header and footer are siblings of the masked viewport, so they never scroll away.
            var back = Box("Back", transform, 16, 18, 72, 72, Color.clear);
            back.raycastTarget = true;
            var backButton = back.gameObject.AddComponent<Button>(); backButton.targetGraphic = back;
            backButton.onClick.AddListener(() => home?.Invoke());
            Icon("Back icon", back.transform, UiGlyph.Back, Paper, new Vector2(0, 0), 28);
            var title = TextAt(transform, "Leveller", 38, Paper, 104, 28, 280, 52, TextAnchor.MiddleLeft);
            title.font = UiResources.Display;
            progress = TextAt(transform, "0/25", 22, Muted, 380, 36, 132, 40, TextAnchor.MiddleRight);

            viewport = Stretch("Map viewport", transform, new Vector2(12, 76), new Vector2(-12, -108));
            var surface = viewport.gameObject.AddComponent<Image>(); surface.color = Background;
            surface.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport; scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true; scroll.decelerationRate = .10f; scroll.scrollSensitivity = 38;
            content = MakeRect("Energy route", viewport);
            content.anchorMin = new Vector2(0, 0); content.anchorMax = new Vector2(1, 0);
            content.pivot = new Vector2(.5f, 0); content.sizeDelta = new Vector2(0, 1);
            scroll.content = content;
            var circuitRect = Stretch("Facility circuits", content, Vector2.zero, Vector2.zero);
            circuitRect.gameObject.AddComponent<CanvasRenderer>();
            circuit = circuitRect.gameObject.AddComponent<LevelMapCircuit>(); circuit.raycastTarget = false;

            var footerRect = MakeRect("Map footer", transform);
            footerRect.anchorMin = Vector2.zero; footerRect.anchorMax = new Vector2(1, 0);
            footerRect.offsetMin = new Vector2(24, 14); footerRect.offsetMax = new Vector2(-24, 62);
            footer = footerRect.gameObject.AddComponent<Text>(); Configure(footer, "Bölüm 1", 22, Paper, TextAnchor.MiddleLeft);
            var coinRect = MakeRect("Focused bonus coin", footerRect);
            coinRect.anchorMin = coinRect.anchorMax = new Vector2(.5f, .5f);
            coinRect.sizeDelta = Vector2.one * 24; coinRect.anchoredPosition = new Vector2(176, 0);
            footerCrystal = coinRect.gameObject.AddComponent<Image>(); footerCrystal.sprite = UiResources.CoinSprite;
            footerCrystal.preserveAspect = true; footerCrystal.raycastTarget = false;
            footerBonus = TextAt(footerRect, "0", 22, Paper, 440, 0, 52, 48, TextAnchor.MiddleRight);
            footerCrystal.gameObject.SetActive(false); footerBonus.gameObject.SetActive(false);
        }

        public void SetLevels(int count, bool[] done, int[] bonuses, bool sequential)
        {
            for (int i = 0; i < nodes.Count; i++) { nodes[i].SetActive(false); Destroy(nodes[i]); }
            nodes.Clear(); completed = done; bestBonuses = bonuses;
            count = Mathf.Max(0, count);
            int highest = -1, total = 0;
            for (int i = 0; i < count; i++) if (IsDone(i)) { highest = i; total++; }
            current = Mathf.Clamp(highest + 1, 0, Mathf.Max(0, count - 1));
            unlocked = sequential ? current : count - 1;
            float height = EndPadding * 2 + Mathf.Max(0, count - 1) * Step;
            content.sizeDelta = new Vector2(0, height);
            progress.text = total + "/" + count;
            circuit.SetRoute(count, done, unlocked, current);
            for (int i = 0; i < count; i++) BuildNode(i);
            FocusLevel(current);
        }

        public void FocusLevel(int index)
        {
            if (NodeCount == 0) return;
            index = Mathf.Clamp(index, 0, NodeCount - 1);
            focusIndex = index;
            footer.text = "Bölüm " + (index + 1);
            bool showBonus = IsDone(index) && bestBonuses != null && index < bestBonuses.Length;
            footerCrystal.gameObject.SetActive(showBonus); footerBonus.gameObject.SetActive(showBonus);
            if (showBonus) footerBonus.text = bestBonuses[index].ToString();
            Canvas.ForceUpdateCanvases();
            focusedViewportSize = viewport.rect.size;
            scroll.StopMovement();
            float travel = Mathf.Max(0, content.rect.height - viewport.rect.height);
            scroll.verticalNormalizedPosition = index == 0 ? 0f : index == NodeCount - 1 ? 1f :
                travel > .01f ? Mathf.Clamp01((NodeY(index) - viewport.rect.height * .5f) / travel) : 0f;
        }

        private void LateUpdate()
        {
            // Reapply the chosen node once on a real viewport resize, never while merely scrolling.
            if (viewport != null && focusIndex >= 0 && (viewport.rect.size - focusedViewportSize).sqrMagnitude > .01f)
                FocusLevel(focusIndex);
        }

        internal static float NodeY(int index) => EndPadding + index * Step;
        internal static float NodeX(int index, float width) => width * .5f + Mathf.Sin(index * .86f) * width * .285f;
        private bool IsDone(int i) => completed != null && i < completed.Length && completed[i];

        private void BuildNode(int index)
        {
            bool done = IsDone(index), locked = index > unlocked, active = index == current;
            var holder = MakeRect("Socket " + (index + 1), content);
            holder.anchorMin = holder.anchorMax = new Vector2(.5f + Mathf.Sin(index * .86f) * .285f, 0);
            holder.sizeDelta = new Vector2(136, 140);
            holder.anchoredPosition = new Vector2(0, NodeY(index));
            nodes.Add(holder.gameObject);
            if (active && !done && !locked)
            {
                Circle("Current rim", holder, 106, VisualTheme.Tether);
                Circle("Current rim spacing", holder, 102, Background);
            }
            var face = Circle("Level " + (index + 1), holder, 94,
                locked ? Surface : done ? Paper : active ? VisualTheme.Tether : Surface);
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<LevelMapNodeButton>(); button.targetGraphic = face;
            button.interactable = !locked;
            button.onClick.AddListener(() => { if (index <= unlocked) play?.Invoke(index); });
            var number = MakeRect("Number", face.transform);
            number.anchorMin = Vector2.zero; number.anchorMax = Vector2.one; number.offsetMin = number.offsetMax = Vector2.zero;
            number.anchoredPosition = new Vector2(0, locked ? 8 : 0);
            var label = number.gameObject.AddComponent<Text>();
            Configure(label, (index + 1).ToString(), 40, locked ? Muted : done || active ? Background : Paper, TextAnchor.MiddleCenter);
            label.font = UiResources.Display;
            if (done)
            {
                var badge = Circle("Completed check backing", holder, 24, Background);
                badge.rectTransform.anchoredPosition = new Vector2(37, -34);
                Icon("Completed check", holder, UiGlyph.Check, Paper, new Vector2(37, -34), 18);
            }
            else if (locked) Icon("Locked", holder, UiGlyph.Lock, Muted, new Vector2(0, -28), 18);
        }

        private Image Circle(string name, RectTransform parent, float size, Color color)
        {
            var r = MakeRect(name, parent); r.anchorMin = r.anchorMax = new Vector2(.5f, .5f);
            r.sizeDelta = Vector2.one * size;
            var image = r.gameObject.AddComponent<Image>(); image.sprite = UiResources.CircleSprite; image.color = color; image.raycastTarget = false;
            image.preserveAspect = true; return image;
        }
        private UiIcon Icon(string name, Transform parent, UiGlyph glyph, Color color, Vector2 position, float size)
        {
            var r = MakeRect(name, parent); r.anchorMin = r.anchorMax = new Vector2(.5f, .5f);
            r.sizeDelta = Vector2.one * size; r.anchoredPosition = position;
            r.gameObject.AddComponent<CanvasRenderer>();
            var icon = r.gameObject.AddComponent<UiIcon>(); icon.Glyph = glyph; icon.color = color; icon.raycastTarget = false;
            return icon;
        }
        private Image Box(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            var r = TopRect(name, parent, x, y, w, h); var image = r.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return image;
        }
        private Text TextAt(Transform parent, string value, int size, Color color, float x, float y, float w, float h, TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var text = TopRect(value, parent, x, y, w, h).gameObject.AddComponent<Text>(); Configure(text, value, size, color, align, style); return text;
        }
        private void Configure(Text text, string value, int size, Color color, TextAnchor align, FontStyle style = FontStyle.Normal)
        {
            text.font = font; text.text = value; text.fontSize = size; text.color = color; text.alignment = align; text.fontStyle = style;
            text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Overflow; text.verticalOverflow = VerticalWrapMode.Truncate;
        }
        private static RectTransform MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return (RectTransform)go.transform;
        }
        private static RectTransform TopRect(string name, Transform parent, float x, float y, float w, float h)
        {
            var r = MakeRect(name, parent); r.anchorMin = r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1); r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
        }
        private static RectTransform Stretch(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var r = MakeRect(name, parent); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = min; r.offsetMax = max; return r;
        }
    }

    /// <summary>ScrollRect also cancels clicks on drag; this guard rejects a displaced touch explicitly.</summary>
    public sealed class LevelMapNodeButton : Button
    {
        public override void OnPointerClick(PointerEventData eventData)
        {
            float threshold = EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 10f;
            if (eventData.dragging || (eventData.position - eventData.pressPosition).sqrMagnitude > threshold * threshold) return;
            base.OnPointerClick(eventData);
        }
    }

    /// <summary>A single clipped, flat conduit connects every numbered facility socket.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LevelMapCircuit : MaskableGraphic
    {
        private int count, current;
        public void SetRoute(int nodeCount, bool[] done, int unlockedIndex, int currentIndex)
        {
            count = nodeCount; current = currentIndex; SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); float w = rectTransform.rect.width;
            Vector2 origin = rectTransform.rect.min;
            for (int i = 0; i < count - 1; i++)
            {
                Vector2 start = new Vector2(LevelMapView.NodeX(i, w), LevelMapView.NodeY(i));
                Vector2 end = new Vector2(LevelMapView.NodeX(i + 1, w), LevelMapView.NodeY(i + 1));
                Color energy = i < current ? VisualTheme.Tether : LevelMapView.RouteLine;
                Vector2 previous = start;
                for (int s = 1; s <= 12; s++)
                {
                    float t = s / 12f, smooth = t * t * (3 - 2 * t);
                    Vector2 next = new Vector2(Mathf.Lerp(start.x, end.x, smooth), Mathf.Lerp(start.y, end.y, t));
                    Line(vh, origin + previous, origin + next, 3, energy); previous = next;
                }
            }
        }
        private static void Line(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 n = new Vector2(-(b - a).y, (b - a).x).normalized * width * .5f; int k = vh.currentVertCount;
            vh.AddVert(a - n, color, Vector2.zero); vh.AddVert(a + n, color, Vector2.zero);
            vh.AddVert(b + n, color, Vector2.zero); vh.AddVert(b - n, color, Vector2.zero);
            vh.AddTriangle(k, k + 1, k + 2); vh.AddTriangle(k, k + 2, k + 3);
        }
    }
}
