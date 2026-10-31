using System;
using NUnit.Framework;
using TetherRush.Core;

namespace TetherRush.Tests
{
    public sealed class RunModelTests
    {
        private RunModel model;

        [SetUp]
        public void SetUp()
        {
            model = new RunModel(new RunTuning());
        }

        private void StartFull()
        {
            model.StartRun();
            model.CollectEnergy(1, model.Tuning.EnergyCapacity);
        }

        [Test]
        public void ConstructorIsReadyNarrowAndEmpty()
        {
            Assert.That(model.Phase, Is.EqualTo(RunPhase.Ready));
            Assert.That(model.HalfSeparation, Is.EqualTo(model.Tuning.MinHalfSeparation));
            Assert.That(model.Distance, Is.Zero);
            Assert.That(model.Energy, Is.Zero);
            Assert.That(model.Bonus, Is.Zero);
            Assert.That(model.PulseCount, Is.Zero);
            Assert.That(model.IsHeld, Is.False);
        }

        [Test]
        public void FullEnergyRealReleasePulsesExactlyOnce()
        {
            StartFull();
            Assert.That(model.Release(), Is.False);
            model.Press();
            model.Press();
            Assert.That(model.Release(), Is.True);
            Assert.That(model.Energy, Is.Zero);
            Assert.That(model.IsHeld, Is.False);
            Assert.That(model.Release(), Is.False);
            Assert.That(model.PulseCount, Is.EqualTo(1));
        }

        [Test]
        public void PartialReleaseKeepsEnergyAndNarrowsWithoutPulse()
        {
            model.StartRun();
            model.CollectEnergy(1, 2);
            model.Press();
            model.Tick(.2f);
            float open = model.HalfSeparation;
            Assert.That(model.Release(), Is.False);
            model.Tick(.1f);
            Assert.That(model.HalfSeparation, Is.LessThan(open));
            Assert.That(model.Energy, Is.EqualTo(2));
            Assert.That(model.PulseCount, Is.Zero);
        }

        [Test]
        public void CollectionIdsAreConsumedOnceAndHaveIndependentKinds()
        {
            model.StartRun();
            Assert.That(model.CollectEnergy(42, 2), Is.True);
            Assert.That(model.CollectEnergy(42, 2), Is.False);
            Assert.That(model.CollectBonus(42, 3), Is.True);
            Assert.That(model.CollectBonus(42, 3), Is.False);
            Assert.That(model.Energy, Is.EqualTo(2));
            Assert.That(model.Bonus, Is.EqualTo(3));
        }

        [Test]
        public void EnergyClampsAndFullPickupStaysConsumedAfterPulse()
        {
            StartFull();
            Assert.That(model.CollectEnergy(2, int.MaxValue), Is.True);
            Assert.That(model.Energy, Is.EqualTo(model.Tuning.EnergyCapacity));
            model.Press();
            model.Release();
            Assert.That(model.CollectEnergy(2), Is.False);
            Assert.That(model.Energy, Is.Zero);
        }

        [Test]
        public void CancelInputDoesNotPulseOrConsumeFullEnergy()
        {
            StartFull();
            model.Press();
            model.CancelInput();
            Assert.That(model.Release(), Is.False);
            Assert.That(model.IsHeld, Is.False);
            Assert.That(model.Energy, Is.EqualTo(model.Tuning.EnergyCapacity));
            Assert.That(model.PulseCount, Is.Zero);
        }

        [Test]
        public void PauseFreezesAndResumeRequiresFreshPress()
        {
            StartFull();
            model.Press();
            model.Tick(.1f);
            float distance = model.Distance;
            float separation = model.HalfSeparation;
            model.Pause();
            model.Press();
            model.Tick(1f);
            Assert.That(model.Phase, Is.EqualTo(RunPhase.Paused));
            Assert.That(model.Distance, Is.EqualTo(distance));
            Assert.That(model.HalfSeparation, Is.EqualTo(separation));
            Assert.That(model.CollectEnergy(99), Is.False);
            Assert.That(model.CollectBonus(99), Is.False);
            model.Resume();
            Assert.That(model.Release(), Is.False);
            Assert.That(model.Energy, Is.EqualTo(model.Tuning.EnergyCapacity));
            model.Press();
            Assert.That(model.Release(), Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TerminalStatesFreezeAndCannotBeOverwritten(bool complete)
        {
            StartFull();
            model.Press();
            model.Tick(.2f);
            if (complete) model.Complete(); else model.Fail("sphere collision");
            RunPhase terminal = model.Phase;
            float distance = model.Distance;
            float separation = model.HalfSeparation;
            model.Fail("later collision");
            model.Complete();
            model.Pause();
            model.Resume();
            model.Press();
            model.Tick(10f);
            Assert.That(model.Release(), Is.False);
            Assert.That(model.CollectEnergy(77), Is.False);
            Assert.That(model.CollectBonus(77), Is.False);
            Assert.That(model.Phase, Is.EqualTo(terminal));
            Assert.That(model.Distance, Is.EqualTo(distance));
            Assert.That(model.HalfSeparation, Is.EqualTo(separation));
            Assert.That(model.Energy, Is.EqualTo(model.Tuning.EnergyCapacity));
            Assert.That(model.IsHeld, Is.False);
            Assert.That(model.FailureReason, Is.EqualTo(complete ? string.Empty : "sphere collision"));
        }

        [Test]
        public void ResetClearsCountersIdsAndStaleInput()
        {
            StartFull();
            model.CollectBonus(1, 4);
            model.Press();
            model.Release();
            model.Press();
            model.Tick(.3f);
            model.Fail("collision");
            model.Reset();
            Assert.That(model.Phase, Is.EqualTo(RunPhase.Ready));
            Assert.That(model.Distance, Is.Zero);
            Assert.That(model.Energy, Is.Zero);
            Assert.That(model.Bonus, Is.Zero);
            Assert.That(model.PulseCount, Is.Zero);
            Assert.That(model.FailureReason, Is.Empty);
            Assert.That(model.IsHeld, Is.False);
            Assert.That(model.HalfSeparation, Is.EqualTo(model.Tuning.MinHalfSeparation));
            model.StartRun();
            Assert.That(model.Release(), Is.False);
            Assert.That(model.CollectEnergy(1), Is.True);
            Assert.That(model.CollectBonus(1), Is.True);
        }

        [Test]
        public void StartingAgainResetsAnActiveRun()
        {
            StartFull();
            model.Press();
            model.Tick(1f);
            model.StartRun();
            Assert.That(model.Phase, Is.EqualTo(RunPhase.Running));
            Assert.That(model.Distance, Is.Zero);
            Assert.That(model.Energy, Is.Zero);
            Assert.That(model.IsHeld, Is.False);
            Assert.That(model.Release(), Is.False);
        }

        [Test]
        public void ForwardSpeedIsConstantAndSeparationMovesLinearlyWithinLimits()
        {
            model.StartRun();
            model.Press();
            model.Tick(.1f);
            Assert.That(model.Distance, Is.EqualTo(.7f).Within(.00001f));
            Assert.That(model.HalfSeparation, Is.EqualTo(1.25f).Within(.00001f));
            model.Tick(10f);
            Assert.That(model.HalfSeparation, Is.EqualTo(model.Tuning.MaxHalfSeparation));
            model.Release();
            model.Tick(10f);
            Assert.That(model.HalfSeparation, Is.EqualTo(model.Tuning.MinHalfSeparation));
            Assert.That(model.Distance, Is.EqualTo(140.7f).Within(.0001f));
        }

        [Test]
        public void HalfSeparationDefinesEqualOppositeOffsetsAroundFixedCenter()
        {
            model.StartRun();
            model.Press();
            model.Tick(.15f);
            float left = -model.HalfSeparation;
            float right = model.HalfSeparation;
            Assert.That((left + right) / 2f, Is.Zero);
            Assert.That(right - left, Is.EqualTo(2f * model.HalfSeparation));
            Assert.That(model.HalfSeparation, Is.EqualTo(1.55f).Within(.00001f));
        }

        [Test]
        public void ReadyIgnoresGameplayAndZeroDeltaIsValid()
        {
            model.Press();
            model.Tick(1f);
            model.Pause();
            model.Resume();
            model.Fail("ignored");
            model.Complete();
            Assert.That(model.Release(), Is.False);
            Assert.That(model.CollectEnergy(1), Is.False);
            Assert.That(model.CollectBonus(1), Is.False);
            Assert.That(model.Phase, Is.EqualTo(RunPhase.Ready));
            Assert.That(model.Distance, Is.Zero);
            model.StartRun();
            model.Tick(0f);
            Assert.That(model.Distance, Is.Zero);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void InvalidDeltaIsRejectedEvenOutsideRunning(float dt)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => model.Tick(dt));
            model.StartRun();
            Assert.Throws<ArgumentOutOfRangeException>(() => model.Tick(dt));
            Assert.That(model.Distance, Is.Zero);
        }

        [Test]
        public void NullTuningIsRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new RunModel(null));
        }

        [TestCase("ForwardSpeed")]
        [TestCase("MinHalfSeparation")]
        [TestCase("MaxHalfSeparation")]
        [TestCase("SeparationSpeed")]
        [TestCase("SphereRadius")]
        [TestCase("PulseRadius")]
        public void AllFloatTuningMustBeFiniteAndPositive(string fieldName)
        {
            foreach (float invalid in new[] { 0f, -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                var tuning = new RunTuning();
                typeof(RunTuning).GetField(fieldName).SetValue(tuning, invalid);
                Assert.Throws<ArgumentOutOfRangeException>(() => new RunModel(tuning), fieldName);
            }
        }

        [Test]
        public void ReversedSeparationAndNonpositiveCapacityAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new RunModel(new RunTuning
                { MinHalfSeparation = 3f, MaxHalfSeparation = 2f }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RunModel(new RunTuning { EnergyCapacity = 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RunModel(new RunTuning { EnergyCapacity = -1 }));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void CollectionAmountsMustBePositive(int amount)
        {
            model.StartRun();
            Assert.Throws<ArgumentOutOfRangeException>(() => model.CollectEnergy(1, amount));
            Assert.Throws<ArgumentOutOfRangeException>(() => model.CollectBonus(1, amount));
            Assert.That(model.CollectEnergy(1), Is.True);
            Assert.That(model.CollectBonus(1), Is.True);
        }

        [Test]
        public void LargeBonusAmountsDoNotWrapNegative()
        {
            model.StartRun();
            model.CollectBonus(1, int.MaxValue);
            model.CollectBonus(2);
            Assert.That(model.Bonus, Is.EqualTo(int.MaxValue));
        }
    }
}
