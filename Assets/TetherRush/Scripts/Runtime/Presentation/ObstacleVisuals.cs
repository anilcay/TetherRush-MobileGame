using TetherRush.Art;
using UnityEngine;

namespace TetherRush.Presentation
{
    /// <summary>Stable cosmetic variety. Does not mutate course data, contacts, motion or Unity random state.</summary>
    public static class ObstacleVisuals
    {
        public static VisualKind Resolve(string courseId, float distance, VisualKind original)
        {
            if (string.IsNullOrEmpty(courseId)) return original;
            uint key = 2166136261u;
            unchecked
            {
                foreach (char c in courseId) key = (key ^ c) * 16777619u;
                key ^= (uint)Mathf.RoundToInt(distance * 4);
                key ^= key >> 16; key *= 0x7feb352du; key ^= key >> 15;
                key *= 0x846ca68bu; key ^= key >> 16;
            }
            // Keep a quarter of encounters in the familiar kit. A paired gate uses
            // the same course/Z key on both sides, independent of world X.
            if ((key & 3u) == 0) return original;
            switch (original)
            {
                case VisualKind.SplitPylon: return VisualKind.ClampBarrier;
                case VisualKind.ChevronBarrier: return VisualKind.CapacitorBank;
                case VisualKind.MovingWall: return VisualKind.HydraulicSlider;
                case VisualKind.RotatingArm: return VisualKind.TurbineBeam;
                default: return original;
            }
        }
    }
}
