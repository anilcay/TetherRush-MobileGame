using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TetherRush.Presentation
{
    /// <summary>Portrait front end. Owns presentation; the app owns navigation data and persistence.</summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        public Action RequestedLevels, RequestedHome, RequestedEndless, RequestedSettings;
        private Action<int> playLevel;
        private Action playEndless;
        private Action<bool> setAudio;
        private Action<float> setVolume;
        private RectTransform safeArea;
        private GameObject home, levels, endless, settings;
        private Text homeRecord, homeProgress, endlessRecord, volumeCaption, audioCaption;
        private LevelMapView levelMap;
        private MenuHero menuHero;
        private Toggle audioToggle;
        private Slider volumeSlider;
        private RectTransform toggleKnob;
        private Image toggleTrack;
        private UiIcon audioStateIcon;
        private int completedLevels, nextLevel;
        private Rect previousSafeArea;
        private int previousWidth, previousHeight;

        public bool SequentialLevels { get; set; }
        public float MapScrollPosition => levelMap != null ? levelMap.ScrollPosition : 0f;
        public int MapNodeCount => levelMap != null ? levelMap.NodeCount : 0;
        public void FocusLevel(int index) { if (levelMap != null) levelMap.FocusLevel(index); }

        public void Initialize(Action<int> playLevel, Action playEndless, Action<bool> setAudio, Action<float> setVolume)
        {
            if (Canvas != null) return;
            this.playLevel = playLevel; this.playEndless = playEndless;
            this.setAudio = setAudio; this.setVolume = setVolume;
            var root = new GameObject("Tether Rush Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            Canvas = root.GetComponent<Canvas>(); Canvas.renderMode = RenderMode.ScreenSpaceOverlay; Canvas.sortingOrder = 30;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960); scaler.matchWidthOrHeight = 0f;
            var background = root.AddComponent<Image>(); background.color = UiResources.Background;
            safeArea = Rect("Safe area", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            home = Page("Home"); levels = Page("Levels"); endless = Page("Endless"); settings = Page("Settings");
            BuildHome(); BuildLevels(); BuildEndless(); BuildSettings();
            if (EventSystem.current == null)
            {
                var events = new GameObject("Menu EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            ApplySafeArea(); Hide();
        }

        /// <summary>Home progress snapshot. nextLevel is zero-based, including the last replayable level after completion.</summary>
        public void SetProgress(int completed, int nextLevel)
        {
            completedLevels = Mathf.Clamp(completed, 0, 25);
            this.nextLevel = Mathf.Clamp(nextLevel, 0, 24);
            UpdateProgressCaption();
        }

        public void ShowHome(int bestScore, bool audio, float volume)
        {
            if (Canvas == null) return;
            homeRecord.text = "En iyi  " + bestScore.ToString("N0");
            UpdateProgressCaption(); SetAudioValues(audio, volume); Show(home);
        }

        public void ShowLevels(string[] titles, string[] descriptions, float[] lengths, bool[] completed, int[] bestBonus)
        {
            if (Canvas == null) return;
            int count = Mathf.Max(titles != null ? titles.Length : 0, completed != null ? completed.Length : 0);
            if (count == 0) count = Mathf.Max(descriptions != null ? descriptions.Length : 0, lengths != null ? lengths.Length : 0);
            Show(levels);
            levelMap.SetLevels(count, completed, bestBonus, SequentialLevels);
        }

        public void ShowEndless(int bestScore)
        {
            if (Canvas == null) return;
            endlessRecord.text = bestScore.ToString("N0"); Show(endless);
        }

        public void ShowSettings(bool audio, float volume)
        {
            if (Canvas == null) return;
            SetAudioValues(audio, volume); Show(settings);
        }

        public void Hide()
        {
            if (menuHero != null) menuHero.SetVisible(false);
            if (Canvas != null) Canvas.gameObject.SetActive(false);
        }

        private void BuildHome()
        {
            Label(home.transform, "TETHER RUSH", 70, UiResources.Paper, 40, 68, 460, 110, true);
            var gear = Button("Settings", home.transform, 428, 22, 72, 72, Color.clear, () => RequestedSettings?.Invoke());
            Icon(gear.transform, UiGlyph.Gear, UiResources.Paper, 23, 23, 26, 26);

            var heroRect = At("Blender menu hero", home.transform, 30, 205, 480, 300);
            var heroImage = heroRect.gameObject.AddComponent<RawImage>();
            heroImage.color = Color.white; heroImage.raycastTarget = false;
            menuHero = gameObject.AddComponent<MenuHero>(); menuHero.Initialize(heroImage);

            homeProgress = Label(home.transform, "", 20, UiResources.Muted, 40, 557, 460, 30);
            UpdateProgressCaption();
            var levelsButton = Button("Levels", home.transform, 40, 615, 460, 84, UiResources.Paper, () => RequestedLevels?.Invoke());
            Label(levelsButton.transform, "LEVELLER", 32, UiResources.Background, 24, 0, 345, 84, true);
            Icon(levelsButton.transform, UiGlyph.Next, UiResources.Background, 404, 29, 26, 26);

            Rule(home.transform, 40, 730, 460);
            var endlessButton = Button("Endless", home.transform, 40, 739, 460, 96, Color.clear, () => RequestedEndless?.Invoke());
            Label(endlessButton.transform, "Sonsuz Akış", 27, UiResources.Paper, 0, 3, 375, 40);
            homeRecord = Label(endlessButton.transform, "En iyi  0", 18, UiResources.Muted, 0, 48, 375, 28);
            Icon(endlessButton.transform, UiGlyph.Infinity, UiResources.Paper, 410, 24, 26, 26);
        }

        private void BuildLevels()
        {
            levelMap = levels.AddComponent<LevelMapView>();
            levelMap.Initialize(UiResources.Body, UiResources.CircleSprite, UiResources.CircleSprite,
                () => RequestedHome?.Invoke(), index => playLevel?.Invoke(index));
        }

        private void BuildEndless()
        {
            Header(endless.transform, "SONSUZ AKIŞ");
            Label(endless.transform, "EN İYİ SKOR", 17, UiResources.Muted, 40, 246, 460, 30, false, TextAnchor.MiddleCenter);
            endlessRecord = Label(endless.transform, "0", 100, UiResources.Paper, 40, 283, 460, 126, true, TextAnchor.MiddleCenter);
            Rule(endless.transform, 40, 463, 460);
            Label(endless.transform, "Küreleri engellerden koru.\nÇarpışana kadar devam et.", 22, UiResources.Paper, 40, 501, 460, 72, false, TextAnchor.MiddleCenter);
            Label(endless.transform, "Skor = tam metre + kristal × 100", 16, UiResources.Muted, 40, 589, 460, 28, false, TextAnchor.MiddleCenter);
            var start = Button("Start endless", endless.transform, 40, 731, 460, 84, UiResources.Paper, () => playEndless?.Invoke());
            Label(start.transform, "AKIŞA BAŞLA", 32, UiResources.Background, 24, 0, 345, 84, true);
            Icon(start.transform, UiGlyph.Play, UiResources.Background, 404, 29, 26, 26);
        }

        private void BuildSettings()
        {
            Header(settings.transform, "AYARLAR");
            audioStateIcon = Icon(settings.transform, UiGlyph.SoundOn, UiResources.Paper, 40, 245, 26, 26);
            Label(settings.transform, "Ses", 26, UiResources.Paper, 82, 226, 275, 38);
            audioCaption = Label(settings.transform, "Açık", 18, UiResources.Muted, 82, 267, 250, 28);

            var toggleArea = Panel("Sound toggle", settings.transform, Color.clear, 386, 222, 114, 80);
            toggleArea.raycastTarget = true;
            audioToggle = toggleArea.gameObject.AddComponent<Toggle>();
            toggleTrack = Panel("Track", toggleArea.transform, UiResources.Surface, 10, 20, 94, 40);
            audioToggle.targetGraphic = toggleTrack;
            var knob = Panel("Knob", toggleArea.transform, UiResources.Paper, 16, 26, 28, 28);
            knob.sprite = UiResources.CircleSprite; knob.type = UnityEngine.UI.Image.Type.Simple;
            toggleKnob = knob.rectTransform;
            var check = Icon(knob.transform, UiGlyph.Check, UiResources.Background, 7, 7, 14, 14);
            audioToggle.graphic = check;
            SetSelectableColors(audioToggle);
            audioToggle.onValueChanged.AddListener(value => { UpdateAudioPresentation(value); setAudio?.Invoke(value); });

            Rule(settings.transform, 40, 326, 460);
            Label(settings.transform, "Ses düzeyi", 23, UiResources.Paper, 40, 352, 300, 36);
            volumeCaption = Label(settings.transform, "100%", 21, UiResources.Muted, 370, 352, 130, 36, false, TextAnchor.MiddleRight);
            var sliderRect = At("Volume", settings.transform, 40, 402, 460, 72);
            var sliderTouchArea = Image(sliderRect, Color.clear); sliderTouchArea.raycastTarget = true;
            volumeSlider = sliderRect.gameObject.AddComponent<Slider>();
            Panel("Rail", sliderRect, UiResources.Line, 14, 32, 432, 8);
            var fillArea = Rect("Fill area", sliderRect, Vector2.zero, Vector2.one, new Vector2(14, 32), new Vector2(-14, -32));
            var fill = Image(Rect("Fill", fillArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero), UiResources.Accent);
            volumeSlider.fillRect = fill.rectTransform;
            var handleArea = Rect("Handle area", sliderRect, new Vector2(0, .5f), new Vector2(1, .5f), new Vector2(14, -14), new Vector2(-14, 14));
            var handle = Image(Rect("Handle", handleArea, Vector2.zero, new Vector2(0, 1), new Vector2(-14, 0), new Vector2(14, 0)), UiResources.Paper);
            handle.sprite = UiResources.CircleSprite; handle.type = UnityEngine.UI.Image.Type.Simple;
            volumeSlider.handleRect = handle.rectTransform; volumeSlider.targetGraphic = handle;
            volumeSlider.minValue = 0; volumeSlider.maxValue = 1; volumeSlider.direction = Slider.Direction.LeftToRight;
            SetSelectableColors(volumeSlider);
            volumeSlider.onValueChanged.AddListener(value => { UpdateVolumeCaption(value); setVolume?.Invoke(value); });
            Rule(settings.transform, 40, 500, 460);

            Label(settings.transform, "KONTROL", 16, UiResources.Muted, 40, 548, 460, 28);
            Label(settings.transform, "Basılı tut", 23, UiResources.Paper, 40, 595, 172, 36);
            Label(settings.transform, "Küreler açılır.", 21, UiResources.Muted, 218, 595, 282, 36);
            Rule(settings.transform, 40, 650, 460);
            Label(settings.transform, "Bırak", 23, UiResources.Paper, 40, 671, 172, 36);
            Label(settings.transform, "Küreler daralır.", 21, UiResources.Muted, 218, 671, 282, 36);
            Label(settings.transform, "Enerji doluyken bırak:\nyakındaki işaretli hedefe tek Pulse.", 18, UiResources.Muted, 40, 749, 460, 60);
        }

        private void Header(Transform page, string title)
        {
            var back = Button("Back", page, 20, 22, 72, 72, Color.clear, () => RequestedHome?.Invoke());
            Icon(back.transform, UiGlyph.Back, UiResources.Paper, 23, 23, 26, 26);
            Label(page, title, 52, UiResources.Paper, 40, 115, 460, 68, true);
            Rule(page, 40, 199, 460);
        }

        private void UpdateProgressCaption()
        {
            if (homeProgress != null) homeProgress.text = "Bölüm " + (nextLevel + 1) + "  •  " + completedLevels + "/25";
        }

        private void SetAudioValues(bool audio, float volume)
        {
            audioToggle.SetIsOnWithoutNotify(audio); UpdateAudioPresentation(audio);
            volumeSlider.SetValueWithoutNotify(Mathf.Clamp01(volume)); UpdateVolumeCaption(volumeSlider.value);
        }

        private void UpdateAudioPresentation(bool audio)
        {
            audioCaption.text = audio ? "Açık" : "Kapalı";
            audioStateIcon.Glyph = audio ? UiGlyph.SoundOn : UiGlyph.SoundOff;
            toggleTrack.color = audio ? UiResources.Accent : UiResources.Surface;
            var position = toggleKnob.anchoredPosition;
            position.x = audio ? 84f : 30f;
            toggleKnob.anchoredPosition = position;
        }

        private void UpdateVolumeCaption(float value) { volumeCaption.text = Mathf.RoundToInt(value * 100) + "%"; }

        private void Show(GameObject page)
        {
            home.SetActive(page == home); levels.SetActive(page == levels); endless.SetActive(page == endless); settings.SetActive(page == settings);
            Canvas.gameObject.SetActive(true); ApplySafeArea();
            if (menuHero != null) menuHero.SetVisible(page == home && Canvas.gameObject.activeInHierarchy);
        }

        private GameObject Page(string name) { return Rect(name, safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject; }
        private void Update() { if (Canvas != null && Canvas.gameObject.activeSelf) ApplySafeArea(); }
        private void OnDisable() { if (menuHero != null) menuHero.SetVisible(false); }

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
            var rect = (RectTransform)go.transform; rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = low; rect.offsetMax = high; return rect;
        }

        private RectTransform At(string name, Transform parent, float x, float y, float width, float height)
        {
            return Rect(name, parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, -y - height), new Vector2(x + width, -y));
        }

        private Image Panel(string name, Transform parent, Color color, float x, float y, float width, float height)
        {
            return Image(At(name, parent, x, y, width, height), color);
        }

        private void Rule(Transform parent, float x, float y, float width)
        {
            var line = Panel("Divider", parent, UiResources.Line, x, y, width, 1);
            line.sprite = null; line.type = UnityEngine.UI.Image.Type.Simple;
        }

        private Image Image(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = UiResources.PanelSprite;
            image.type = UnityEngine.UI.Image.Type.Sliced; image.color = color; image.raycastTarget = false; return image;
        }

        private Text Label(Transform parent, string value, int size, Color color, float x, float y, float width, float height, bool display = false, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var text = At(string.IsNullOrEmpty(value) ? "Progress" : value, parent, x, y, width, height).gameObject.AddComponent<Text>();
            text.font = display ? UiResources.Display : UiResources.Body; text.text = value; text.fontSize = size; text.fontStyle = FontStyle.Normal;
            text.color = color; text.alignment = alignment; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; return text;
        }

        private UiIcon Icon(Transform parent, UiGlyph glyph, Color color, float x, float y, float width, float height)
        {
            var rect = At(glyph.ToString(), parent, x, y, width, height);
            rect.gameObject.AddComponent<CanvasRenderer>();
            var icon = rect.gameObject.AddComponent<UiIcon>(); icon.Glyph = glyph; icon.color = color; icon.raycastTarget = false; return icon;
        }

        private Button Button(string name, Transform parent, float x, float y, float width, float height, Color background, Action callback)
        {
            var image = Panel(name, parent, background, x, y, width, height); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            SetSelectableColors(button); button.onClick.AddListener(() => callback?.Invoke()); return button;
        }

        private static void SetSelectableColors(Selectable selectable)
        {
            var colors = selectable.colors;
            colors.normalColor = Color.white; colors.highlightedColor = new Color(.92f, .95f, 1f);
            colors.pressedColor = new Color(.76f, .82f, .89f); colors.selectedColor = Color.white;
            colors.fadeDuration = .08f; selectable.colors = colors;
        }
    }
}
