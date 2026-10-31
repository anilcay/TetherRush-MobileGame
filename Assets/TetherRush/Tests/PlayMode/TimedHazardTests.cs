using System;
using NUnit.Framework;
using TetherRush.Course;
using UnityEngine;

namespace TetherRush.Tests
{
    public sealed class TimedHazardTests
    {
        private static TimedHazard Wall()
        {
            return new TimedHazard { Kind = TimedHazardKind.MovingWall, HalfExtents = new Vector2(.5f, .5f) };
        }

        [Test]
        public void WallSineUsesPhaseAndKeepsForwardPositionFixed()
        {
            var h = Wall();
            h.Center = new Vector2(1, 45); h.Amplitude = 2; h.AngularSpeed = Mathf.PI;
            Assert.That(h.PositionAt(.5f).x, Is.EqualTo(3).Within(.00001));
            Assert.That(h.PositionAt(1.5f).x, Is.EqualTo(-1).Within(.00001));
            Assert.That(h.PositionAt(.5f).y, Is.EqualTo(45));
            Assert.That(h.AngleAt(.5f), Is.Zero);
            h.Phase = Mathf.PI * .5f;
            Assert.That(h.PositionAt(0).x, Is.EqualTo(3).Within(.00001));
        }

        [Test]
        public void RotatedBoxRejectsEmptyAabbCornerAndAcceptsActualArm()
        {
            var h = new TimedHazard { Kind = TimedHazardKind.RotatingArm,
                HalfExtents = new Vector2(2, .1f), Phase = Mathf.PI / 4 };
            var emptyCorner = new Vector2(1.3f, -1.3f);
            Assert.That(h.HitsSweptSphere(emptyCorner, emptyCorner, .1f, 0, 0), Is.False);
            var onArm = new Vector2(1, 1);
            Assert.That(h.HitsSweptSphere(onArm, onArm, .1f, 0, 0), Is.True);
            Assert.That(h.AngleAt(0), Is.EqualTo(Mathf.PI / 4));
        }

        [TestCase(-4f, 4f)]
        [TestCase(4f, -4f)]
        public void SpatialSweepCannotTunnelFromEitherSide(float a, float b)
        {
            Assert.That(Wall().HitsSweptSphere(new Vector2(a, 0), new Vector2(b, 0), .1f, 0, 1), Is.True);
        }

        [Test]
        public void WallCanCrossStationarySphereWithBothEndpointsClear()
        {
            var h = Wall(); h.Amplitude = 4; h.AngularSpeed = Mathf.PI; h.Phase = -Mathf.PI / 2;
            Assert.That(h.HitsSweptSphere(Vector2.zero, Vector2.zero, .1f, 0, 0), Is.False);
            Assert.That(h.HitsSweptSphere(Vector2.zero, Vector2.zero, .1f, 1, 1), Is.False);
            Assert.That(h.HitsSweptSphere(Vector2.zero, Vector2.zero, .1f, 0, 1), Is.True);
        }

        [Test]
        public void FullRotationIntervalCannotTunnelAndDoesNotInflateOutsideArm()
        {
            var h = new TimedHazard { Kind = TimedHazardKind.RotatingArm,
                HalfExtents = new Vector2(2, .1f), AngularSpeed = Mathf.PI * 8 };
            var p = new Vector2(0, 1.5f);
            Assert.That(h.HitsSweptSphere(p, p, .1f, 0, 0), Is.False);
            Assert.That(h.HitsSweptSphere(p, p, .1f, 0, 1), Is.True);
            var outside = new Vector2(2.2f, 0);
            Assert.That(h.HitsSweptSphere(outside, outside, .1f, 0, 1), Is.False);
        }

        [Test]
        public void CircleCornerMissAndExactSideTouchUseRoundedBoxGeometry()
        {
            var h = Wall();
            var corner = new Vector2(.59f, .59f);
            Assert.That(h.HitsSweptSphere(corner, corner, .1f, 0, 0), Is.False);
            // Binary-exact values make this an exact touch, including at the sweep endpoint.
            Assert.That(h.HitsSweptSphere(new Vector2(2, 0), new Vector2(.75f, 0), .25f, 0, 0), Is.True);
            Assert.That(h.HitsSweptSphere(new Vector2(.752f, 0), new Vector2(.752f, 0), .25f, 0, 1), Is.False);
        }

        [Test]
        public void SmallSphereAndFastRelativeMotionStillCollide()
        {
            var h = Wall(); h.HalfExtents = new Vector2(.0001f, .0001f);
            Assert.That(h.HitsSweptSphere(new Vector2(-100, 0), new Vector2(101, 0), .0001f, 0, 10), Is.True);
            Assert.That(h.HitsSweptSphere(new Vector2(-100, .01f), new Vector2(101, .01f), .0001f, 0, 10), Is.False);
        }

        [Test]
        public void SameSimulationTimeFreezesAndResetRestoresInitialState()
        {
            var h = TimedCourseLayout.Create()[0];
            var initial = h.PositionAt(0);
            var paused = h.PositionAt(4);
            h.PositionAt(20);
            Assert.That(h.PositionAt(4), Is.EqualTo(paused));
            Assert.That(h.PositionAt(0), Is.EqualTo(initial));
            Assert.That(TimedCourseLayout.Create()[0].PositionAt(4), Is.EqualTo(paused));
        }

        [Test]
        public void AuthoredRoutePassesBothSpheresWithBroadTimingMargins()
        {
            var hazards = TimedCourseLayout.Create();
            float[] distances = { 0, 25, 28, 55, 58, 85, 88, 130 };
            float[] halves = { .65f, .65f, 2.2f, 2.2f, .65f, .65f, 2.2f, 2.2f };
            foreach (var h in hazards)
                for (int i = 1; i < distances.Length; i++)
                    foreach (int side in new[] { -1, 1 })
                        Assert.That(h.HitsSweptSphere(new Vector2(side * halves[i - 1], distances[i - 1]),
                            new Vector2(side * halves[i], distances[i]), .38f,
                            distances[i - 1] / 7f, distances[i] / 7f), Is.False,
                            "Safe authored route failed at hazard " + h.Center.y + " segment " + i);
            Assert.That(hazards[0].HitsSweptSphere(new Vector2(.65f, 44), new Vector2(.65f, 46), .38f, 44f / 7, 46f / 7), Is.True);
            Assert.That(hazards[1].HitsSweptSphere(new Vector2(2.2f, 74), new Vector2(2.2f, 76), .38f, 74f / 7, 76f / 7), Is.True);
        }

        [Test]
        public void AuthoredArmReachesWideSpheresHorizontallyButArrivalPhaseIsSafe()
        {
            var arm = TimedCourseLayout.Create()[2];
            float arrival = 100f / 7f;
            float horizontalTime = arrival - (Mathf.PI * .5f) / arm.AngularSpeed;
            Assert.That(Mathf.Abs(Mathf.Sin(arm.AngleAt(horizontalTime))), Is.LessThan(.00001f));
            Assert.That(arm.AngleAt(arrival), Is.EqualTo(Mathf.PI * .5f).Within(.00001f));
            foreach (int side in new[] { -1, 1 })
            {
                var crossing = new Vector2(side * 2.2f, 100);
                Assert.That(arm.HitsSweptSphere(crossing, crossing, .38f, horizontalTime, horizontalTime), Is.True,
                    "Horizontal arm must reach the wide spheres; staying wide is not safe at every phase.");
                Assert.That(arm.HitsSweptSphere(crossing, crossing, .38f, arrival, arrival), Is.False);
                Assert.That(arm.HitsSweptSphere(new Vector2(side * 2.2f, 95), new Vector2(side * 2.2f, 105),
                    .38f, 95f / 7f, 105f / 7f), Is.False,
                    "The intended approach and exit must both clear the actual rotating geometry.");
            }
        }

        [Test]
        public void InvalidInputsFailExplicitly()
        {
            var h = Wall();
            Assert.Throws<ArgumentOutOfRangeException>(() => h.PositionAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => h.AngleAt(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => h.HitsSweptSphere(Vector2.zero, Vector2.zero, -.1f, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => h.HitsSweptSphere(Vector2.zero, Vector2.zero, .1f, 1, 0));
            Assert.Throws<ArgumentException>(() => h.HitsSweptSphere(new Vector2(float.PositiveInfinity, 0), Vector2.zero, .1f, 0, 1));
            h.HalfExtents = Vector2.zero;
            Assert.Throws<ArgumentException>(() => h.PositionAt(0));
        }
    }
}
