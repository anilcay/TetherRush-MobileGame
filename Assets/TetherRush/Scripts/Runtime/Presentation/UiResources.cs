using UnityEngine;

namespace TetherRush.Presentation
{
    /// <summary>Shared UI assets; world art keeps its own VisualTheme palette.</summary>
    public static class UiResources
    {
        private static Font body, display;
        private static Sprite panel, circle, coin;
        public static Font Body => body != null ? body : body = LoadFont("Barlow-SemiBold");
        public static Font Display => display != null ? display : display = LoadFont("BarlowCondensed-ExtraBold");
        public static Sprite PanelSprite => panel != null ? panel : panel = Resources.Load<Sprite>("UI/Textures/Panel");
        public static Sprite CircleSprite => circle != null ? circle : circle = Resources.Load<Sprite>("UI/Textures/Circle");
        public static Sprite CoinSprite => coin != null ? coin : coin = Resources.Load<Sprite>("UI/Textures/Coin");
        public static readonly Color Background = new Color32(13, 23, 34, 255);
        public static readonly Color Surface = new Color32(26, 42, 59, 255);
        public static readonly Color Paper = new Color32(245, 242, 233, 255);
        public static readonly Color Muted = new Color32(131, 147, 163, 255);
        public static readonly Color Accent = VisualTheme.Tether;
        public static readonly Color Line = new Color32(49, 67, 86, 255);
        private static Font LoadFont(string name)
        {
            var font = Resources.Load<Font>("UI/Fonts/" + name);
            return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
