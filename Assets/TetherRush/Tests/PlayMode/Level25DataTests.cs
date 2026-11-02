using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using TetherRush.Course;
using UnityEngine;

namespace TetherRush.Tests
{
    public sealed class Level25DataTests
    {
        [Test]
        public void TwentyFiveNumberedLevelsHaveDistinctGameplayAndIndependentRunData()
        {
            Assert.That(LevelCatalog.Count, Is.EqualTo(25));
            var signatures = new HashSet<string>();
            for (int i = 0; i < LevelCatalog.Count; i++)
            {
                var c = LevelCatalog.Get(i);
                var fresh = LevelCatalog.Get(i);
                Assert.That(c.Id, Is.EqualTo("level-" + (i + 1)));
                Assert.That(c.Title, Is.EqualTo("Bölüm " + (i + 1)));
                Assert.That(c.Description, Is.Not.Empty);
                Assert.That(signatures.Add(GameplaySignature(c)), Is.True, c.Id);
                Assert.That(c.Hazards, Is.Not.Null);
                Assert.That(c.Pickups, Is.Not.Null);
                Assert.That(c.TimedHazards, Is.Not.Null);
                Assert.That(c.Hints, Is.Not.Empty);
                Assert.That(c.SafeRoute, Is.Not.Empty);
                for (int n = 0; n < c.Hazards.Length; n++)
                {
                    Assert.That(c.Hazards[n], Is.Not.SameAs(fresh.Hazards[n]));
                    AssertFinite(c.Hazards[n].Position.z);
                    Assert.That(c.Hazards[n].Position.z, Is.InRange(6, c.Length - 5));
                }
                for (int n = 0; n < c.Pickups.Length; n++)
                    Assert.That(c.Pickups[n], Is.Not.SameAs(fresh.Pickups[n]));
                for (int n = 0; n < c.TimedHazards.Length; n++)
                {
                    Assert.That(c.TimedHazards[n], Is.Not.SameAs(fresh.TimedHazards[n]));
                    Assert.That(c.TimedHazards[n].Center.y, Is.InRange(6, c.Length - 5));
                }
                if (i >= 5) Assert.That(c.Length, Is.InRange(124, 280));
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => LevelCatalog.Get(25));
        }

        [Test]
        public void ReferenceInputsAreRealAlternatingEventsWithReactionTimeAndReleasedFinish()
        {
            for (int i = 0; i < LevelCatalog.Count; i++)
            {
                var c = LevelCatalog.Get(i);
                bool held = false;
                float previous = 0;
                foreach (var cue in c.SafeRoute)
                {
                    AssertFinite(cue.Distance);
                    Assert.That(cue.Distance, Is.InRange(4, c.Length - 4), c.Id);
                    Assert.That(cue.Distance - previous, Is.GreaterThanOrEqualTo(6), c.Id);
                    Assert.That(cue.Press, Is.Not.EqualTo(held), c.Id);
                    held = cue.Press;
                    previous = cue.Distance;
                }
                Assert.That(held, Is.False, c.Id);
                previous = -1;
                foreach (var hint in c.Hints)
                {
                    Assert.That(hint.Distance, Is.GreaterThan(previous));
                    Assert.That(hint.Distance, Is.InRange(0, c.Length));
                    Assert.That(hint.Text, Is.Not.Empty);
                    previous = hint.Distance;
                }
            }
        }

        [Test]
        public void EveryPulseUsesFiveNewReachableParticlesDuringTheImmediatelyPrecedingHold()
        {
            int multiPulseLevels = 0;
            for (int i = 0; i < LevelCatalog.Count; i++)
            {
                var c = LevelCatalog.Get(i);
                int targets = 0;
                foreach (var target in c.Hazards)
                {
                    if (!target.Marked) continue;
                    targets++;
                    Assert.That(c.UsesEnergy, Is.True);
                    int releaseIndex = Array.FindIndex(c.SafeRoute,
                        cue => !cue.Press && Mathf.Abs(cue.Distance - (target.Position.z - 4)) < .001f);
                    Assert.That(releaseIndex, Is.GreaterThan(0), c.Id);
                    var press = c.SafeRoute[releaseIndex - 1];
                    Assert.That(press.Press, Is.True);
                    float release = c.SafeRoute[releaseIndex].Distance;
                    int particles = 0;
                    foreach (var p in c.Pickups)
                    {
                        if (p.Bonus || p.Position.z <= press.Distance + 3 || p.Position.z >= release - 3) continue;
                        Assert.That(Mathf.Abs(p.Position.x), Is.LessThan(2.2f));
                        particles++;
                    }
                    Assert.That(particles, Is.EqualTo(5), c.Id);
                }
                if (targets > 1) multiPulseLevels++;
            }
            Assert.That(multiPulseLevels, Is.GreaterThanOrEqualTo(6));
            Assert.That(Array.FindAll(LevelCatalog.Get(24).Hazards, h => h.Marked).Length, Is.EqualTo(3));
        }

        [Test]
        public void AllTimedBeatsAndExistingEndlessChunksKeepGlobalDistancePhaseCalibration()
        {
            for (int i = 0; i < 25; i++)
            {
                AssertPhase(LevelCatalog.Get(i));
                var chunk = EndlessCourse.CreateChunk(i, 23);
                Assert.That(chunk.Length, Is.EqualTo(112));
                Assert.That(chunk.Id, Is.EqualTo("endless-23-" + i));
                AssertPhase(chunk);
            }
        }

        private static void AssertPhase(CourseDefinition course)
        {
            foreach (var h in course.TimedHazards)
            {
                float time = h.Center.y / 7;
                if (h.Kind == TimedHazardKind.RotatingArm)
                    Assert.That(Mathf.Abs(Mathf.Cos(h.AngleAt(time))), Is.LessThan(.0001f));
                else Assert.That(h.PositionAt(time).x, Is.EqualTo(h.Amplitude > 1 ? 2.2f : 0).Within(.0001f));
            }
        }

        private static void AssertFinite(float value)
        {
            Assert.That(float.IsNaN(value) || float.IsInfinity(value), Is.False);
        }

        // Excludes level ID, text and length: renaming or extending a duplicate cannot pass.
        private static string GameplaySignature(CourseDefinition course)
        {
            var s = new StringBuilder();
            foreach (var h in course.Hazards)
                s.Append(h.Position.ToString("R")).Append(h.Size.ToString("R")).Append(h.Marked).Append(h.Gate);
            foreach (var h in course.TimedHazards)
                s.Append(h.Kind).Append(h.Center.ToString("R")).Append(h.Phase.ToString("R", CultureInfo.InvariantCulture));
            foreach (var p in course.Pickups) s.Append(p.Position.ToString("R")).Append(p.Bonus);
            foreach (var cue in course.SafeRoute)
                s.Append(cue.Distance.ToString("R", CultureInfo.InvariantCulture)).Append(cue.Press);
            return s.ToString();
        }
    }
}
