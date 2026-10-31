using UnityEngine;

namespace TetherRush.Course
{
    /// <summary>Separate M2 practice; tuning reference speed is 7 world units per second.</summary>
    public static class TimedCourseLayout
    {
        public const float FinishDistance = 130f;

        public static TimedHazard[] Create()
        {
            return new[]
            {
                new TimedHazard
                {
                    Kind = TimedHazardKind.MovingWall, Center = new Vector2(0, 45),
                    HalfExtents = new Vector2(.65f, .45f), Amplitude = .65f, AngularSpeed = .8f,
                    Phase = -.8f * (45f / 7f), Hint = "Hareketli duvar: basılı tut ve geniş geç."
                },
                new TimedHazard
                {
                    Kind = TimedHazardKind.MovingWall, Center = new Vector2(0, 75),
                    HalfExtents = new Vector2(.45f, .45f), Amplitude = 2.2f, AngularSpeed = .6f,
                    Phase = Mathf.PI * .5f - .6f * (75f / 7f), Hint = "Duvar yana kayıyor: bırak ve dar geç."
                },
                new TimedHazard
                {
                    Kind = TimedHazardKind.RotatingArm, Center = new Vector2(0, 100),
                    HalfExtents = new Vector2(2.7f, .14f), AngularSpeed = .9f,
                    Phase = Mathf.PI * .5f - .9f * (100f / 7f), Hint = "Dönen kol: basılı tut ve uçların dışından geç."
                }
            };
        }
    }
}
