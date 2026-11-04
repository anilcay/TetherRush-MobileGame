using System;
using NUnit.Framework;
using TetherRush.Core;

namespace TetherRush.Tests
{
    public sealed class EndlessPaceTests
    {
        [Test]
        public void ScoreCurveHasGraceAndSmoothBoundedMonotonicGrowth()
        {
            Assert.That(EndlessPace.TargetSpeedForScore(0), Is.EqualTo(7f));
            Assert.That(EndlessPace.TargetSpeedForScore(500), Is.EqualTo(7f));
            Assert.That(EndlessPace.TargetSpeedForScore(3000), Is.EqualTo(9.25f));
            float previous = 7f;
            for (int score = 0; score <= 10000; score++)
            {
                float target = EndlessPace.TargetSpeedForScore(score);
                Assert.That(target, Is.InRange(previous, 11.5f));
                previous = target;
            }
            Assert.That(EndlessPace.TargetSpeedForScore(int.MaxValue), Is.EqualTo(11.5f));
        }

        [Test]
        public void LargeBonusJumpIsSlewLimitedAndNeverOvershootsCap()
        {
            var pace = new EndlessPace();
            Assert.That(pace.Advance(0f, 5500), Is.EqualTo(7f));
            Assert.That(pace.Advance(.1f, 5500), Is.EqualTo(7.025f).Within(.000001f));
            Assert.That(pace.Advance(100f, int.MaxValue), Is.EqualTo(11.5f));
            Assert.That(pace.Advance(100f, int.MaxValue), Is.EqualTo(11.5f));
        }

        [Test]
        public void HundredPointBonusCannotSnapSpeedToNewTarget()
        {
            var pace = new EndlessPace();
            pace.Advance(100f, 3000);
            float before = pace.CurrentSpeed;
            float target = EndlessPace.TargetSpeedForScore(3100);
            float after = pace.Advance(1f / 120f, 3100);
            Assert.That(after, Is.GreaterThan(before));
            Assert.That(after - before, Is.LessThanOrEqualTo(.25f / 120f + .000001f));
            Assert.That(after, Is.LessThan(target));
        }

        [Test]
        public void LowerScoreDoesNotDecelerateAndResetRestoresGrace()
        {
            var pace = new EndlessPace();
            pace.Advance(4f, 5500);
            Assert.That(pace.Advance(10f, 0), Is.EqualTo(8f));
            pace.Reset();
            Assert.That(pace.CurrentSpeed, Is.EqualTo(7f));
            Assert.That(pace.Advance(100f, 500), Is.EqualTo(7f));
        }

        [Test]
        public void FixedTargetPaceIsIndependentOfSubstepCount()
        {
            var single = new EndlessPace();
            var substeps = new EndlessPace();
            single.Advance(8f, 5500);
            for (int i = 0; i < 960; i++) substeps.Advance(1f / 120f, 5500);
            Assert.That(substeps.CurrentSpeed, Is.EqualTo(single.CurrentSpeed).Within(.000001f));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void PaceRejectsInvalidDeltaWithoutMutation(float dt)
        {
            var pace = new EndlessPace();
            Assert.Throws<ArgumentOutOfRangeException>(() => pace.Advance(dt, 5500));
            Assert.That(pace.CurrentSpeed, Is.EqualTo(7f));
        }

        [Test]
        public void NegativeScoreIsRejectedWithoutMutation()
        {
            var pace = new EndlessPace();
            Assert.Throws<ArgumentOutOfRangeException>(() => EndlessPace.TargetSpeedForScore(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => pace.Advance(1f, -1));
            Assert.That(pace.CurrentSpeed, Is.EqualTo(7f));
        }

        [Test]
        public void LegacyTickKeepsSevenMetresPerSecondAndElapsedClock()
        {
            var model = new RunModel(new RunTuning());
            model.StartRun();
            model.Tick(.25f);
            model.Tick(.75f);
            Assert.That(model.Distance, Is.EqualTo(7f));
            Assert.That(model.ActualElapsedTime, Is.EqualTo(1f));
        }

        [Test]
        public void ExplicitSpeedOnlyChangesDistanceAndNotSeparationOrTuning()
        {
            var model = new RunModel(new RunTuning());
            model.StartRun();
            model.Press();
            model.Tick(.1f, 11.5f);
            Assert.That(model.Distance, Is.EqualTo(1.15f).Within(.000001f));
            Assert.That(model.HalfSeparation, Is.EqualTo(1.25f).Within(.000001f));
            Assert.That(model.ActualElapsedTime, Is.EqualTo(.1f));
            Assert.That(model.Tuning.ForwardSpeed, Is.EqualTo(7f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void ExplicitSpeedRejectsInvalidValuesEvenWhenReady(float speed)
        {
            var model = new RunModel(new RunTuning());
            Assert.Throws<ArgumentOutOfRangeException>(() => model.Tick(1f, speed));
            model.StartRun();
            Assert.Throws<ArgumentOutOfRangeException>(() => model.Tick(1f, speed));
            Assert.That(model.Distance, Is.Zero);
            Assert.That(model.ActualElapsedTime, Is.Zero);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void ExplicitSpeedTickRejectsInvalidDelta(float dt)
        {
            var model = new RunModel(new RunTuning());
            Assert.Throws<ArgumentOutOfRangeException>(() => model.Tick(dt, 11.5f));
            model.StartRun();
            Assert.Throws<ArgumentOutOfRangeException>(() => model.Tick(dt, 11.5f));
            Assert.That(model.ActualElapsedTime, Is.Zero);
        }

        [Test]
        public void PausedIntegrationFreezesPaceAndActualTimeAndRetryResetsBoth()
        {
            var pace = new EndlessPace();
            var model = new RunModel(new RunTuning());
            model.StartRun();
            model.Tick(2f, pace.Advance(2f, 5500));
            float speed = pace.CurrentSpeed;
            float distance = model.Distance;
            model.Pause();
            // Runtime owns this guard: pause must not feed time into the pace component.
            if (model.Phase == RunPhase.Running) pace.Advance(10f, 5500);
            model.Tick(10f, pace.CurrentSpeed);
            Assert.That(model.Distance, Is.EqualTo(distance));
            Assert.That(model.ActualElapsedTime, Is.EqualTo(2f));
            Assert.That(pace.CurrentSpeed, Is.EqualTo(speed));
            model.Resume();
            model.Tick(1f, pace.Advance(1f, 5500));
            Assert.That(model.ActualElapsedTime, Is.EqualTo(3f));
            model.StartRun();
            pace.Reset();
            Assert.That(model.ActualElapsedTime, Is.Zero);
            Assert.That(model.Distance, Is.Zero);
            Assert.That(pace.CurrentSpeed, Is.EqualTo(7f));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TerminalStatesFreezeActualTime(bool complete)
        {
            var model = new RunModel(new RunTuning());
            model.StartRun();
            model.Tick(1f, 9f);
            if (complete) model.Complete(); else model.Fail("collision");
            model.Tick(10f, 11.5f);
            Assert.That(model.ActualElapsedTime, Is.EqualTo(1f));
            Assert.That(model.Distance, Is.EqualTo(9f));
            model.Reset();
            Assert.That(model.ActualElapsedTime, Is.Zero);
        }
    }
}
