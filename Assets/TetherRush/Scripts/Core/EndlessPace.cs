using System;

namespace TetherRush.Core
{
    /// <summary>Score-driven endless pace. Advance only while the run is Running.</summary>
    public sealed class EndlessPace
    {
        public const float StartSpeed = 7f;
        public const float MaximumSpeed = 11.5f;
        public const int GraceScore = 500;
        public const int MaximumPaceScore = 5500;
        public const float MaximumAcceleration = .25f;

        // Retain precision when the runtime advances in 1/120 second substeps.
        private double currentSpeed = StartSpeed;
        public float CurrentSpeed => (float)currentSpeed;

        public void Reset() => currentSpeed = StartSpeed;

        public static float TargetSpeedForScore(int score)
        {
            if (score < 0) throw new ArgumentOutOfRangeException(nameof(score));
            double progress = Math.Min(1d, Math.Max(0d,
                (score - (double)GraceScore) / (MaximumPaceScore - GraceScore)));
            double smooth = progress * progress * (3d - 2d * progress);
            return (float)(StartSpeed + (MaximumSpeed - StartSpeed) * smooth);
        }

        /// <summary>
        /// Approach the score's target at at most MaximumAcceleration metres/second squared.
        /// Bonus score changes cannot instantly change speed; a lower score never slows a run.
        /// </summary>
        public float Advance(float dt, int score)
        {
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0f)
                throw new ArgumentOutOfRangeException(nameof(dt), "Delta time must be finite and nonnegative.");
            float target = TargetSpeedForScore(score);
            currentSpeed = Math.Max(currentSpeed,
                Math.Min(target, currentSpeed + MaximumAcceleration * (double)dt));
            return CurrentSpeed;
        }
    }
}
