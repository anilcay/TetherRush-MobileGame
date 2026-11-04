using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using Unity.Profiling;
using TetherRush.Core;
using TetherRush.Course;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using Stopwatch = System.Diagnostics.Stopwatch;
using Object = UnityEngine.Object;

namespace TetherRush.Tests
{
    // Measurement harness, not a phone FPS benchmark. Only SimulateStep is timed;
    // test routing, assertions, serialization and deferred destruction are outside it.
    public sealed class OptimizationMeasurements
    {
        private GameObject root;
        [Serializable] private sealed class Measurement
        {
            public string scenario, label, unity, phase;
            public int frames, bonus, pulses, targets, relays, activeChunks, trackedPickups;
            public float distance, half, speed;
            public long allocationEvents, maxFrameAllocationEvents;
            public double meanCpuUs, p50CpuUs, p95CpuUs, maxCpuUs;
            public int observedPreviewRenders;
        }
        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }
        private TetherRushGame Create(int level, bool endless)
        {
            root = new GameObject("Optimization measurement"); root.SetActive(false);
            var game = root.AddComponent<TetherRushGame>();
            if (endless) game.ConfigureEndless(); else game.ConfigureLevel(level);
            root.SetActive(true); game.AutoSimulate = false; game.Feedback.AudioEnabled = false; game.StartRun();
            return game;
        }
        private static void Step(TetherRushGame game, CourseDefinition course, ref int cue)
        {
            bool press = false, release = false;
            if (cue < course.SafeRoute.Length && game.Model.Distance >= course.SafeRoute[cue].Distance)
            { press = course.SafeRoute[cue].Press; release = !press; cue++; }
            game.SimulateStep(1f / 60, press, release);
        }
        [UnityTest]
        public IEnumerator CaptureGameplayAndIdleAllocations()
        {
            using (var calibration = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Alloc", 128,
                ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
            {
                GC.KeepAlive(new byte[4096]); calibration.Stop();
                Assert.That(Allocated(calibration), Is.GreaterThanOrEqualTo(1), "GC recorder must detect a known allocation");
            }
            foreach (int level in new[] { 0, 24, -1 })
            {
                bool endless = level < 0; var game = Create(level, endless);
                var course = endless ? EndlessCourse.CreateChunk(0) : LevelCatalog.Get(level);
                int cue = 0, chunk = 0;
                for (int n = 0; n < 60; n++) Step(game, course, ref cue);
                var ticks = new long[16000]; long events = 0, maxEvents = 0; int count = 0;
                var allocations = new ProfilerRecorder(ProfilerCategory.Internal, "GC.Alloc", 8192,
                    ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
                try
                {
                float stop = endless ? 8 * EndlessCourse.ChunkLength : course.Length;
                while (game.Model.Phase == RunPhase.Running && game.Model.Distance < stop && count < ticks.Length)
                {
                    if (endless && game.Model.Distance >= (chunk + 1) * EndlessCourse.ChunkLength)
                    { course = EndlessCourse.CreateChunk(++chunk); cue = 0; yield return null; }
                    // Route generation/input bookkeeping is deliberately excluded.
                    bool press = false, release = false;
                    if (cue < course.SafeRoute.Length && game.Model.Distance >= course.SafeRoute[cue].Distance)
                    { press = course.SafeRoute[cue].Press; release = !press; cue++; }
                    allocations.Reset(); allocations.Start(); long start = Stopwatch.GetTimestamp();
                    game.SimulateStep(1f / 60, press, release);
                    ticks[count++] = Stopwatch.GetTimestamp() - start;
                    allocations.Stop(); long allocated = Allocated(allocations);
                    events += allocated; maxEvents = Math.Max(maxEvents, allocated);
                }
                Assert.That(count, Is.LessThan(ticks.Length));
                Assert.That(game.Model.Phase, Is.EqualTo(endless ? RunPhase.Running : RunPhase.Completed), game.Model.FailureReason);
                Save(game, endless ? "endless_8_chunks" : "level_" + (level + 1), ticks, count, events, maxEvents);
                game.PauseRun();
                for (int n = 0; n < 64; n++) game.SimulateStep(0);
                events = maxEvents = 0;
                for (int n = 0; n < 512; n++)
                {
                    allocations.Reset(); allocations.Start(); long start = Stopwatch.GetTimestamp();
                    game.SimulateStep(0); ticks[n] = Stopwatch.GetTimestamp() - start;
                    allocations.Stop(); long allocated = Allocated(allocations);
                    events += allocated; maxEvents = Math.Max(maxEvents, allocated);
                }
                Save(game, endless ? "endless_idle" : "level_" + (level + 1) + "_idle", ticks, 512, events, maxEvents);
                }
                finally { allocations.Dispose(); }
                Object.Destroy(root); root = null; yield return null;
            }
        }
        private static long Allocated(ProfilerRecorder recorder)
        {
            Assert.That(recorder.Valid, Is.True);
            Assert.That(recorder.Count, Is.LessThan(8192), "Allocation sample buffer must not overflow");
            // GC.Alloc is a marker: Value is duration, not allocated bytes.
            // Count records allocation events on the measured thread.
            return recorder.Count;
        }
        [UnityTest]
        public IEnumerator CaptureIdleMenuPreviewCameraWork()
        {
            root = new GameObject("Menu performance measurement"); root.SetActive(false);
            var app = root.AddComponent<TetherRushApp>(); app.ConfigureSession(new PlayerProgress()); root.SetActive(true);
            for (int i = 0; i < 8; i++) yield return null;
            int renders = 0;
            Action<ScriptableRenderContext, Camera> onCamera = (context, camera) => { if (camera.name == "Menu preview camera") renders++; };
            RenderPipelineManager.beginCameraRendering += onCamera;
            try { for (int i = 0; i < 30; i++) yield return null; }
            finally { RenderPipelineManager.beginCameraRendering -= onCamera; }
            Write(new Measurement { scenario = "menu_idle_30_frames", frames = 30, observedPreviewRenders = renders });
        }
        private static void Save(TetherRushGame game, string scenario, long[] ticks, int count, long events, long maxEvents)
        {
            var ordered = new long[count]; Array.Copy(ticks, ordered, count); Array.Sort(ordered);
            double total = 0; for (int i = 0; i < count; i++) total += ticks[i];
            double toUs = 1000000.0 / Stopwatch.Frequency;
            Write(new Measurement { scenario = scenario, frames = count, allocationEvents = events, maxFrameAllocationEvents = maxEvents,
                meanCpuUs = total / count * toUs, p50CpuUs = ordered[count / 2] * toUs,
                p95CpuUs = ordered[(int)((count - 1) * .95)] * toUs, maxCpuUs = ordered[count - 1] * toUs,
                phase = game.Model.Phase.ToString(), distance = game.Model.Distance, half = game.Model.HalfSeparation,
                bonus = game.Model.Bonus, pulses = game.Model.PulseCount, targets = game.ActivatedTargetCount,
                relays = game.ActivatedRelayCount, speed = game.CurrentForwardSpeed,
                activeChunks = game.ActiveChunkCount, trackedPickups = game.Model.TrackedCollectibleCount });
        }
        private static void Write(Measurement data)
        {
            data.label = Environment.GetEnvironmentVariable("TETHER_RUSH_PERF_LABEL") ?? "current";
            Assert.That(data.label, Does.Match("^[a-zA-Z0-9_-]+$"));
            data.unity = Application.unityVersion;
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "output", "performance"));
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, data.label + "_" + data.scenario + ".json"), JsonUtility.ToJson(data, true));
        }
    }
}
