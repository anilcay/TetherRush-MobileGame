using NUnit.Framework;
using TetherRush.Core;
using UnityEngine;

namespace TetherRush.Tests
{
    /// <summary>Exercises the authored prototype through its production simulation entry point.</summary>
    public sealed class PrototypeFlowTests
    {
        private const float Step = 1f / 60f;
        private const int MaximumSteps = 2400;
        private GameObject root;
        private TetherRushGame game;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("PrototypeFlowTests");
            game = root.AddComponent<TetherRushGame>();
            game.AutoSimulate = false;
            AssertPhase(RunPhase.Ready);
            game.StartRun();
            AssertPhase(RunPhase.Running);
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null)
                Object.DestroyImmediate(root);
        }

        [Test]
        public void AuthoredRouteCollectsEnergyAndBonusActivatesBothTargetsAndCompletes()
        {
            ReachFirstCharge();
            Assert.That(game.Model.Energy, Is.EqualTo(5));
            Assert.That(game.Model.Bonus, Is.GreaterThan(0), "Wide spheres should collect authored bonus crystals.");

            AdvanceTo(96f);
            Release();
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.Model.Energy, Is.Zero);
            Assert.That(game.ActivatedTargetCount, Is.EqualTo(1));

            AdvanceTo(105f);
            Press();
            AdvanceTo(149f);
            Assert.That(game.Model.Energy, Is.EqualTo(5));
            Release();
            Assert.That(game.ActivatedTargetCount, Is.EqualTo(2));
            AdvanceUntilTerminal();

            AssertPhase(RunPhase.Completed);
            Assert.That(game.Model.PulseCount, Is.EqualTo(2));
            Assert.That(game.RemainingEnergyCount, Is.Zero);
            Assert.That(game.Model.Energy, Is.Zero);
        }

        [Test]
        public void StayingNarrowFailsAtCenterBarrier()
        {
            AdvanceUntilTerminal();

            AssertPhase(RunPhase.Failed);
            Assert.That(game.Model.Distance, Is.InRange(39f, 45f));
            Assert.That(game.Model.FailureReason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void HalfSecondFrameCannotSkipCenterBarrierWhileNarrow()
        {
            AdvanceTo(40f);
            float before = game.Model.Distance;
            Assert.That(game.Model.IsHeld, Is.False);

            // The uninterrupted endpoint would be beyond the entire barrier and sphere radius.
            game.SimulateStep(.5f);

            AssertPhase(RunPhase.Failed);
            Assert.That(game.Model.Distance, Is.InRange(40f, 42f));
            Assert.That(game.Model.Distance, Is.LessThan(before + game.Model.Tuning.ForwardSpeed * .5f));
            Assert.That(game.Model.FailureReason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void HalfSecondReleaseFramesOpenBothTargetsAndBeginNarrowingWithoutFatalDelay()
        {
            ReachFirstCharge();
            AdvanceTo(96f);
            float firstSeparation = game.Model.HalfSeparation;

            game.SimulateStep(.5f, release: true);

            AssertPhase(RunPhase.Running);
            Assert.That(game.ActivatedTargetCount, Is.EqualTo(1));
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.Model.Energy, Is.Zero);
            Assert.That(game.Model.IsHeld, Is.False);
            Assert.That(game.Model.HalfSeparation, Is.LessThan(firstSeparation),
                "Release should start narrowing within the same coarse frame.");

            AdvanceTo(105f);
            Press();
            AdvanceTo(149f);
            Assert.That(game.Model.Energy, Is.EqualTo(5));
            float secondSeparation = game.Model.HalfSeparation;

            game.SimulateStep(.5f, release: true);

            AssertPhase(RunPhase.Running);
            Assert.That(game.ActivatedTargetCount, Is.EqualTo(2));
            Assert.That(game.Model.PulseCount, Is.EqualTo(2));
            Assert.That(game.Model.Energy, Is.Zero);
            Assert.That(game.Model.IsHeld, Is.False);
            Assert.That(game.Model.HalfSeparation, Is.LessThan(secondSeparation));
            AdvanceUntilTerminal();
            AssertPhase(RunPhase.Completed);
            Assert.That(game.Model.PulseCount, Is.EqualTo(2));
        }

        [Test]
        public void StayingWideFailsAtFirstNarrowGate()
        {
            Press();
            AdvanceUntilTerminal();

            AssertPhase(RunPhase.Failed);
            Assert.That(game.Model.Distance, Is.InRange(17f, 23f));
        }

        [Test]
        public void EarlyFullPulseSpendsChargeAndClosedGateEndsRun()
        {
            ReachFirstCharge();
            AdvanceTo(90f);
            Release();
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.Model.Energy, Is.Zero);
            Assert.That(game.ActivatedTargetCount, Is.Zero, "Gate must remain closed outside Pulse range.");
            AdvanceUntilTerminal();

            AssertPhase(RunPhase.Failed);
            Assert.That(game.Model.Distance, Is.InRange(97f, 103f));
        }

        [Test]
        public void FullPulseDoesNotDestroyNormalCenterHazard()
        {
            AdvanceTo(39f);
            // Isolate the normal-hazard/Pulse distinction before authored energy exists.
            // All interaction and collision still run through the production path.
            for (int i = 0; i < 5; i++)
                game.Model.CollectEnergy(10000 + i);
            Press();
            Release();
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.Model.Energy, Is.Zero);
            Assert.That(game.ActivatedTargetCount, Is.Zero);
            AdvanceUntilTerminal();

            AssertPhase(RunPhase.Failed);
            Assert.That(game.Model.Distance, Is.InRange(39f, 45f));
        }

        [Test]
        public void RestartRestoresWorldChargeBonusInputAndTargets()
        {
            ReachFirstCharge();
            AdvanceTo(96f);
            Release();
            Assert.That(game.ActivatedTargetCount, Is.EqualTo(1));
            Assert.That(game.Model.Bonus, Is.GreaterThan(0));
            Press();

            game.RestartRun();

            AssertPhase(RunPhase.Running);
            Assert.That(game.Model.Distance, Is.Zero);
            Assert.That(game.Model.Energy, Is.Zero);
            Assert.That(game.Model.Bonus, Is.Zero);
            Assert.That(game.Model.PulseCount, Is.Zero);
            Assert.That(game.Model.IsHeld, Is.False);
            Assert.That(game.ActivatedTargetCount, Is.Zero);
            Assert.That(game.RemainingEnergyCount, Is.EqualTo(10));
            Assert.That(game.LeftCore.position.x, Is.EqualTo(-game.Model.HalfSeparation).Within(0.001f));
            Assert.That(game.RightCore.position.x, Is.EqualTo(game.Model.HalfSeparation).Within(0.001f));

            // Recollecting verifies the pickups were restored, including consumed IDs.
            ReachFirstCharge();
            Assert.That(game.Model.Energy, Is.EqualTo(5));
            Assert.That(game.Model.Bonus, Is.GreaterThan(0));
            Assert.That(game.RemainingEnergyCount, Is.EqualTo(5));
        }

        [Test]
        public void PauseFreezesRunAndResumeRequiresFreshPressBeforePulse()
        {
            ReachFirstCharge();
            float distance = game.Model.Distance;
            float separation = game.Model.HalfSeparation;
            int remainingEnergy = game.RemainingEnergyCount;
            game.PauseRun();
            AssertPhase(RunPhase.Paused);
            Assert.That(game.Model.IsHeld, Is.False);
            for (int i = 0; i < 60; i++)
                game.SimulateStep(Step);
            Assert.That(game.Model.Distance, Is.EqualTo(distance));
            Assert.That(game.Model.HalfSeparation, Is.EqualTo(separation));
            Assert.That(game.RemainingEnergyCount, Is.EqualTo(remainingEnergy));

            game.ResumeRun();
            Release();
            AssertPhase(RunPhase.Running);
            Assert.That(game.Model.PulseCount, Is.Zero, "Release after cancellation must not synthesize a Pulse.");
            Assert.That(game.Model.Energy, Is.EqualTo(5));
            Press();
            Release();
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.Model.Energy, Is.Zero);
        }

        [Test]
        public void WideTetherCrossesNormalCenterBarrierWithoutFatalContact()
        {
            AdvanceTo(30f);
            Press();
            AdvanceTo(48f);

            AssertPhase(RunPhase.Running);
            Assert.That(game.LeftCore.position.x, Is.LessThan(-1.5f));
            Assert.That(game.RightCore.position.x, Is.GreaterThan(1.5f));
            Assert.That((game.LeftCore.position.x + game.RightCore.position.x) * 0.5f,
                Is.EqualTo(0f).Within(0.001f));
            Assert.That(game.Model.PulseCount, Is.Zero);
        }

        [Test]
        public void ReleasingWithAuthoredPartialChargeDoesNotPulseOrSpendEnergy()
        {
            AdvanceTo(30f);
            Press();
            AdvanceTo(62f);
            int partialCharge = game.Model.Energy;
            Assert.That(partialCharge, Is.InRange(1, 4));

            Release();

            AssertPhase(RunPhase.Running);
            Assert.That(game.Model.PulseCount, Is.Zero);
            Assert.That(game.Model.Energy, Is.EqualTo(partialCharge));
            Assert.That(game.Model.IsHeld, Is.False);
            Assert.That(game.ActivatedTargetCount, Is.Zero);
        }

        private void ReachFirstCharge()
        {
            AdvanceTo(30f);
            Press();
            AdvanceTo(86f);
            Assert.That(game.Model.Energy, Is.EqualTo(5), "Authored pickups should fill the wide tether.");
        }

        private void Press() => game.SimulateStep(Step, press: true);
        private void Release() => game.SimulateStep(Step, release: true);

        private void AdvanceTo(float distance)
        {
            for (int i = 0; i < MaximumSteps && game.Model.Distance < distance; i++)
            {
                AssertPhase(RunPhase.Running);
                game.SimulateStep(Step);
            }
            AssertPhase(RunPhase.Running);
            Assert.That(game.Model.Distance, Is.GreaterThanOrEqualTo(distance), "Bounded simulation did not reach the requested distance.");
        }

        private void AdvanceUntilTerminal()
        {
            for (int i = 0; i < MaximumSteps && game.Model.Phase == RunPhase.Running; i++)
                game.SimulateStep(Step);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed).Or.EqualTo(RunPhase.Completed),
                "Run must reach a terminal state within the bounded simulation.");
        }

        private void AssertPhase(RunPhase expected)
        {
            Assert.That(game.Model.Phase, Is.EqualTo(expected),
                $"Unexpected phase at distance {game.Model.Distance}: {game.Model.FailureReason}");
        }
    }
}
