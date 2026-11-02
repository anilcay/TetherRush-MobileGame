using System;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using TetherRush.Course;
using UnityEngine;

namespace TetherRush.Tests
{
    public sealed class CourseDefinitionTests
    {
        [Test]
        public void TwentyFiveNumberedLevelsRetainTheIntroProgression()
        {
            Assert.That(LevelCatalog.Count, Is.EqualTo(25));
            for (int i = 0; i < LevelCatalog.Count; i++)
            {
                var course = LevelCatalog.Get(i);
                Assert.That(course.Title, Is.EqualTo("Bölüm " + (i + 1)));
                Assert.That(course.Length, Is.InRange(55, 300));
                Validate(course, 0);
                Assert.That(LevelCatalog.Get(i), Is.Not.SameAs(course));
                course.Hazards[0].Reason = "mutated";
                Assert.That(LevelCatalog.Get(i).Hazards[0].Reason, Is.Not.EqualTo("mutated"));
            }
            Assert.That(LevelCatalog.Get(0).UsesEnergy, Is.False);
            Assert.That(LevelCatalog.Get(1).UsesEnergy, Is.True);
            Assert.That(Array.FindAll(LevelCatalog.Get(1).Pickups, p => !p.Bonus).Length, Is.EqualTo(5));
            Assert.That(Array.Exists(LevelCatalog.Get(2).Hazards, h => h.Gate && h.Marked), Is.True);
            Assert.That(LevelCatalog.Get(3).TimedHazards.Length, Is.EqualTo(3));
            Assert.That(Array.Exists(LevelCatalog.Get(4).Hazards, h => h.Marked && !h.Gate), Is.True);
        }

        [Test]
        public void InvalidIndicesFailExplicitly()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelCatalog.Get(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelCatalog.Get(25));
            Assert.Throws<ArgumentOutOfRangeException>(() => EndlessCourse.CreateChunk(-1));
        }

        [Test]
        public void EndlessChunksAreDeterministicFreshAndUseAbsoluteCoordinates()
        {
            foreach (int seed in new[] { 0, 1, -17, int.MaxValue })
            {
                var ids = new System.Collections.Generic.HashSet<string>();
                for (int i = 0; i < 25; i++)
                {
                    var chunk = EndlessCourse.CreateChunk(i, seed);
                    Assert.That(chunk.Title, Is.EqualTo("Sonsuz Akış"));
                    Assert.That(chunk.Length, Is.EqualTo(112));
                    Assert.That(ids.Add(chunk.Id), Is.True);
                    Validate(chunk, i * EndlessCourse.ChunkLength);
                    Assert.That(Fingerprint(chunk), Is.EqualTo(Fingerprint(EndlessCourse.CreateChunk(i, seed))));
                    Assert.That(EndlessCourse.CreateChunk(i, seed).Hazards, Is.Not.SameAs(chunk.Hazards));
                }
            }
            Assert.That(Fingerprint(EndlessCourse.CreateChunk(0, 1)),
                Is.Not.EqualTo(Fingerprint(EndlessCourse.CreateChunk(0, 2))));
        }

        [Test]
        public void EveryMarkedTargetHasFivePrecedingEnergyAndAnUninterruptedHold()
        {
            for (int i = 0; i < LevelCatalog.Count; i++) ValidatePulseSequence(LevelCatalog.Get(i));
            for (int i = 0; i < 50; i++) ValidatePulseSequence(EndlessCourse.CreateChunk(i, 9));
        }

        [Test]
        public void TimedPhasesAreCalibratedAtGlobalCrossingTime()
        {
            for (int index = 0; index < 50; index++)
            {
                var chunk = EndlessCourse.CreateChunk(index, 31);
                foreach (var hazard in chunk.TimedHazards)
                {
                    float time = hazard.Center.y / 7;
                    if (hazard.Kind == TimedHazardKind.RotatingArm)
                        Assert.That(Mathf.Abs(Mathf.Cos(hazard.AngleAt(time))), Is.LessThan(.0001f));
                    else
                        Assert.That(hazard.PositionAt(time).x,
                            Is.EqualTo(hazard.Amplitude > 1 ? 2.2f : 0).Within(.0001f));
                }
            }
        }

        private static void Validate(CourseDefinition course, float start)
        {
            Assert.That(course.Id, Is.Not.Empty); Assert.That(course.Description, Is.Not.Empty);
            Assert.That(course.Hazards, Is.Not.Null); Assert.That(course.Pickups, Is.Not.Null);
            Assert.That(course.TimedHazards, Is.Not.Null); Assert.That(course.Hints, Is.Not.Empty);
            Assert.That(course.SafeRoute, Is.Not.Empty);
            float end = start + course.Length;
            foreach (var h in course.Hazards)
            {
                Finite(h.Position); Finite(h.Size);
                Assert.That(h.Position.z, Is.InRange(start + 6, end - 5));
                Assert.That(h.Size.x, Is.GreaterThan(0)); Assert.That(h.Size.y, Is.GreaterThan(0));
                Assert.That(h.Size.z, Is.GreaterThan(0));
                Assert.That(h.Reason, Is.Not.Empty);
                if (h.Gate) Assert.That(h.Marked, Is.True);
            }
            foreach (var p in course.Pickups)
            { Finite(p.Position); Assert.That(p.Position.z, Is.InRange(start, end)); }
            foreach (var h in course.TimedHazards)
            {
                Assert.That(h.Center.y, Is.InRange(start + 6, end - 5));
                Assert.That(h.HalfExtents.x, Is.GreaterThan(0)); Assert.That(h.HalfExtents.y, Is.GreaterThan(0));
                Assert.That(h.Hint, Is.Not.Empty); h.PositionAt(h.Center.y / 7);
            }
            float previous = start - 1;
            foreach (var hint in course.Hints)
            { Assert.That(hint.Distance, Is.InRange(start, end)); Assert.That(hint.Distance, Is.GreaterThan(previous));
                Assert.That(hint.Text, Is.Not.Empty); previous = hint.Distance; }
            bool expectedPress = true;
            previous = start;
            foreach (var cue in course.SafeRoute)
            {
                Assert.That(cue.Distance, Is.InRange(start + 4, end - 4));
                // More than .85 seconds between decisions, with ample room for the .26s transition.
                Assert.That(cue.Distance - previous, Is.GreaterThanOrEqualTo(6));
                Assert.That(cue.Press, Is.EqualTo(expectedPress)); expectedPress = !expectedPress; previous = cue.Distance;
            }
            Assert.That(course.SafeRoute[course.SafeRoute.Length - 1].Press, Is.False);
        }

        private static void ValidatePulseSequence(CourseDefinition c)
        {
            foreach (var target in c.Hazards)
            {
                if (!target.Marked) continue;
                float release = target.Position.z - 4;
                int cueIndex = Array.FindIndex(c.SafeRoute, cue => !cue.Press && Mathf.Abs(cue.Distance - release) < .001f);
                Assert.That(cueIndex, Is.GreaterThan(0));
                float press = c.SafeRoute[cueIndex - 1].Distance;
                Assert.That(c.SafeRoute[cueIndex - 1].Press, Is.True);
                int count = Array.FindAll(c.Pickups, p => !p.Bonus && p.Position.z > press + 3 && p.Position.z < release - 3).Length;
                Assert.That(count, Is.GreaterThanOrEqualTo(5));
            }
        }

        private static void Finite(Vector3 value)
        {
            foreach (float n in new[] { value.x, value.y, value.z })
                Assert.That(!float.IsNaN(n) && !float.IsInfinity(n), Is.True);
        }

        private static string Fingerprint(CourseDefinition c)
        {
            var result = new StringBuilder(c.Id);
            foreach (var h in c.Hazards) result.Append(h.Position.ToString("R")).Append(h.Size.ToString("R")).Append(h.Marked).Append(h.Gate);
            foreach (var p in c.Pickups) result.Append(p.Position.ToString("R")).Append(p.Bonus);
            foreach (var h in c.TimedHazards) result.Append(h.Center.ToString("R")).Append(h.Phase.ToString("R", CultureInfo.InvariantCulture));
            foreach (var cue in c.SafeRoute) result.Append(cue.Distance.ToString("R", CultureInfo.InvariantCulture)).Append(cue.Press);
            return result.ToString();
        }
    }
}
