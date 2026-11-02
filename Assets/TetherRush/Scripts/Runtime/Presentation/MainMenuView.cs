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
        private Text homeRecord, endlessRecord, volumeCaption, audioCaption;
        private readonly Text[] levelTitles = new Text[5], levelLessons = new Text[5], levelResults = new Text[5];
        private Toggle audioToggle;
        private Slider volumeSlider;
        private Font font;
        private Texture2D roundedTexture;
        private Sprite roundedSprite;
        private Texture2D coreTexture, haloTexture;
        private Sprite coreSprite, haloSprite;
        private Rect previousSafeArea;
        private int previousWidth, previousHeight;

        public void Initialize(Action<int> playLevel, Action playEndless, Action<bool> setAudio, Action<float> setVolume)
        {
            if (Canvas != null) return;
            this.playLevel = playLevel; this.playEndless = playEndless;
            this.setAudio = setAudio; this.setVolume = setVolume;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateRoundedSprite();
            CreateCoreSprites();
            var root = new GameObject("Tether Rush Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            Canvas = root.GetComponent<Canvas>(); Canvas.renderMode = RenderMode.ScreenSpaceOverlay; Canvas.sortingOrder = 30;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960); scaler.matchWidthOrHeight = 0f;
            var background = root.AddComponent<Image>(); background.color = VisualTheme.Void;
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

        public void ShowHome(int bestScore, bool audio, float volume)
        {
            if (Canvas == null) return;
            homeRecord.text = "EN İYİ  " + bestScore.ToString("N0");
            SetAudioValues(audio, volume); Show(home);
        }

        public void ShowLevels(string[] titles, string[] descriptions, float[] lengths, bool[] completed, int[] bestBonus)
        {
            if (Canvas == null) return;
            for (int i = 0; i < 5; i++)
            {
                levelTitles[i].text = At(titles, i, "Bölüm " + (i + 1));
                levelLessons[i].text = At(descriptions, i, "Bağın ritmini keşfet.");
                int seconds = Mathf.RoundToInt(At(lengths, i, 0f));
                levelResults[i].text = (At(completed, i, false) ? "TAMAMLANDI" : "KEŞFET") + "   ·   " + seconds + " sn   ·   EN İYİ BONUS " + At(bestBonus, i, 0);
                levelResults[i].color = At(completed, i, false) ? VisualTheme.Tether : VisualTheme.UIMuted;
            }
            Show(levels);
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

        public void Hide() { if (Canvas != null) Canvas.gameObject.SetActive(false); }

        private void BuildHome()
        {
            Label(home.transform, "BAĞINI BUL. RİTMİNİ YAKALA.", 12, VisualTheme.Tether, 30, 39, 480, 20);
            Label(home.transform, "TETHER\nRUSH", 62, VisualTheme.UIText, 27, 77, 480, 150, FontStyle.Bold);
            Button("Settings", home.transform, "Ayarlar", 390, 35, 120, 48, VisualTheme.Track, VisualTheme.Silver, () => RequestedSettings?.Invoke(), 15);
            var hero = Panel("Paired cores", home.transform, VisualTheme.UIBackground, 30, 260, 480, 225);
            Label(hero.transform, "İKİ KÜRE. TEK BAĞ.", 12, VisualTheme.UIMuted, 22, 18, 420, 20);
            Panel("Tether glow", hero.transform, Alpha(VisualTheme.Tether, .13f), 77, 102, 326, 30);
            Panel("Tether", hero.transform, VisualTheme.Tether, 85, 114, 310, 5);
            Core(hero.transform, "Peach", 63, 77, VisualTheme.CoreA);
            Core(hero.transform, "Lavender", 347, 77, VisualTheme.CoreB);
            Label(hero.transform, "Basılı tut, açıl. Bırak, daral.", 16, VisualTheme.Silver, 22, 178, 436, 25, FontStyle.Normal, TextAnchor.MiddleCenter);
            Button("Levels", home.transform, "Leveller                         →", 30, 517, 480, 86, VisualTheme.Tether, VisualTheme.Void, () => RequestedLevels?.Invoke(), 27);
            Label(home.transform, "Beş kısa parkurla bağın ustası ol.", 15, VisualTheme.UIMuted, 33, 612, 470, 27);
            var endlessButton = Button("Endless", home.transform, "", 30, 665, 480, 117, VisualTheme.UIBackground, VisualTheme.UIText, () => RequestedEndless?.Invoke());
            Label(endlessButton.transform, "Sonsuz Akış", 25, VisualTheme.UIText, 22, 18, 375, 32, FontStyle.Bold);
            homeRecord = Label(endlessButton.transform, "EN İYİ  0", 14, VisualTheme.CoreB, 22, 65, 380, 24);
            Label(endlessButton.transform, "→", 28, VisualTheme.Tether, 416, 36, 40, 40);
            Label(home.transform, "TEK DOKUNUŞ  /  KESİNTİSİZ AKIŞ", 11, VisualTheme.UIMuted, 30, 846, 480, 30, FontStyle.Normal, TextAnchor.MiddleCenter);
        }

        private void BuildLevels()
        {
            Header(levels.transform, "Leveller", "Kısa parkurlar. Her turda yeni bir ritim.");
            for (int i = 0; i < 5; i++)
            {
                int index = i;
                var button = Button("Level " + (i + 1), levels.transform, "", 30, 177 + i * 132, 480, 116, VisualTheme.UIBackground, VisualTheme.UIText, () => playLevel?.Invoke(index));
                Label(button.transform, (i + 1).ToString("00"), 19, i % 2 == 0 ? VisualTheme.CoreA : VisualTheme.CoreB, 18, 18, 42, 30, FontStyle.Bold);
                levelTitles[i] = Label(button.transform, "", 23, VisualTheme.UIText, 71, 15, 353, 32, FontStyle.Bold);
                levelLessons[i] = Label(button.transform, "", 14, VisualTheme.Silver, 71, 49, 382, 32);
                levelResults[i] = Label(button.transform, "", 11, VisualTheme.UIMuted, 71, 83, 384, 20);
                Label(button.transform, "›", 25, VisualTheme.Tether, 438, 16, 25, 30);
            }
            Label(levels.transform, "Tüm parkurlar keşfine açık.", 13, VisualTheme.UIMuted, 30, 858, 480, 28, FontStyle.Normal, TextAnchor.MiddleCenter);
        }

        private void BuildEndless()
        {
            Header(endless.transform, "Sonsuz Akış", "Ritmi koru. Kendi rekorunu aş.");
            var record = Panel("Best score", endless.transform, VisualTheme.UIBackground, 30, 192, 480, 194);
            Label(record.transform, "EN İYİ SKOR", 13, VisualTheme.CoreB, 24, 25, 432, 25, FontStyle.Bold, TextAnchor.MiddleCenter);
            endlessRecord = Label(record.transform, "0", 64, VisualTheme.UIText, 24, 67, 432, 92, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(endless.transform, "Akış çarpışana kadar sürer.", 23, VisualTheme.UIText, 30, 433, 480, 40, FontStyle.Bold);
            Label(endless.transform, "İleri hız sabittir. Küreleri engellerden koru,\nbonus kristalleri topla ve mesafeni büyüt.", 18, VisualTheme.Silver, 30, 489, 480, 66);
            var formula = Panel("Score explanation", endless.transform, VisualTheme.Inlay, 30, 591, 480, 91);
            Label(formula.transform, "SKOR = TAM METRE + BONUS × 100", 17, VisualTheme.Tether, 20, 16, 440, 27, FontStyle.Bold);
            Label(formula.transform, "Her tam metre 1, her bonus kristal 100 puan.", 14, VisualTheme.UIMuted, 20, 51, 440, 25);
            Button("Start endless", endless.transform, "Akışa başla   →", 30, 731, 480, 82, VisualTheme.Tether, VisualTheme.Void, () => playEndless?.Invoke(), 24);
        }

        private void BuildSettings()
        {
            Header(settings.transform, "Ayarlar", "Kendi ritmine göre.");
            var sound = Panel("Sound", settings.transform, VisualTheme.UIBackground, 30, 193, 480, 235);
            Label(sound.transform, "Ses", 25, VisualTheme.UIText, 23, 22, 260, 36, FontStyle.Bold);
            audioCaption = Label(sound.transform, "Açık", 16, VisualTheme.Silver, 23, 61, 260, 29);
            var toggleBg = Panel("Sound toggle", sound.transform, VisualTheme.Track, 372, 24, 84, 48);
            toggleBg.raycastTarget = true; audioToggle = toggleBg.gameObject.AddComponent<Toggle>(); audioToggle.targetGraphic = toggleBg;
            var check = Panel("On indicator", toggleBg.transform, VisualTheme.Tether, 8, 8, 68, 32);
            audioToggle.graphic = check;
            audioToggle.onValueChanged.AddListener(value => { audioCaption.text = value ? "Açık" : "Kapalı"; setAudio?.Invoke(value); });
            volumeCaption = Label(sound.transform, "Ses düzeyi  100%", 16, VisualTheme.Silver, 23, 119, 430, 27);
            var sliderRect = Rect("Volume", sound.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(23, -207), new Vector2(457, -157));
            var sliderTouchArea = Image(sliderRect, Color.clear); sliderTouchArea.raycastTarget = true;
            volumeSlider = sliderRect.gameObject.AddComponent<Slider>();
            var rail = Panel("Rail", sliderRect, VisualTheme.Track, 0, 21, 434, 8);
            var fillArea = Rect("Fill area", sliderRect, Vector2.zero, Vector2.one, new Vector2(12, 21), new Vector2(-12, -21));
            var fill = StretchPanel("Fill", fillArea, VisualTheme.Tether); volumeSlider.fillRect = fill.rectTransform;
            var handleArea = Rect("Handle area", sliderRect, Vector2.zero, Vector2.one, new Vector2(12, 0), new Vector2(-12, 0));
            var handle = StretchPanel("Handle", handleArea, VisualTheme.UIText);
            handle.rectTransform.anchorMin = new Vector2(0, 0); handle.rectTransform.anchorMax = new Vector2(0, 1);
            handle.rectTransform.sizeDelta = new Vector2(26, -22); handle.raycastTarget = true;
            volumeSlider.handleRect = handle.rectTransform; volumeSlider.targetGraphic = handle;
            rail.raycastTarget = true;
            volumeSlider.minValue = 0; volumeSlider.maxValue = 1;
            volumeSlider.onValueChanged.AddListener(value => { UpdateVolumeCaption(value); setVolume?.Invoke(value); });
            var controls = Panel("Controls", settings.transform, VisualTheme.UIBackground, 30, 462, 480, 317);
            Label(controls.transform, "Tek dokunuş, tam kontrol.", 23, VisualTheme.UIText, 23, 24, 434, 38, FontStyle.Bold);
            Label(controls.transform, "01   BASILI TUT", 14, VisualTheme.CoreA, 23, 88, 434, 25, FontStyle.Bold);
            Label(controls.transform, "Küreler simetrik olarak açılır.", 17, VisualTheme.Silver, 23, 118, 434, 30);
            Label(controls.transform, "02   BIRAK", 14, VisualTheme.CoreB, 23, 166, 434, 25, FontStyle.Bold);
            Label(controls.transform, "Küreler daralır. Enerji tam doluysa\ntek Pulse, yakındaki işaretli hedefi etkiler.", 17, VisualTheme.Silver, 23, 199, 434, 68);
            Label(settings.transform, "Ses tercihlerin bu cihazda saklanır.", 13, VisualTheme.UIMuted, 30, 822, 480, 30, FontStyle.Normal, TextAnchor.MiddleCenter);
        }

        private void Header(Transform page, string title, string description)
        {
            Button("Back", page, "‹  Ana menü", 30, 32, 155, 48, VisualTheme.Track, VisualTheme.Silver, () => RequestedHome?.Invoke(), 16);
            Label(page, title, 38, VisualTheme.UIText, 30, 96, 480, 53, FontStyle.Bold);
            Label(page, description, 15, VisualTheme.UIMuted, 31, 148, 479, 25);
        }

        private void Core(Transform parent, string name, float x, float y, Color color)
        {
            var halo = Panel(name + " halo", parent, Alpha(color, .3f), x - 12, y - 12, 94, 94);
            halo.sprite = haloSprite; halo.type = UnityEngine.UI.Image.Type.Simple; halo.preserveAspect = true;
            var core = Panel(name, parent, color, x, y, 70, 70);
            core.sprite = coreSprite; core.type = UnityEngine.UI.Image.Type.Simple; core.preserveAspect = true;
        }

        private void SetAudioValues(bool audio, float volume)
        {
            audioToggle.SetIsOnWithoutNotify(audio); audioCaption.text = audio ? "Açık" : "Kapalı";
            volumeSlider.SetValueWithoutNotify(Mathf.Clamp01(volume)); UpdateVolumeCaption(volumeSlider.value);
        }
        private void UpdateVolumeCaption(float value) { volumeCaption.text = "Ses düzeyi  " + Mathf.RoundToInt(value * 100) + "%"; }
        private void Show(GameObject page)
        {
            home.SetActive(page == home); levels.SetActive(page == levels); endless.SetActive(page == endless); settings.SetActive(page == settings);
            Canvas.gameObject.SetActive(true); ApplySafeArea();
        }
        private GameObject Page(string name) { return Rect(name, safeArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject; }
        private void Update() { if (Canvas != null && Canvas.gameObject.activeSelf) ApplySafeArea(); }
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
        private Image Panel(string name, Transform parent, Color color, float x, float y, float width, float height)
        {
            var rect = Rect(name, parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, -y - height), new Vector2(x + width, -y));
            return Image(rect, color);
        }
        private Image StretchPanel(string name, Transform parent, Color color) { return Image(Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero), color); }
        private Image Image(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.sprite = roundedSprite; image.type = UnityEngine.UI.Image.Type.Sliced; image.color = color; image.raycastTarget = false; return image;
        }
        private Text Label(Transform parent, string value, int size, Color color, float x, float y, float width, float height, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var rect = Rect(value, parent, new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, -y - height), new Vector2(x + width, -y));
            var text = rect.gameObject.AddComponent<Text>(); text.font = font; text.text = value; text.fontSize = size; text.fontStyle = style;
            text.color = color; text.alignment = alignment; text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; return text;
        }
        private Button Button(string name, Transform parent, string caption, float x, float y, float width, float height, Color background, Color foreground, Action callback, int size = 20)
        {
            var image = Panel(name, parent, background, x, y, width, height); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(.91f, .97f, 1); colors.pressedColor = new Color(.67f, .8f, .9f); colors.selectedColor = Color.white; colors.fadeDuration = .1f; button.colors = colors;
            button.onClick.AddListener(() => callback?.Invoke());
            if (!string.IsNullOrEmpty(caption)) Label(image.transform, caption, size, foreground, 16, 0, width - 32, height, FontStyle.Bold, TextAnchor.MiddleCenter);
            return button;
        }
        private void CreateRoundedSprite()
        {
            const int size = 64; const float radius = 20;
            roundedTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Menu corners", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x + .5f - size / 2f) - (size / 2f - radius), 0);
                float dy = Mathf.Max(Mathf.Abs(y + .5f - size / 2f) - (size / 2f - radius), 0);
                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(radius + .5f - Mathf.Sqrt(dx * dx + dy * dy)));
            }
            roundedTexture.SetPixels(pixels); roundedTexture.Apply(false, true);
            roundedSprite = Sprite.Create(roundedTexture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(22, 22, 22, 22));
        }
        private void CreateCoreSprites()
        {
            // A tintable sphere glyph: circular antialiasing, upper-left light and a soft rim.
            // Generated once; no animation or per-frame texture allocation is needed.
            const int size = 128;
            coreTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Menu shaded sphere", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            haloTexture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Menu circular halo", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var corePixels = new Color[size * size]; var haloPixels = new Color[size * size];
            var light = new Vector3(-.38f, .5f, .78f).normalized;
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float nx = (x + .5f - size * .5f) / (size * .5f - 1);
                float ny = (y + .5f - size * .5f) / (size * .5f - 1);
                float radius = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.Clamp01((1 - radius) * (size * .5f - 1));
                float nz = Mathf.Sqrt(Mathf.Max(0, 1 - radius * radius));
                float diffuse = Mathf.Max(0, Vector3.Dot(new Vector3(nx, ny, nz), light));
                float highlight = Mathf.Exp(-((nx + .3f) * (nx + .3f) + (ny - .38f) * (ny - .38f)) / .045f);
                float shade = Mathf.Clamp01(.5f + diffuse * .43f + highlight * .14f);
                corePixels[y * size + x] = new Color(shade, shade, shade, alpha);
                haloPixels[y * size + x] = new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - radius), 1.5f));
            }
            coreTexture.SetPixels(corePixels); coreTexture.Apply(false, true);
            haloTexture.SetPixels(haloPixels); haloTexture.Apply(false, true);
            coreSprite = Sprite.Create(coreTexture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            haloSprite = Sprite.Create(haloTexture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
        }
        private static T At<T>(T[] values, int index, T fallback) { return values != null && index < values.Length ? values[index] : fallback; }
        private static Color Alpha(Color color, float alpha) { color.a = alpha; return color; }
        private void OnDestroy()
        {
            if (roundedSprite != null) Destroy(roundedSprite); if (roundedTexture != null) Destroy(roundedTexture);
            if (coreSprite != null) Destroy(coreSprite); if (coreTexture != null) Destroy(coreTexture);
            if (haloSprite != null) Destroy(haloSprite); if (haloTexture != null) Destroy(haloTexture);
        }
    }
}
