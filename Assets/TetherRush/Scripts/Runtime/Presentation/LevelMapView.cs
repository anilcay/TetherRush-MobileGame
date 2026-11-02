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
        private Sprite sphere, halo;
        private ScrollRect scroll;
        private RectTransform content, viewport;
        private LevelMapCircuit circuit;
        private Text progress, footer, legend;
        private Action<int> play;
        private bool[] completed;
        private int unlocked, current;
        private int focusIndex = -1;
        private Vector2 focusedViewportSize;
        public int NodeCount => nodes.Count;
        public float ScrollPosition => scroll != null ? scroll.verticalNormalizedPosition : 0f;

        public void Initialize(Font textFont, Sprite coreSprite, Sprite haloSprite, Action home, Action<int> start)
        {
            font = textFont; sphere = coreSprite; halo = haloSprite; play = start;
            // The header and footer are siblings of the masked viewport, so they never scroll away.
            var back = Box("Back", transform, 22, 24, 144, 48, VisualTheme.Track);
            back.raycastTarget = true;
            var backButton = back.gameObject.AddComponent<Button>(); backButton.targetGraphic = back;
            backButton.onClick.AddListener(() => home?.Invoke());
            TextAt(back.transform, "‹  Ana menü", 15, VisualTheme.Silver, 0, 0, 144, 48);
            TextAt(transform, "Leveller", 35, VisualTheme.UIText, 22, 83, 300, 48, TextAnchor.MiddleLeft, FontStyle.Bold);
            progress = TextAt(transform, "0 / 25", 17, VisualTheme.Tether, 350, 91, 166, 34, TextAnchor.MiddleRight, FontStyle.Bold);
            legend = TextAt(transform, "● Tamamlandı    ◉ Sıradaki    ○ Keşfet", 12, VisualTheme.UIMuted, 22, 136, 496, 24, TextAnchor.MiddleLeft);

            viewport = Stretch("Map viewport", transform, new Vector2(12, 72), new Vector2(-12, -174));
            var surface = viewport.gameObject.AddComponent<Image>(); surface.color = VisualTheme.Inlay;
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
            footerRect.offsetMin = new Vector2(22, 14); footerRect.offsetMax = new Vector2(-22, 62);
            footer = footerRect.gameObject.AddComponent<Text>(); Configure(footer, "Haritayı kaydır, enerji hattını keşfet.", 13, VisualTheme.UIMuted, TextAnchor.MiddleCenter);
        }

        public void SetLevels(int count, bool[] done, int[] bonuses, bool sequential)
        {
            for (int i = 0; i < nodes.Count; i++) { nodes[i].SetActive(false); Destroy(nodes[i]); }
            nodes.Clear(); completed = done;
            count = Mathf.Max(0, count);
            int highest = -1, total = 0;
            for (int i = 0; i < count; i++) if (IsDone(i)) { highest = i; total++; }
            current = Mathf.Clamp(highest + 1, 0, Mathf.Max(0, count - 1));
            unlocked = sequential ? current : count - 1;
            float height = EndPadding * 2 + Mathf.Max(0, count - 1) * Step;
            content.sizeDelta = new Vector2(0, height);
            progress.text = total + " / " + count + "  TAMAM";
            legend.text = sequential ? "● Tamamlandı    ◉ Sıradaki    ○ Kilitli" : "● Tamamlandı    ◉ Sıradaki    ○ Keşfet";
            footer.text = sequential ? "Bölümleri tamamla, enerji hattını aç." : "Tüm bölümler açık. Haritayı kaydır.";
            circuit.SetRoute(count, done, unlocked, current);
            for (int i = 0; i < count; i++) BuildNode(i, bonuses);
            FocusLevel(current);
        }

        public void FocusLevel(int index)
        {
            if (NodeCount == 0) return;
            index = Mathf.Clamp(index, 0, NodeCount - 1);
            focusIndex = index;
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

        private void BuildNode(int index, int[] bonuses)
        {
            bool done = IsDone(index), locked = index > unlocked, active = index == current;
            Color accent = index / 5 % 3 == 0 ? VisualTheme.CoreA : index / 5 % 3 == 1 ? VisualTheme.CoreB : VisualTheme.Bonus;
            var holder = MakeRect("Socket " + (index + 1), content);
            holder.anchorMin = holder.anchorMax = new Vector2(.5f + Mathf.Sin(index * .86f) * .285f, 0);
            holder.sizeDelta = new Vector2(136, 140);
            holder.anchoredPosition = new Vector2(0, NodeY(index));
            nodes.Add(holder.gameObject);
            if (active) Circle("Current energy glow", holder, 150, WithAlpha(VisualTheme.Tether, .45f), halo);
            Circle("Socket rim", holder, 108, locked ? VisualTheme.Track : done ? VisualTheme.Tether : accent, sphere);
            Circle("Socket inset", holder, 96, VisualTheme.Inlay, sphere);
            var face = Circle("Level " + (index + 1), holder, 84, locked ? VisualTheme.Track : done ? VisualTheme.TrackAccent : VisualTheme.UIBackground, sphere);
            face.raycastTarget = true;
            var button = face.gameObject.AddComponent<LevelMapNodeButton>(); button.targetGraphic = face;
            button.interactable = !locked;
            button.onClick.AddListener(() => { if (index <= unlocked) play?.Invoke(index); });
            var number = MakeRect("Number", face.transform);
            number.anchorMin = Vector2.zero; number.anchorMax = Vector2.one; number.offsetMin = number.offsetMax = Vector2.zero;
            Configure(number.gameObject.AddComponent<Text>(), (index + 1).ToString(), 30, locked ? VisualTheme.UIMuted : VisualTheme.UIText, TextAnchor.MiddleCenter, FontStyle.Bold);
            string status = locked ? "KİLİTLİ" : done ? "TAMAMLANDI" : active ? "SIRADAKİ" : "";
            TextAt(holder, status, 10, locked ? VisualTheme.UIMuted : done ? VisualTheme.Tether : accent, 0, 126, 136, 18);
            if (done && bonuses != null && index < bonuses.Length)
                TextAt(holder, "BONUS " + bonuses[index], 10, VisualTheme.Bonus, 0, 145, 136, 17);
        }

        private Image Circle(string name, RectTransform parent, float size, Color color, Sprite sprite)
        {
            var r = MakeRect(name, parent); r.anchorMin = r.anchorMax = new Vector2(.5f, .5f);
            r.sizeDelta = Vector2.one * size;
            var image = r.gameObject.AddComponent<Image>(); image.sprite = sprite; image.color = color; image.raycastTarget = false;
            image.preserveAspect = true; return image;
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
        private static Color WithAlpha(Color c, float a) { c.a = a; return c; }
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

    /// <summary>One clipped UI mesh for the whole static facility, energized conduit and completion ticks.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LevelMapCircuit : MaskableGraphic
    {
        private int count, unlocked, current;
        private bool[] completed;
        public void SetRoute(int nodeCount, bool[] done, int unlockedIndex, int currentIndex)
        {
            count = nodeCount; completed = done; unlocked = unlockedIndex; current = currentIndex; SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); float w = rectTransform.rect.width;
            Vector2 origin = rectTransform.rect.min;
            // Five-node facility bays vary their rail geometry and accent, without individual level names.
            for (int band = 0; band * 5 < count; band++)
            {
                float bottom = Mathf.Max(20, LevelMapView.NodeY(band * 5) - 96);
                float top = Mathf.Min(rectTransform.rect.height - 20, bottom + 754);
                Color accent = band % 3 == 0 ? VisualTheme.CoreA : band % 3 == 1 ? VisualTheme.CoreB : VisualTheme.Bonus;
                Color plate = VisualTheme.UIBackground; plate.a = .56f;
                Quad(vh, origin + new Vector2(20, bottom), new Vector2(w - 40, top - bottom), plate);
                for (int rail = 0; rail < 2; rail++)
                {
                    float x = rail == 0 ? 34 : w - 34;
                    Line(vh, origin + new Vector2(x, bottom + 24), origin + new Vector2(x, top - 24), 2, VisualTheme.Track);
                    int bars = 3 + band % 3;
                    for (int b = 0; b < bars; b++)
                    {
                        float y = bottom + 35 + b * 20;
                        Color dim = accent; dim.a = .38f;
                        Quad(vh, origin + new Vector2(x - 5, y), new Vector2(10, 8), dim);
                    }
                }
                Color rim = accent; rim.a = .25f;
                Line(vh, origin + new Vector2(55, top), origin + new Vector2(w - 55, top), 2, rim);
                // Relay bracket shape changes per bay, producing a deliberate five-section rhythm.
                float side = band % 2 == 0 ? 60 : w - 60;
                Line(vh, origin + new Vector2(side, bottom + 180), origin + new Vector2(side, bottom + 310 + band % 3 * 30), 4, VisualTheme.TrackAccent);
                Line(vh, origin + new Vector2(side, bottom + 180), origin + new Vector2(side + (band % 2 == 0 ? 32 : -32), bottom + 180), 4, VisualTheme.TrackAccent);
            }
            for (int i = 0; i < count - 1; i++)
            {
                Vector2 start = new Vector2(LevelMapView.NodeX(i, w), LevelMapView.NodeY(i));
                Vector2 end = new Vector2(LevelMapView.NodeX(i + 1, w), LevelMapView.NodeY(i + 1));
                Color energy = i < current ? VisualTheme.Tether : i + 1 <= unlocked ? VisualTheme.TrackAccent : VisualTheme.Track;
                Vector2 previous = start;
                for (int s = 1; s <= 12; s++)
                {
                    float t = s / 12f, smooth = t * t * (3 - 2 * t);
                    Vector2 next = new Vector2(Mathf.Lerp(start.x, end.x, smooth), Mathf.Lerp(start.y, end.y, t));
                    Color glow = energy; glow.a = .13f;
                    Line(vh, origin + previous, origin + next, 18, glow);
                    Line(vh, origin + previous, origin + next, 5, energy); previous = next;
                }
            }
            for (int i = 0; i < count; i++) if (completed != null && i < completed.Length && completed[i])
            {
                Vector2 badge = origin + new Vector2(LevelMapView.NodeX(i, w) + 47, LevelMapView.NodeY(i) + 47);
                Disc(vh, badge, 13, VisualTheme.Tether);
                Line(vh, badge + new Vector2(-6, 0), badge + new Vector2(-1, -5), 3, VisualTheme.Void);
                Line(vh, badge + new Vector2(-1, -5), badge + new Vector2(7, 5), 3, VisualTheme.Void);
            }
        }
        private static void Quad(VertexHelper vh, Vector2 p, Vector2 size, Color color)
        {
            int k = vh.currentVertCount;
            vh.AddVert(p, color, Vector2.zero); vh.AddVert(p + new Vector2(size.x, 0), color, Vector2.zero);
            vh.AddVert(p + size, color, Vector2.zero); vh.AddVert(p + new Vector2(0, size.y), color, Vector2.zero);
            vh.AddTriangle(k, k + 1, k + 2); vh.AddTriangle(k, k + 2, k + 3);
        }
        private static void Line(VertexHelper vh, Vector2 a, Vector2 b, float width, Color color)
        {
            Vector2 n = new Vector2(-(b - a).y, (b - a).x).normalized * width * .5f; int k = vh.currentVertCount;
            vh.AddVert(a - n, color, Vector2.zero); vh.AddVert(a + n, color, Vector2.zero);
            vh.AddVert(b + n, color, Vector2.zero); vh.AddVert(b - n, color, Vector2.zero);
            vh.AddTriangle(k, k + 1, k + 2); vh.AddTriangle(k, k + 2, k + 3);
        }
        private static void Disc(VertexHelper vh, Vector2 p, float r, Color color)
        {
            int k = vh.currentVertCount; vh.AddVert(p, color, Vector2.zero);
            const int segments = 20;
            for (int s = 0; s <= segments; s++)
            {
                float a = s * Mathf.PI * 2 / segments;
                vh.AddVert(p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r, color, Vector2.zero);
                if (s > 0) vh.AddTriangle(k, k + s, k + s + 1);
            }
        }
    }
}
