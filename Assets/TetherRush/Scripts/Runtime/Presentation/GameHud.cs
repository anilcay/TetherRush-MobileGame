using System;
using TetherRush.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TetherRush.Presentation
{
    public struct HudState
    {
        public RunPhase Phase;
        public bool AdvancedCourse, AudioEnabled, CanSwitchCourse;
        public float Distance, FinishDistance;
        public int Energy, EnergyCapacity, Bonus, ActivatedTargets;
        public string Hint, FailureReason;
    }

    /// <summary>Presentation only. All gameplay changes are delegated to supplied callbacks.</summary>
    public sealed class GameHud : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        private RectTransform safeArea, progressFill, energyContainer, primaryRect;
        private Text subtitle, distance, bonus, energyLabel, hint, audioLabel, cardTitle, cardBody, primaryLabel;
        private Button pauseButton, secondaryButton;
        private GameObject card;
        private Image[] energySegments;
        private Image cardAccent;
        private Texture2D roundedTexture;
        private Sprite roundedSprite;
        private Font font;
        private Rect previousSafeArea;
        private int previousWidth, previousHeight;
        private bool initialized;
        private float displayedProgress;

        public void Initialize(bool advanced, Action startOrResume, Action pause, Action toggleAudio, Action otherCourse)
        {
            if (initialized) return;
            initialized = true;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateRoundedSprite();
            var root = new GameObject("Tether Rush HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            Canvas = root.GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 20;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960);
            scaler.matchWidthOrHeight = .5f;
            safeArea = Rect("Safe area", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var top = Panel("Flight instruments", safeArea, Alpha(VisualTheme.UIBackground, .94f),
                new Vector2(0, 1), Vector2.one, new Vector2(16, -152), new Vector2(-16, -12));
            Label("Title", top.transform, "TETHER RUSH", 20, VisualTheme.UIText, 18, 12, 252, 27, FontStyle.Bold);
            subtitle = Label("Course", top.transform, advanced ? "02  /  ZAMANLAMA" : "01  /  TEMEL PARKUR", 12, VisualTheme.UIMuted, 18, 37, 245, 17);
            Control("Audio", top.transform, "Ses açık", VisualTheme.Inlay, VisualTheme.Silver,
                new Vector2(1, 1), Vector2.one, new Vector2(-174, -58), new Vector2(-94, -10), toggleAudio, out audioLabel);
            audioLabel.fontSize = 14;
            pauseButton = Control("Pause", top.transform, "Duraklat", VisualTheme.Inlay, VisualTheme.Silver,
                new Vector2(1, 1), Vector2.one, new Vector2(-88, -58), new Vector2(-10, -10), pause, out var pauseLabel);
            pauseLabel.fontSize = 14;
            distance = Label("Distance", top.transform, "0 / 180 m", 17, VisualTheme.UIText, 18, 65, 220, 22, FontStyle.Bold);
            bonus = Label("Bonus", top.transform, "BONUS  0", 14, VisualTheme.Bonus, 268, 66, 214, 20);
            bonus.rectTransform.anchorMin = bonus.rectTransform.anchorMax = Vector2.one;
            bonus.rectTransform.offsetMin = new Vector2(-242, -86);
            bonus.rectTransform.offsetMax = new Vector2(-18, -66);
            bonus.alignment = TextAnchor.MiddleRight;
            var progress = Panel("Progress rail", top.transform, VisualTheme.Inlay, Vector2.zero, Vector2.one,
                new Vector2(18, 41), new Vector2(-18, -93));
            progressFill = Panel("Progress", progress.transform, VisualTheme.Tether, Vector2.zero, new Vector2(0, 1), Vector2.zero, Vector2.zero).rectTransform;
            energyLabel = Label("Energy caption", top.transform, "ENERJİ  0 / 5", 12, VisualTheme.UIMuted, 18, 108, 205, 20);
            energyContainer = Rect("Energy segments", top.transform, new Vector2(.46f, 0), new Vector2(1, 0), new Vector2(0, 16), new Vector2(-18, 26));
            var hintPanel = Panel("Course hint", safeArea, Alpha(VisualTheme.UIBackground, .9f),
                new Vector2(0, 1), Vector2.one, new Vector2(28, -197), new Vector2(-28, -162));
            hint = StretchLabel("Instruction", hintPanel.transform, "", 15, VisualTheme.UIText, 12);

            var footer = Panel("Touch reminder", safeArea, Alpha(VisualTheme.UIBackground, .83f),
                Vector2.zero, new Vector2(1, 0), new Vector2(46, 9), new Vector2(-46, 35));
            StretchLabel("Gesture", footer.transform, "BASILI TUT: AÇIL     /     BIRAK: DARAL", 12, VisualTheme.Silver, 8);

            var cardImage = Panel("Run card", safeArea, Alpha(VisualTheme.UIBackground, .97f),
                Vector2.zero, new Vector2(1, 0), new Vector2(24, 45), new Vector2(-24, 265));
            card = cardImage.gameObject;
            cardImage.raycastTarget = true;
            cardAccent = Panel("State accent", card.transform, VisualTheme.Tether,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -29), new Vector2(49, -25));
            Panel("Core peach", card.transform, VisualTheme.CoreA, new Vector2(1, 1), Vector2.one, new Vector2(-47, -29), new Vector2(-35, -17));
            Panel("Core lavender", card.transform, VisualTheme.CoreB, new Vector2(1, 1), Vector2.one, new Vector2(-29, -29), new Vector2(-17, -17));
            cardTitle = Label("State title", card.transform, "Bağa güven.", 24, VisualTheme.UIText, 20, 39, 440, 31, FontStyle.Bold);
            cardBody = Label("State explanation", card.transform, "", 16, VisualTheme.UIMuted, 20, 78, 440, 63);
            cardBody.alignment = TextAnchor.UpperLeft;
            cardBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            cardTitle.rectTransform.anchorMax = Vector2.one;
            cardTitle.rectTransform.offsetMax = new Vector2(-20, -39);
            cardBody.rectTransform.anchorMax = Vector2.one;
            cardBody.rectTransform.offsetMax = new Vector2(-20, -78);
            var primary = Control("Start retry resume", card.transform, "Başla", VisualTheme.Tether, VisualTheme.Void,
                Vector2.zero, new Vector2(.5f, 0), new Vector2(20, 20), new Vector2(-5, 68), startOrResume, out primaryLabel);
            primaryRect = (RectTransform)primary.transform;
            secondaryButton = Control("Other course", card.transform, advanced ? "Temel parkur" : "Zamanlama",
                VisualTheme.Track, VisualTheme.UIText, new Vector2(.5f, 0), new Vector2(1, 0), new Vector2(5, 20), new Vector2(-20, 68), otherCourse, out var secondaryLabel);
            secondaryLabel.fontSize = 16;

            if (EventSystem.current == null)
            {
                var events = new GameObject("HUD EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            ApplySafeArea();
        }

        public void Present(HudState state, float dt)
        {
            if (!initialized) return;
            ApplySafeArea();
            subtitle.text = state.AdvancedCourse ? "02  /  ZAMANLAMA" : "01  /  TEMEL PARKUR";
            distance.text = Mathf.FloorToInt(state.Distance) + " / " + Mathf.RoundToInt(state.FinishDistance) + " m";
            bonus.text = "BONUS  " + state.Bonus + "   /   HEDEF  " + state.ActivatedTargets;
            audioLabel.text = state.AudioEnabled ? "Ses açık" : "Sessiz";
            pauseButton.gameObject.SetActive(state.Phase == RunPhase.Running);
            float progress = state.FinishDistance > 0 ? Mathf.Clamp01(state.Distance / state.FinishDistance) : 0;
            displayedProgress = state.Phase == RunPhase.Ready ? 0 : Mathf.Lerp(displayedProgress, progress, 1 - Mathf.Exp(-12 * Mathf.Max(0, dt)));
            progressFill.anchorMax = new Vector2(displayedProgress, 1);
            bool charged = state.EnergyCapacity > 0 && state.Energy >= state.EnergyCapacity;
            energyLabel.text = state.AdvancedCourse ? "RİTMİ YAKALA" : charged ? "PULSE HAZIR" : "ENERJİ  " + state.Energy + " / " + state.EnergyCapacity;
            energyLabel.color = charged ? VisualTheme.TetherReady : VisualTheme.UIMuted;
            UpdateEnergy(state);
            hint.text = state.Hint ?? string.Empty;
            hint.color = charged ? VisualTheme.TetherReady : VisualTheme.UIText;
            bool showCard = state.Phase != RunPhase.Running;
            card.SetActive(showCard);
            if (!showCard) return;
            bool canSwitch = state.CanSwitchCourse && (state.Phase == RunPhase.Ready || state.Phase == RunPhase.Failed || state.Phase == RunPhase.Completed);
            secondaryButton.gameObject.SetActive(canSwitch);
            // One primary action expands to the full card width when switching is unavailable.
            primaryRect.anchorMax = new Vector2(canSwitch ? .5f : 1, 0);
            primaryRect.offsetMax = new Vector2(canSwitch ? -5 : -20, 68);
            switch (state.Phase)
            {
                case RunPhase.Paused:
                    cardTitle.text = "Bir nefes al.";
                    cardBody.text = "Hazır olduğunda devam et.\nAçılmak için yeniden basılı tut.";
                    primaryLabel.text = "Devam et";
                    cardAccent.color = VisualTheme.CoreB;
                    break;
                case RunPhase.Failed:
                    cardTitle.text = "Bir kez daha.";
                    cardBody.text = string.IsNullOrEmpty(state.FailureReason) ? "İki küreyi de koru.\nYeni denemede ritmi yakala." : state.FailureReason;
                    primaryLabel.text = "Yeniden dene";
                    cardAccent.color = VisualTheme.Danger;
                    break;
                case RunPhase.Completed:
                    cardTitle.text = "Bağ tamamlandı.";
                    cardBody.text = state.Bonus + " bonus topladın.  " + state.ActivatedTargets + " hedef açıldı.\nBir tur daha?";
                    primaryLabel.text = "Tekrar oyna";
                    cardAccent.color = VisualTheme.Bonus;
                    break;
                default:
                    cardTitle.text = "Bağa güven.";
                    cardBody.text = state.AdvancedCourse ? "Basılı tut, açıl. Bırak, daral.\nHareketli engellerin ritmini yakala." : "Basılı tut, açıl. Bırak, daral.\nEnerji tamken menzilde bırak: Pulse.";
                    primaryLabel.text = "Başla";
                    cardAccent.color = VisualTheme.Tether;
                    break;
            }
        }

        private void UpdateEnergy(HudState state)
        {
            int count = state.AdvancedCourse ? 0 : Mathf.Clamp(state.EnergyCapacity, 0, 16);
            if (energySegments == null || energySegments.Length != count)
            {
                if (energySegments != null) foreach (var segment in energySegments) { segment.gameObject.SetActive(false); Destroy(segment.gameObject); }
                energySegments = new Image[count];
                for (int i = 0; i < count; i++)
                    energySegments[i] = Panel("Energy " + (i + 1), energyContainer, VisualTheme.Track,
                        new Vector2((float)i / count, 0), new Vector2((float)(i + 1) / count, 1), new Vector2(2, 0), new Vector2(-2, 0));
            }
            for (int i = 0; i < count; i++)
                energySegments[i].color = (float)(i + 1) / count <= (float)state.Energy / Mathf.Max(1, state.EnergyCapacity)
                    ? VisualTheme.Energy : VisualTheme.Track;
        }

        private void ApplySafeArea()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var area = Screen.safeArea;
            if (area == previousSafeArea && previousWidth == Screen.width && previousHeight == Screen.height) return;
            previousSafeArea = area; previousWidth = Screen.width; previousHeight = Screen.height;
            safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
        }

        private RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = low; rect.offsetMax = high;
            return rect;
        }

        private Image Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            var rect = Rect(name, parent, min, max, low, high);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = roundedSprite; image.type = Image.Type.Sliced; image.color = color; image.raycastTarget = false;
            return image;
        }

        private Text Label(string name, Transform parent, string value, int size, Color color, float x, float y, float width, float height, FontStyle style = FontStyle.Normal)
        {
            var text = Rect(name, parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, -y - height), new Vector2(x + width, -y)).gameObject.AddComponent<Text>();
            SetText(text, value, size, color, style); return text;
        }

        private Text StretchLabel(string name, Transform parent, string value, int size, Color color, float padding)
        {
            var text = Rect(name, parent, Vector2.zero, Vector2.one, new Vector2(padding, 0), new Vector2(-padding, 0)).gameObject.AddComponent<Text>();
            SetText(text, value, size, color, FontStyle.Normal); text.alignment = TextAnchor.MiddleCenter; return text;
        }

        private void SetText(Text text, string value, int size, Color color, FontStyle style)
        {
            text.font = font; text.text = value; text.fontSize = size; text.fontStyle = style; text.color = color;
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private Button Control(string name, Transform parent, string caption, Color background, Color foreground,
            Vector2 min, Vector2 max, Vector2 low, Vector2 high, Action callback, out Text label)
        {
            var image = Panel(name, parent, background, min, max, low, high); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(.9f, .96f, 1);
            colors.pressedColor = new Color(.65f, .78f, .88f);
            colors.selectedColor = Color.white; colors.fadeDuration = .12f; button.colors = colors;
            button.onClick.AddListener(() => callback?.Invoke());
            label = StretchLabel("Caption", image.transform, caption, 18, foreground, 8); label.fontStyle = FontStyle.Bold;
            return button;
        }

        private void CreateRoundedSprite()
        {
            const int size = 64;
            const float radius = 18;
            roundedTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "HUD rounded corners", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x + .5f - size / 2f) - (size / 2f - radius), 0);
                float dy = Mathf.Max(Mathf.Abs(y + .5f - size / 2f) - (size / 2f - radius), 0);
                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(radius + .5f - Mathf.Sqrt(dx * dx + dy * dy)));
            }
            roundedTexture.SetPixels(pixels); roundedTexture.Apply(false, true);
            roundedSprite = Sprite.Create(roundedTexture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(20, 20, 20, 20));
            roundedSprite.name = "HUD rounded nine-slice";
        }

        private static Color Alpha(Color color, float alpha) { color.a = alpha; return color; }
        private void OnDestroy()
        {
            if (roundedSprite != null) Destroy(roundedSprite);
            if (roundedTexture != null) Destroy(roundedTexture);
        }
    }
}
