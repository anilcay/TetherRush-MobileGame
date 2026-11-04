using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TetherRush.Core;
using TetherRush.Course;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TetherRush.Tests
{
    public sealed class OptimizationContactTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
        private GameObject root;

        [TearDown]
        public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }

        private TetherRushGame Create()
        {
            root = new GameObject("Optimization contact fixture");
            var game = root.AddComponent<TetherRushGame>();
            game.AutoSimulate = false;
            game.Feedback.AudioEnabled = false;
            return game;
        }

        [Test]
        public void SeededTimedSweepsNeverRejectDetailedContacts()
        {
            var broadphase = typeof(TetherRushGame).GetMethod("TimedOverlapsZ", PrivateStatic);
            var random = new System.Random(98317);
            int contacts = 0, rejected = 0;
            for (int i = 0; i < 144; i++)
            {
                float centerZ = i % 3 == 0 ? 100000f : 20f;
                var hazard = new TimedHazard
                {
                    Kind = i % 2 == 0 ? TimedHazardKind.MovingWall : TimedHazardKind.RotatingArm,
                    Center = new Vector2(0, centerZ), HalfExtents = new Vector2(2.7f, .14f),
                    AngularSpeed = 1.7f, Amplitude = .3f, Phase = (float)(random.NextDouble() * Math.PI * 2)
                };
                var from = new Vector2((float)(random.NextDouble() * 4 - 2), centerZ + (float)(random.NextDouble() * 12 - 6));
                var to = new Vector2((float)(random.NextDouble() * 4 - 2), centerZ + (float)(random.NextDouble() * 12 - 6));
                bool detailed = hazard.HitsSweptSphere(from, to, .3f, 0, .2f);
                bool accepted = (bool)broadphase.Invoke(null, new object[] { hazard, from.y, to.y, .3f });
                if (detailed) { contacts++; Assert.That(accepted, Is.True, "Detailed contact rejected at sample " + i); }
                if (!accepted) rejected++;
            }
            Assert.That(contacts, Is.GreaterThan(10));
            Assert.That(rejected, Is.GreaterThan(10), "Broadphase should also reject genuinely distant sweeps.");
        }

        [Test]
        public void TimedTangentPrecisionAndRotatingTipRemainCandidates()
        {
            var broadphase = typeof(TetherRushGame).GetMethod("TimedOverlapsZ", PrivateStatic);
            var wall = new TimedHazard { Kind = TimedHazardKind.MovingWall, HalfExtents = new Vector2(.5f, .5f) };
            var arm = new TimedHazard { Kind = TimedHazardKind.RotatingArm,
                HalfExtents = new Vector2(2.7f, .14f), Phase = Mathf.PI * .5f };
            foreach (var hazard in new[] { wall, arm })
            {
                float z = hazard == wall ? .75f : 2.9f;
                var point = new Vector2(0, z);
                Assert.That(hazard.HitsSweptSphere(point, point, .25f, 0, 0), Is.True);
                Assert.That((bool)broadphase.Invoke(null, new object[] { hazard, z, z, .25f }), Is.True);
            }
            // This point is within the detailed solver's documented conservative near-contact band.
            float near = .7501f;
            wall.Amplitude = .3f; wall.AngularSpeed = 1;
            Assert.That(wall.HitsSweptSphere(new Vector2(0, near), new Vector2(0, near), .25f, 0, .1f), Is.True);
            Assert.That((bool)broadphase.Invoke(null, new object[] { wall, near, near, .25f }), Is.True);
        }

        [Test]
        public void SeededStaticSweepsNeverRejectDetailedContacts()
        {
            var game = Create();
            var detailed = typeof(TetherRushGame).GetMethod("SweptCircleBox", PrivateInstance);
            var broadphase = typeof(TetherRushGame).GetMethod("OverlapsZ", PrivateStatic);
            var random = new System.Random(7919);
            int contacts = 0, rejected = 0;
            for (int i = 0; i < 96; i++)
            {
                float z = i % 3 == 0 ? 100000f : 20f;
                var box = new Bounds(new Vector3(0, .5f, z), new Vector3(2, 1, 1));
                var from = new Vector2((float)(random.NextDouble() * 4 - 2), z + (float)(random.NextDouble() * 8 - 4));
                var to = new Vector2((float)(random.NextDouble() * 4 - 2), z + (float)(random.NextDouble() * 8 - 4));
                bool hit = (bool)detailed.Invoke(game, new object[] { from, to, box, .25f });
                bool accepted = (bool)broadphase.Invoke(null, new object[] { from.y, to.y, (double)box.min.z, (double)box.max.z, .25d });
                if (hit) { contacts++; Assert.That(accepted, Is.True, "Detailed static contact rejected at sample " + i); }
                if (!accepted) rejected++;
            }
            Assert.That(contacts, Is.GreaterThan(10));
            Assert.That(rejected, Is.GreaterThan(5));
        }

        [Test]
        public void FatalContactInReleaseSubstepStillPreventsPulse()
        {
            var game = Create();
            // Isolate one fatal box at the initial sphere position through the existing private data.
            var hazards = (IList)typeof(TetherRushGame).GetField("hazards", PrivateInstance).GetValue(game);
            hazards.Clear();
            var hazardType = typeof(TetherRushGame).GetNestedType("Hazard", BindingFlags.NonPublic);
            var hazard = Activator.CreateInstance(hazardType, true);
            hazardType.GetField("Bounds").SetValue(hazard, new Bounds(new Vector3(.65f, .55f, 0), Vector3.one));
            hazardType.GetField("Reason").SetValue(hazard, "fatal priority");
            hazards.Add(hazard);
            game.Model.StartRun();
            game.Model.Press();
            game.Model.CollectEnergy(999, game.Model.Tuning.EnergyCapacity);
            game.SimulateStep(1f / 120f, release: true);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
            Assert.That(game.Model.FailureReason, Is.EqualTo("fatal priority"));
            Assert.That(game.Model.PulseCount, Is.Zero);
            Assert.That(game.Model.Energy, Is.EqualTo(game.Model.Tuning.EnergyCapacity));
        }
    }
}
