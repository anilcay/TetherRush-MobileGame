using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using TetherRush.Core;
using TetherRush.Course;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TetherRush.Tests
{
    public sealed class InteractiveRuntimeTests
    {
        private GameObject root;
        private TetherRushGame game;
        private const float Step = 1f / 120f;

        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }

        private void Create(int level = -1)
        {
            Cleanup(); root = new GameObject("Interactive runtime fixture"); root.SetActive(false);
            game = root.AddComponent<TetherRushGame>();
            if (level < 0) game.ConfigureEndless(); else game.ConfigureLevel(level);
            root.SetActive(true); game.AutoSimulate = false; game.Feedback.AudioEnabled = false;
            game.StartRun();
        }

        private void RouteStep(CourseDefinition course, ref int cue)
        {
            bool press = false, release = false;
            if (cue < course.SafeRoute.Length && game.Model.Distance >= course.SafeRoute[cue].Distance)
            { press = course.SafeRoute[cue].Press; release = !press; cue++; }
            game.SimulateStep(Step, press, release);
        }

        private void Advance(CourseDefinition course, ref int cue, float destination)
        {
            for (int n = 0; n < 12000 && game.Model.Distance < destination && game.Model.Phase == RunPhase.Running; n++)
                RouteStep(course, ref cue);
            Assert.That(game.Model.Phase == RunPhase.Completed || game.Model.Distance >= destination, Is.True,
                course.Id + " stopped at " + game.Model.Distance + ": " + game.Model.FailureReason);
        }

        private void NoInputUntil(float destination)
        {
            for (int n = 0; n < 12000 && game.Model.Distance < destination && game.Model.Phase == RunPhase.Running; n++)
                game.SimulateStep(Step);
        }

        [UnityTest]
        public IEnumerator RealRelayCrossingOpensWithoutEnergyOrPulseAndCompletesAtFiniteSpeed()
        {
            Create(5); var course = LevelCatalog.Get(5); int cue = 0;
            var relay = course.Relays[0];
            Advance(course, ref cue, relay.Position.z - 3);
            Assert.That(game.ActivatedRelayCount, Is.Zero);
            yield return Capture("Systems_Relay_Before.png");
            Advance(course, ref cue, relay.Position.z + .1f);
            Assert.That(game.ActivatedRelayCount, Is.EqualTo(1));
            Assert.That(game.Model.Energy, Is.Zero); Assert.That(game.Model.PulseCount, Is.Zero);
            Assert.That(game.MechanicFeedback.ActiveEffectCount, Is.InRange(1, 8));
            Advance(course, ref cue, relay.Position.z + 1.6f);
            yield return Capture("Systems_Relay_After.png");
            Advance(course, ref cue, course.Length + 1);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Completed));
            Assert.That(game.CurrentForwardSpeed, Is.EqualTo(7f));
            Assert.That(game.Model.Distance / game.Model.ActualElapsedTime, Is.EqualTo(7f).Within(.002f));
            game.RestartRun();
            Assert.That(game.ActivatedRelayCount, Is.Zero);
            Assert.That(game.MechanicFeedback.ActiveEffectCount, Is.Zero);
            Assert.That(game.CurrentForwardSpeed, Is.EqualTo(7f));
            // A fresh narrow attempt must not inherit the previously opened gate.
            NoInputUntil(relay.Position.z + 1); game.SimulateStep(Step, press: true);
            NoInputUntil(relay.Position.z + 20);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
            Assert.That(game.ActivatedRelayCount, Is.Zero);
        }

        [Test]
        public void MissingRelayCannotBeRepairedByFullPulseAtItsUnmarkedGate()
        {
            Create(5); var course = LevelCatalog.Get(5); var relay = course.Relays[0];
            var gate = Array.Find(course.Hazards, h => h.RelayId == relay.Id);
            NoInputUntil(relay.Position.z + 1);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Running));
            game.Model.CollectEnergy(90000, 5);
            game.SimulateStep(.3f, press: true);
            NoInputUntil(gate.Position.z - 4);
            game.SimulateStep(Step, release: true);
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.ActivatedRelayCount, Is.Zero);
            Assert.That(game.ActivatedTargetCount, Is.Zero);
            NoInputUntil(relay.Position.z + 20);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
            Assert.That(game.Model.Distance, Is.InRange(gate.Position.z - 2, gate.Position.z + 1));
        }

        [Test]
        public void SpreadingAfterNarrowRelayCrossingCannotActivateItRetroactively()
        {
            Create(5); var relay = LevelCatalog.Get(5).Relays[0];
            NoInputUntil(relay.Position.z - .5f);
            game.SimulateStep(Step, press: true);
            NoInputUntil(relay.Position.z + 2);
            Assert.That(game.Model.HalfSeparation, Is.GreaterThan(relay.RequiredHalfSpan));
            Assert.That(game.ActivatedRelayCount, Is.Zero);
            NoInputUntil(relay.Position.z + 20);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
        }

        [UnityTest]
        public IEnumerator MovingPulseLockRetractsOnRealChargedReleaseAndPauseRetryResetItsEffects()
        {
            Create(7); var course = LevelCatalog.Get(7); int cue = 0;
            var pulseLock = Array.Find(course.TimedHazards, h => h.PulseLock);
            Transform source = null;
            foreach (var view in root.GetComponentsInChildren<Transform>(true))
                if (view.name == "Timed hazard" && Mathf.Abs(view.position.z - pulseLock.Center.y) < .001f) source = view;
            Assert.That(source, Is.Not.Null);
            Advance(course, ref cue, pulseLock.Center.y - 4.2f);
            Assert.That(game.Model.Energy, Is.EqualTo(5));
            Assert.That(game.ClosedPulseLockCount, Is.EqualTo(1));
            yield return Capture("Systems_PulseLock_Before.png");
            Advance(course, ref cue, pulseLock.Center.y - 3.9f);
            Assert.That(game.ClosedPulseLockCount, Is.Zero);
            Assert.That(source.gameObject.activeSelf, Is.False, "Gameplay source must hide immediately during the visual retraction");
            Assert.That(game.Model.Energy, Is.Zero); Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.MechanicFeedback.ActiveEffectCount, Is.InRange(1, 8));
            game.PauseRun(); int effects = game.MechanicFeedback.ActiveEffectCount;
            float distance = game.Model.Distance; game.SimulateStep(2);
            Assert.That(game.MechanicFeedback.ActiveEffectCount, Is.EqualTo(effects));
            Assert.That(game.Model.Distance, Is.EqualTo(distance));
            game.ResumeRun(); game.SimulateStep(.4f);
            Assert.That(game.MechanicFeedback.ActiveEffectCount, Is.Zero);
            yield return Capture("Systems_PulseLock_After.png");
            Advance(course, ref cue, course.Length + 1);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Completed));
            game.RestartRun();
            Assert.That(game.ClosedPulseLockCount, Is.EqualTo(1));
            Assert.That(source.gameObject.activeSelf, Is.True);
            Assert.That(game.Model.PulseCount, Is.Zero); Assert.That(game.Model.Energy, Is.Zero);
            Assert.That(game.MechanicFeedback.ActiveEffectCount, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PartialOrTooEarlyReleaseLeavesMovingPulseLockFatal(bool partial)
        {
            Create(7); var course = LevelCatalog.Get(7); int cue = 0;
            var pulseLock = Array.Find(course.TimedHazards, h => h.PulseLock);
            Advance(course, ref cue, pulseLock.Center.y - (partial ? 4.2f : 8f));
            Assert.That(game.Model.Energy, Is.EqualTo(5));
            if (partial)
            {
                // Isolate the energy prerequisite after the real route collected its charge.
                // Direct model release drains energy without invoking the runtime's target action.
                Assert.That(game.Model.Release(), Is.True);
                game.Model.CollectEnergy(90001, 2);
                game.SimulateStep(0, press: true);
            }
            int pulsesBefore = game.Model.PulseCount;
            game.SimulateStep(Step, release: true);
            Assert.That(game.Model.PulseCount, Is.EqualTo(pulsesBefore + (partial ? 0 : 1)));
            Assert.That(game.Model.Energy, Is.EqualTo(partial ? 2 : 0));
            Assert.That(game.ClosedPulseLockCount, Is.EqualTo(1));
            NoInputUntil(pulseLock.Center.y + 2);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
            Assert.That(game.Model.Distance, Is.InRange(pulseLock.Center.y - 2, pulseLock.Center.y + 1));
        }

        [Test]
        public void NormalMovingHazardRemainsFatalAfterNearbyFullPulse()
        {
            Create(3); var course = LevelCatalog.Get(3); int cue = 0;
            var hazard = course.TimedHazards[0];
            Assert.That(hazard.PulseLock, Is.False);
            Advance(course, ref cue, hazard.Center.y - 4);
            game.Model.CollectEnergy(90002, 5);
            game.SimulateStep(Step, release: true);
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.ActivatedTargetCount, Is.Zero);
            Assert.That(game.ClosedPulseLockCount, Is.Zero);
            NoInputUntil(hazard.Center.y + 2);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
            Assert.That(game.Model.Distance, Is.InRange(hazard.Center.y - 2, hazard.Center.y + 1));
        }

        [UnityTest]
        public IEnumerator EndlessAtMaximumScoreKeepsRealFiveChunkRoutesBoundedAndResetsPace()
        {
            Create(); game.Model.CollectBonus(99000, 100);
            int chunk = 0, cue = 0, observedRelayChunks = 0, observedLockChunks = 0;
            var course = EndlessCourse.CreateChunk(0); float previousSpeed = 7f;
            bool captured = false; var samples = new List<string>();
            for (int n = 0; n < 12000 && game.Model.Distance < 5 * EndlessCourse.ChunkLength && game.Model.Phase == RunPhase.Running; n++)
            {
                if (game.Model.Distance >= (chunk + 1) * EndlessCourse.ChunkLength)
                {
                    if (course.Relays.Length > 0) { observedRelayChunks++; Assert.That(game.ActivatedRelayCount, Is.GreaterThan(0)); }
                    if (Array.Exists(course.TimedHazards, h => h.PulseLock)) observedLockChunks++;
                    chunk++; course = EndlessCourse.CreateChunk(chunk); cue = 0;
                    Assert.That(game.ActiveChunkCount, Is.LessThanOrEqualTo(4));
                    Assert.That(game.Model.TrackedCollectibleCount, Is.LessThan(80));
                    Assert.That(root.GetComponentsInChildren<MeshRenderer>(true).Length, Is.LessThan(400));
                    yield return null;
                }
                RouteStep(course, ref cue);
                Assert.That(game.CurrentForwardSpeed, Is.InRange(previousSpeed - .000001f, 11.5f));
                Assert.That(game.CurrentForwardSpeed - previousSpeed, Is.LessThanOrEqualTo(.25f * Step + .000001f));
                previousSpeed = game.CurrentForwardSpeed;
                Assert.That(game.MechanicFeedback.ActiveEffectCount, Is.LessThanOrEqualTo(8));
                if (n % 240 == 0) samples.Add("{\"seconds\":" + Number(game.Model.ActualElapsedTime) +
                    ",\"distance\":" + Number(game.Model.Distance) + ",\"speed\":" + Number(game.CurrentForwardSpeed) + "}");
                if (!captured && game.CurrentForwardSpeed >= 11.49f)
                { captured = true; yield return Capture("Systems_Endless_HighPace.png"); }
            }
            // Include the final chunk, whose boundary may end the bounded loop directly.
            if (course.Relays.Length > 0) { observedRelayChunks++; Assert.That(game.ActivatedRelayCount, Is.GreaterThan(0)); }
            if (Array.Exists(course.TimedHazards, h => h.PulseLock)) observedLockChunks++;
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Running), game.Model.FailureReason + " at " + game.Model.Distance);
            Assert.That(game.Model.Distance, Is.GreaterThanOrEqualTo(5 * EndlessCourse.ChunkLength));
            Assert.That(observedRelayChunks, Is.GreaterThanOrEqualTo(1));
            Assert.That(observedLockChunks, Is.GreaterThanOrEqualTo(1));
            Assert.That(game.CurrentForwardSpeed, Is.EqualTo(11.5f)); Assert.That(captured, Is.True);
            File.WriteAllText(Path.Combine(OutputDirectory(), "Systems_Pace_samples.json"), "[" + string.Join(",", samples) + "]");
            game.PauseRun(); float speed = game.CurrentForwardSpeed, time = game.Model.ActualElapsedTime, z = game.Model.Distance;
            game.SimulateStep(10);
            Assert.That(game.CurrentForwardSpeed, Is.EqualTo(speed)); Assert.That(game.Model.Distance, Is.EqualTo(z));
            Assert.That(game.Model.ActualElapsedTime, Is.EqualTo(time));
            game.RestartRun();
            Assert.That(game.CurrentForwardSpeed, Is.EqualTo(7f)); Assert.That(game.Score, Is.Zero);
            Assert.That(game.ActivatedRelayCount, Is.Zero); Assert.That(game.MechanicFeedback.ActiveEffectCount, Is.Zero);
            Assert.That(game.Model.ActualElapsedTime, Is.Zero);
        }

        private static string Number(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        private static string OutputDirectory()
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "output", "previews"));
            Directory.CreateDirectory(path); return path;
        }

        private IEnumerator Capture(string file)
        {
            game.SimulateStep(0); yield return null;
            var camera = root.GetComponentInChildren<Camera>(); var canvas = root.GetComponentInChildren<Canvas>();
            Assert.That(RenderPipeline.SupportsRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest()), Is.True);
            var scaler = canvas.GetComponent<CanvasScaler>(); bool enabled = scaler.enabled;
            var target = new RenderTexture(540, 960, 24, RenderTextureFormat.ARGB32);
            var oldActive = RenderTexture.active; var oldTarget = camera.targetTexture;
            var mode = canvas.renderMode; var oldCamera = canvas.worldCamera; float scale = canvas.scaleFactor, plane = canvas.planeDistance;
            Texture2D image = null;
            try
            {
                target.Create(); camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera; canvas.planeDistance = 1; scaler.enabled = false; canvas.scaleFactor = 1;
                Canvas.ForceUpdateCanvases(); yield return null; Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                yield return null; Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target; image = new Texture2D(540, 960, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(OutputDirectory(), file), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget; canvas.renderMode = mode; canvas.worldCamera = oldCamera; canvas.planeDistance = plane;
                scaler.enabled = enabled; canvas.scaleFactor = scale; RenderTexture.active = oldActive;
                if (image != null) Object.DestroyImmediate(image); target.Release(); Object.DestroyImmediate(target);
            }
        }
    }
}
