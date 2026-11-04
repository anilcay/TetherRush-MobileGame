using UnityEngine;

namespace TetherRush.Presentation
{
    /// <summary>Shared semantic palette for the course, heroes, feedback and HUD.</summary>
    public static class VisualTheme
    {
        public static readonly Color Void = Hex(0x0A101D);
        public static readonly Color Atmosphere = Hex(0x263F59);
        public static readonly Color Track = Hex(0x213247);
        public static readonly Color TrackAccent = Hex(0x3E728C);
        public static readonly Color Metal = Hex(0x3A4B62);
        public static readonly Color Inlay = Hex(0x0E1929);
        public static readonly Color Silver = Hex(0xBBCBDD);
        public static readonly Color CoreA = Hex(0xFFB48F);
        public static readonly Color CoreB = Hex(0xB5A4FF);
        public static readonly Color Tether = Hex(0x42DFF5);
        public static readonly Color TetherReady = Hex(0xA7F3FF);
        public static readonly Color Energy = Hex(0x66EBF5);
        public static readonly Color Bonus = Hex(0xFFDFA0);
        public static readonly Color Danger = Hex(0xCF7B83);
        public static readonly Color DangerBody = Hex(0x405364);
        public static readonly Color PulseTarget = Hex(0x6B6254);
        public static readonly Color PulseAccent = Hex(0xD8AC64);
        public static readonly Color UIBackground = Hex(0x102035);
        public static readonly Color UIText = Hex(0xEAF4FC);
        public static readonly Color UIMuted = Hex(0x9BAFC5);

        private static Color Hex(uint rgb) => new Color(((rgb >> 16) & 255) / 255f,
            ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1f);
    }
}
