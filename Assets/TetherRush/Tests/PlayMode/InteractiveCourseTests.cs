using System;
using System.Collections.Generic;
using NUnit.Framework;
using TetherRush.Art;
using TetherRush.Core;
using TetherRush.Course;
using UnityEngine;

namespace TetherRush.Tests
{
    public sealed class InteractiveCourseTests
    {
        private static IEnumerable<CourseDefinition> Courses()
        {
            for (int i = 0; i < LevelCatalog.Count; i++) yield return LevelCatalog.Get(i);
            foreach (int seed in new[] { 0, 1, -17, int.MaxValue })
                for (int i = 0; i < 25; i++) yield return EndlessCourse.CreateChunk(i, seed);
        }

        [Test]
        public void IntroductionsKeepFiveLegacyLevelsAndTeachRelayBeforePulseLock()
        {
            Assert.That(LevelCatalog.Count, Is.EqualTo(25));
            for (int i = 0; i < 5; i++)
            {
                var c = LevelCatalog.Get(i);
                Assert.That(c.Relays, Is.Empty);
                Assert.That(Array.Exists(c.TimedHazards, h => h.PulseLock), Is.False);
            }
            Assert.That(LevelCatalog.Get(5).Relays.Length, Is.EqualTo(1));
            Assert.That(LevelCatalog.Get(5).UsesEnergy, Is.False);
            Assert.That(Array.Exists(LevelCatalog.Get(7).TimedHazards, h => h.PulseLock), Is.True);
        }

        [Test]
        public void RelayGatePairsAreLocalUnmarkedAndAllowAmpleTimeAfterWidePadCrossing()
        {
            foreach (var c in Courses())
            {
                var ids = new HashSet<int>();
                foreach (var r in c.Relays)
                {
                    Assert.That(ids.Add(r.Id), Is.True, c.Id);
                    Assert.That(r.RequiredHalfSpan, Is.EqualTo(1.5f));
                    var gates = Array.FindAll(c.Hazards, h => h.RelayId == r.Id);
                    Assert.That(gates.Length, Is.EqualTo(1), c.Id);
                    var gate = gates[0];
                    Assert.That(gate.Marked, Is.False);
                    Assert.That(gate.Gate, Is.True);
                    Assert.That(gate.Visual, Is.EqualTo(VisualKind.EnergyGate));
                    Assert.That(gate.Size, Is.EqualTo(new Vector3(7.2f, 1.3f, .7f)));
                    Assert.That(gate.Position.z - r.Position.z, Is.InRange(12, 18));
                    int cue = Array.FindLastIndex(c.SafeRoute, q => q.Distance <= r.Position.z);
                    Assert.That(cue, Is.GreaterThanOrEqualTo(0), c.Id);
                    Assert.That(c.SafeRoute[cue].Press, Is.True, c.Id);
                    // At the 11.5 m/s cap, a full spread takes under three metres.
                    Assert.That(r.Position.z - c.SafeRoute[cue].Distance, Is.GreaterThan(3.5f));
                    Assert.That(Array.Exists(c.SafeRoute, q => !q.Press &&
                        q.Distance > r.Position.z && q.Distance < gate.Position.z + 1), Is.False);
                }
                foreach (var gate in c.Hazards)
                    if (gate.RelayId >= 0) Assert.That(ids.Contains(gate.RelayId), Is.True);
            }
        }

        [Test]
        public void EveryMovingPulseLockHasItsOwnFiveEnergyAndOneInRangeRealRelease()
        {
            foreach (var c in Courses())
            foreach (var h in c.TimedHazards)
            {
                if (!h.PulseLock) continue;
                Assert.That(c.UsesEnergy, Is.True);
                int releaseIndex = Array.FindIndex(c.SafeRoute,
                    q => !q.Press && Mathf.Abs(q.Distance - (h.Center.y - 4)) < .001f);
                Assert.That(releaseIndex, Is.GreaterThan(0), c.Id);
                var press = c.SafeRoute[releaseIndex - 1];
                Assert.That(press.Press, Is.True);
                float release = c.SafeRoute[releaseIndex].Distance;
                var charge = Array.FindAll(c.Pickups, p => !p.Bonus &&
                    p.Position.z > press.Distance + 3.5f && p.Position.z < release - 3);
                Assert.That(charge.Length, Is.EqualTo(5), c.Id);
                foreach (var p in charge) Assert.That(Mathf.Abs(p.Position.x), Is.LessThan(2.2f));
                // Center is within the OBB's lateral span in every motion phase.
                Assert.That(h.HalfExtents.x - h.Amplitude, Is.GreaterThan(2.2f + .38f));
                Assert.That(4 - h.HalfExtents.y, Is.LessThan(new RunTuning().PulseRadius));
            }
        }

        [Test]
        public void UnpoweredPulseLockBlocksEveryPlayableLaneThroughoutMotion()
        {
            foreach (var c in Courses())
            foreach (var h in c.TimedHazards)
            {
                if (!h.PulseLock) continue;
                Assert.That(h.Kind, Is.EqualTo(TimedHazardKind.MovingWall));
                for (int phase = 0; phase < 24; phase++)
                for (int lane = -22; lane <= 22; lane++)
                {
                    float time = phase * Mathf.PI * 2 / (24 * h.AngularSpeed);
                    Assert.That(h.HitsSweptSphere(new Vector2(lane * .1f, h.Center.y),
                        new Vector2(lane * .1f, h.Center.y), .38f, time, time), Is.True, c.Id);
                }
            }
        }

        [Test]
        public void EachFiveChunkCycleContainsOneRelayAndOnePulseLockWithFreshData()
        {
            foreach (int seed in new[] { 0, 1, -17, int.MaxValue })
            for (int cycle = 0; cycle < 5; cycle++)
            {
                int relays = 0, locks = 0;
                for (int i = cycle * 5; i < cycle * 5 + 5; i++)
                {
                    var c = EndlessCourse.CreateChunk(i, seed);
                    relays += c.Relays.Length;
                    locks += Array.FindAll(c.TimedHazards, h => h.PulseLock).Length;
                    Assert.That(c.Length, Is.EqualTo(112));
                    if (c.Relays.Length > 0)
                    {
                        var fresh = EndlessCourse.CreateChunk(i, seed);
                        Assert.That(c.Relays[0], Is.Not.SameAs(fresh.Relays[0]));
                        Assert.That(c.Relays[0].Position, Is.EqualTo(fresh.Relays[0].Position));
                    }
                }
                Assert.That(relays, Is.EqualTo(1));
                Assert.That(locks, Is.EqualTo(1));
            }
        }
    }
}
