using NUnit.Framework;
using TetherRush.Core;
using UnityEngine;

namespace TetherRush.Tests
{
    /// <summary>Exercises M2 through the production game, including moving views and fatal contact.</summary>
    public sealed class ChallengeFlowTests
    {
        private const float Step = 1f / 60f;
        private GameObject root;
        private TetherRushGame game;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("ChallengeFlowTests");
            root.SetActive(false);
            game = root.AddComponent<TetherRushGame>();
            game.ConfigureCourse(true);
            root.SetActive(true);
            game.AutoSimulate = false;
            AssertPhase(RunPhase.Ready);
            game.StartRun();
            AssertPhase(RunPhase.Running);
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
        }

        [Test]
        public void AuthoredChallengeCompletesWithBothSphereBonusRows()
        {
            Assert.That(game.IsAdvancedCourse, Is.True);
            Assert.That(game.TimedHazardCount, Is.EqualTo(3));
            Assert.That(game.CourseFinishDistance, Is.EqualTo(130));
            AdvanceTo(25);
            game.SimulateStep(Step, press: true);
            AdvanceTo(55);
            game.SimulateStep(Step, release: true);
            AdvanceTo(85);
            game.SimulateStep(Step, press: true);
            AdvanceUntilTerminal();

            AssertPhase(RunPhase.Completed);
            Assert.That(game.Model.Bonus, Is.EqualTo(6));
            Assert.That(game.Model.Distance, Is.GreaterThanOrEqualTo(game.CourseFinishDistance));
            Assert.That(game.Model.PulseCount, Is.Zero);
            Assert.That(game.Model.Energy, Is.Zero);
        }

        [Test]
        public void StayingNarrowFailsAtFirstMovingWall()
        {
            AdvanceUntilTerminal();
            AssertPhase(RunPhase.Failed);
            Assert.That(game.Model.Distance, Is.InRange(43f, 46f));
            Assert.That(game.Model.FailureReason, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void HoldingPastFirstWallFailsAtSecondMovingWall()
        {
            AdvanceTo(25);
            game.SimulateStep(Step, press: true);
            AdvanceTo(60);
            Assert.That(game.Model.Bonus, Is.EqualTo(2));
            AdvanceUntilTerminal();
            AssertPhase(RunPhase.Failed);
            Assert.That(game.Model.Distance, Is.InRange(73f, 76f));
        }

        [Test]
        public void PauseFreezesHazardPoseAndRestartRestoresInitialPoseAndRun()
        {
            Transform wall = FindHazard("Timed hazard 0");
            Transform arm = FindHazard("Timed hazard 2");
            Vector3 initialWall = wall.position;
            Quaternion initialArm = arm.rotation;
            AdvanceTo(25);
            game.SimulateStep(Step, press: true);
            AdvanceTo(53);
            Assert.That(game.Model.Bonus, Is.EqualTo(2));
            Assert.That(Vector3.Distance(wall.position, initialWall), Is.GreaterThan(.01f));
            float pausedDistance = game.Model.Distance;
            Vector3 pausedWall = wall.position;
            Quaternion pausedArm = arm.rotation;

            game.PauseRun();
            AssertPhase(RunPhase.Paused);
            game.SimulateStep(2);
            Assert.That(game.Model.Distance, Is.EqualTo(pausedDistance));
            Assert.That(wall.position, Is.EqualTo(pausedWall));
            Assert.That(arm.rotation, Is.EqualTo(pausedArm));
            game.ResumeRun();
            AssertPhase(RunPhase.Running);
            Assert.That(game.Model.IsHeld, Is.False);
            game.SimulateStep(Step, press: true);
            Assert.That(game.Model.Distance, Is.GreaterThan(pausedDistance));

            game.RestartRun();
            AssertPhase(RunPhase.Running);
            Assert.That(game.Model.Distance, Is.Zero);
            Assert.That(game.Model.Bonus, Is.Zero);
            Assert.That(game.Model.Energy, Is.Zero);
            Assert.That(game.Model.PulseCount, Is.Zero);
            Assert.That(game.Model.IsHeld, Is.False);
            Assert.That(Vector3.Distance(wall.position, initialWall), Is.LessThan(.0001f));
            Assert.That(Quaternion.Angle(arm.rotation, initialArm), Is.LessThan(.001f));
            Assert.That(game.RightCore.position.x, Is.EqualTo(game.Model.Tuning.MinHalfSeparation).Within(.001f));
            Assert.That(game.LeftCore.position.x, Is.EqualTo(-game.Model.Tuning.MinHalfSeparation).Within(.001f));
        }

        [Test]
        public void HalfSecondFrameCannotSkipMovingWallWhileNarrow()
        {
            AdvanceTo(43);
            float uninterruptedEnd = game.Model.Distance + game.Model.Tuning.ForwardSpeed * .5f;
            game.SimulateStep(.5f);
            AssertPhase(RunPhase.Failed);
            Assert.That(game.Model.Distance, Is.InRange(43f, 45f));
            Assert.That(game.Model.Distance, Is.LessThan(uninterruptedEnd));
            Assert.That(game.Model.FailureReason, Is.Not.Null.And.Not.Empty);
        }

        private Transform FindHazard(string name)
        {
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == name) return item;
            Assert.Fail("Expected authored hazard view: " + name);
            return null;
        }

        private void AdvanceTo(float distance)
        {
            for (int i = 0; i < 2400 && game.Model.Distance < distance; i++)
            {
                AssertPhase(RunPhase.Running);
                game.SimulateStep(Step);
            }
            AssertPhase(RunPhase.Running);
            Assert.That(game.Model.Distance, Is.GreaterThanOrEqualTo(distance));
        }

        private void AdvanceUntilTerminal()
        {
            for (int i = 0; i < 2400 && game.Model.Phase == RunPhase.Running; i++)
                game.SimulateStep(Step);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed).Or.EqualTo(RunPhase.Completed));
        }

        private void AssertPhase(RunPhase expected)
        {
            Assert.That(game.Model.Phase, Is.EqualTo(expected),
                $"Unexpected challenge phase at {game.Model.Distance}: {game.Model.FailureReason}");
        }
    }
}
