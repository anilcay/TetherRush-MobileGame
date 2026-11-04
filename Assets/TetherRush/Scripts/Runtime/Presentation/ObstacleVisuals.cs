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
            // One coherent revised kit throughout authored/endless routes. Legacy
            // prototype scenes do not call this resolver and retain their original kit.
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
