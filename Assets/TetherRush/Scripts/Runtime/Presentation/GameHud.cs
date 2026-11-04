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
        public string CourseTitle;
        public bool Endless, CanGoHome, CanAdvance, TeachControls;
        public int Score, BestScore;
        public float PaceMultiplier;
    }

    /// <summary>Presentation only; input, simulation and navigation remain callback owned.</summary>
    public sealed class GameHud : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        private RectTransform safeArea, progressFill;
        private Text subtitle, score, record, bonus, energyLabel, hint, cardTitle, cardNumber, cardBody;
        private Text primaryLabel, secondaryLabel, homeLabel;
        private Button pauseButton, audioButton, primaryButton, secondaryButton, homeButton;
        private UiIcon audioIcon;
        private GameObject outcome, energyRow, progressRail;
        private Image[] energySegments;
        private Action goHome, nextLevel;
        private bool initialized, nextAction;
        private Rect previousSafeArea;
        private int previousWidth, previousHeight;
        private float displayedProgress, coachRemaining;
        private string previousHint;
        private RunPhase previousPhase;

        public void Initialize(bool advanced, Action startOrResume, Action pause, Action toggleAudio, Action otherCourse)
        {
            if (initialized) return;
            initialized = true;
            var root = new GameObject("Tether Rush HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(transform, false);
            Canvas = root.GetComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 20;
            if (root.GetComponent<GraphicRaycaster>() == null) root.AddComponent<GraphicRaycaster>();
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960);
            scaler.matchWidthOrHeight = 0;
            safeArea = Rect("Safe area", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            pauseButton = IconButton("Pause", safeArea, UiGlyph.Pause, 12, 8, pause, out _);
            subtitle = Label("Course", safeArea, "BÖLÜM 01", 20, UiResources.Paper, 116, 16, 308, 30, true);
            subtitle.alignment = TextAnchor.MiddleCenter;
            progressRail = Panel("Progress rail", safeArea, UiResources.Line, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(116, -63), new Vector2(424, -58), false).gameObject;
            progressFill = Panel("Progress", progressRail.transform, UiResources.Paper, Vector2.zero,
                new Vector2(0, 1), Vector2.zero, Vector2.zero, false).rectTransform;
            var coin = Rect("Coin", safeArea, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(444, -53), new Vector2(471, -26)).gameObject.AddComponent<Image>();
            coin.sprite = UiResources.CoinSprite; coin.preserveAspect = true; coin.raycastTarget = false;
            bonus = Label("Bonus", safeArea, "0", 20, UiResources.Paper, 474, 24, 52, 30);
            score = Label("Score", safeArea, "", 36, UiResources.Paper, 116, 25, 308, 43, true);
            score.alignment = TextAnchor.MiddleCenter;
            record = Label("Best", safeArea, "", 16, UiResources.Muted, 116, 64, 308, 20);
            record.alignment = TextAnchor.MiddleCenter;

            energyRow = Rect("Pulse status", safeArea, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(116, -103), new Vector2(424, -82)).gameObject;
            energyLabel = Label("Energy caption", energyRow.transform, "PULSE", 16, UiResources.Muted, 0, 0, 110, 21);
            energySegments = new Image[5];
            for (int i = 0; i < energySegments.Length; i++)
                energySegments[i] = Panel("Energy " + (i + 1), energyRow.transform, UiResources.Line,
                    new Vector2(0, 1), new Vector2(0, 1), new Vector2(114 + i * 39, -14), new Vector2(147 + i * 39, -7), false);
            hint = Label("Instruction", safeArea, "", 19, UiResources.Paper, 36, 115, 468, 28);
            hint.alignment = TextAnchor.MiddleCenter;

            // The outcome is a calm flat scrim, without decorative instrument cards.
            outcome = Panel("Run card", safeArea, new Color(UiResources.Background.r, UiResources.Background.g, UiResources.Background.b, .97f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false).gameObject;
            outcome.GetComponent<Image>().raycastTarget = true;
            var contents = Rect("Outcome content", outcome.transform, new Vector2(0, .5f), new Vector2(1, .5f),
                new Vector2(36, -270), new Vector2(-36, 270));
            cardTitle = Label("State title", contents, "", 42, UiResources.Paper, 0, 20, 468, 60, true);
            cardTitle.alignment = TextAnchor.MiddleCenter;
            cardNumber = Label("State number", contents, "", 72, UiResources.Paper, 0, 100, 468, 88, true);
            cardNumber.alignment = TextAnchor.MiddleCenter;
            cardBody = Label("State explanation", contents, "", 17, UiResources.Muted, 20, 196, 428, 68);
            cardBody.alignment = TextAnchor.UpperCenter;
            primaryButton = Control("Start retry resume", contents, "Başla", startOrResume, out primaryLabel);
            secondaryButton = Control("Other course", contents, "Sonraki bölüm",
                () => { if (nextAction) nextLevel?.Invoke(); else otherCourse?.Invoke(); }, out secondaryLabel);
            homeButton = Control("Main menu", contents, "Haritaya dön", () => goHome?.Invoke(), out homeLabel);
            audioButton = IconButton("Audio", outcome.transform, UiGlyph.SoundOn, 12, 8, toggleAudio, out audioIcon);
            if (EventSystem.current == null)
            {
                var events = new GameObject("HUD EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            ApplySafeArea();
        }

        public void SetNavigation(Action home, Action next) { goHome = home; nextLevel = next; }

        public void Present(HudState state, float dt)
        {
            if (!initialized) return;
            ApplySafeArea();
            subtitle.text = state.Endless ? "SONSUZ AKIŞ" : !string.IsNullOrEmpty(state.CourseTitle)
                ? state.CourseTitle : state.AdvancedCourse ? "BÖLÜM 02" : "BÖLÜM 01";
            subtitle.rectTransform.offsetMin = new Vector2(116, state.Endless ? -30 : -46);
            subtitle.rectTransform.offsetMax = new Vector2(424, state.Endless ? 0 : -16);
            bonus.text = state.Bonus.ToString();
            score.gameObject.SetActive(state.Endless);
            record.gameObject.SetActive(state.Endless);
            score.text = state.Score.ToString("N0");
            record.text = "REKOR  " + state.BestScore.ToString("N0") + (state.Endless ? "  ·  " + Mathf.Max(1, state.PaceMultiplier).ToString("0.0") + "×" : "");
            progressRail.SetActive(!state.Endless);
            float progress = state.FinishDistance > 0 ? Mathf.Clamp01(state.Distance / state.FinishDistance) : 0;
            displayedProgress = state.Phase == RunPhase.Ready ? 0 :
                Mathf.Lerp(displayedProgress, progress, 1 - Mathf.Exp(-12 * Mathf.Max(0, dt)));
            progressFill.anchorMax = new Vector2(displayedProgress, 1);
            bool charged = state.EnergyCapacity > 0 && state.Energy >= state.EnergyCapacity;
            energyRow.SetActive(!state.AdvancedCourse);
            energyLabel.text = charged ? "PULSE HAZIR" : "PULSE";
            energyLabel.color = charged ? UiResources.Accent : UiResources.Muted;
            for (int i = 0; i < energySegments.Length; i++)
                energySegments[i].color = (i + 1f) / energySegments.Length <= (float)state.Energy / Mathf.Max(1, state.EnergyCapacity)
                    ? (charged ? Color.Lerp(UiResources.Accent, UiResources.Paper, .25f) : UiResources.Accent) : UiResources.Line;

            UpdateCoach(state, dt);
            bool running = state.Phase == RunPhase.Running;
            pauseButton.gameObject.SetActive(running);
            outcome.SetActive(!running);
            audioButton.gameObject.SetActive(!running);
            audioIcon.Glyph = state.AudioEnabled ? UiGlyph.SoundOn : UiGlyph.SoundOff;
            if (running) { previousPhase = state.Phase; return; }

            nextAction = state.CanAdvance && state.Phase == RunPhase.Completed;
            bool canSwitch = state.CanSwitchCourse && (state.Phase == RunPhase.Ready || state.Phase == RunPhase.Failed || state.Phase == RunPhase.Completed);
            secondaryButton.gameObject.SetActive(nextAction || canSwitch);
            secondaryLabel.text = nextAction ? "Sonraki bölüm" : state.AdvancedCourse ? "Temel parkur" : "Zamanlama";
            homeButton.gameObject.SetActive(state.CanGoHome);
            homeLabel.text = state.Endless ? "Ana menü" : "Haritaya dön";
            // Other course owns Next; Start retry resume keeps its original retry/resume callback.
            StyleButton(nextAction ? secondaryButton : primaryButton, true, 288);
            if (nextAction) StyleButton(primaryButton, false, 374);
            else if (canSwitch) StyleButton(secondaryButton, false, 374);
            StyleButton(homeButton, false, nextAction || canSwitch ? 456 : 374);
            cardNumber.gameObject.SetActive(state.Phase == RunPhase.Completed || state.Phase == RunPhase.Failed);
            switch (state.Phase)
            {
                case RunPhase.Paused:
                    cardTitle.text = "DURAKLATILDI";
                    cardBody.text = "Devam ettikten sonra yeniden basılı tut.";
                    primaryLabel.text = "Devam et";
                    break;
                case RunPhase.Failed:
                    cardTitle.text = state.Endless ? "AKIŞ BİTTİ" : "TEKRAR DENE";
                    cardNumber.text = state.Endless ? state.Score.ToString("N0") : state.Bonus.ToString();
                    cardBody.text = (state.Endless ? "SKOR · REKOR " + state.BestScore.ToString("N0") : "BONUS") +
                        (string.IsNullOrEmpty(state.FailureReason) ? "" : "\n" + state.FailureReason);
                    primaryLabel.text = "Tekrar dene";
                    break;
                case RunPhase.Completed:
                    cardTitle.text = "BÖLÜM TAMAM";
                    cardNumber.text = state.Bonus.ToString();
                    cardBody.text = "BONUS";
                    primaryLabel.text = "Tekrar oyna";
                    break;
                default:
                    cardTitle.text = subtitle.text;
                    cardBody.text = "Basılı tut: açıl\nBırak: daral";
                    primaryLabel.text = "Başla";
                    break;
            }
            previousPhase = state.Phase;
        }

        private void UpdateCoach(HudState state, float dt)
        {
            string current = state.Hint ?? string.Empty;
            if (state.Phase == RunPhase.Ready || (state.Phase == RunPhase.Running && previousPhase != RunPhase.Running && previousPhase != RunPhase.Paused))
                previousHint = null;
            if (state.Phase != RunPhase.Paused && current != previousHint)
            {
                previousHint = current;
                coachRemaining = 2.2f;
            }
            if (state.Phase == RunPhase.Running) coachRemaining = Mathf.Max(0, coachRemaining - Mathf.Max(0, dt));
            bool critical = current.IndexOf("MENZİLDE", StringComparison.OrdinalIgnoreCase) >= 0;
            bool contextual = current.StartsWith("BAĞ RÖLESİ", StringComparison.Ordinal) || current.StartsWith("PULSE KİLİDİ", StringComparison.Ordinal);
            bool show = state.Phase == RunPhase.Running && (critical || ((state.TeachControls || contextual) && coachRemaining > 0));
            hint.gameObject.SetActive(show);
            if (!show) return;
            if (critical) hint.text = "MENZİLDE · BIRAK";
            else
            {
                int split = current.IndexOf('•');
                string shortHint = (split >= 0 ? current.Substring(split + 1) : current).Trim();
                hint.text = shortHint.Length > 50 || shortHint.Length == 0 ? "Basılı tut: açıl · Bırak: daral" : shortHint;
            }
            hint.color = critical ? UiResources.Accent : UiResources.Paper;
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
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = low; rect.offsetMax = high;
            return rect;
        }

        private Image Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max, Vector2 low, Vector2 high, bool rounded = true)
        {
            var image = Rect(name, parent, min, max, low, high).gameObject.AddComponent<Image>();
            if (rounded) { image.sprite = UiResources.PanelSprite; image.type = Image.Type.Sliced; }
            image.color = color; image.raycastTarget = false;
            return image;
        }

        private Text Label(string name, Transform parent, string value, int size, Color color, float x, float y, float width, float height, bool display = false)
        {
            var text = Rect(name, parent, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(x, -y - height), new Vector2(x + width, -y)).gameObject.AddComponent<Text>();
            text.font = display ? UiResources.Display : UiResources.Body;
            text.fontStyle = FontStyle.Normal;
            text.text = value; text.fontSize = size; text.color = color;
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private UiIcon Icon(string name, Transform parent, UiGlyph glyph, Color color, float x, float y, float size)
        {
            var icon = Rect(name, parent, new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(x, -y - size), new Vector2(x + size, -y)).gameObject.AddComponent<UiIcon>();
            icon.color = color; icon.Glyph = glyph; icon.raycastTarget = false;
            return icon;
        }

        private Button IconButton(string name, Transform parent, UiGlyph glyph, float x, float y, Action callback, out UiIcon icon)
        {
            var image = Panel(name, parent, new Color(1, 1, 1, 0), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(x, -y - 72), new Vector2(x + 72, -y), false);
            var button = AddButton(image, callback);
            icon = Icon("Glyph", image.transform, glyph, UiResources.Paper, 24, 24, 24);
            return button;
        }

        private Button Control(string name, Transform parent, string caption, Action callback, out Text label)
        {
            var image = Panel(name, parent, UiResources.Surface, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var button = AddButton(image, callback);
            label = Label("Caption", image.transform, caption, 22, UiResources.Paper, 12, 0, 444, 72);
            label.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private Button AddButton(Image image, Action callback)
        {
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(.88f, .92f, .96f);
            colors.pressedColor = new Color(.64f, .72f, .8f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = .1f;
            button.colors = colors;
            button.onClick.AddListener(() => callback?.Invoke());
            return button;
        }

        private void StyleButton(Button button, bool primary, float top)
        {
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0, -top - 72); rect.offsetMax = new Vector2(0, -top);
            button.GetComponent<Image>().color = primary ? UiResources.Paper : new Color(1, 1, 1, 0);
            button.GetComponentInChildren<Text>().color = primary ? UiResources.Background : UiResources.Paper;
        }
    }
}
