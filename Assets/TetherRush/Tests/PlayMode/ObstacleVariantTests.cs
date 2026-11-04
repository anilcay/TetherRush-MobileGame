using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TetherRush.Art;
using TetherRush.Core;
using TetherRush.Course;
using TetherRush.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TetherRush.Tests
{
    public sealed class ObstacleVariantTests
    {
        private GameObject root;
        private TetherRushGame game;
        private static readonly VisualKind[] NewKinds = { VisualKind.ClampBarrier, VisualKind.CapacitorBank,
            VisualKind.HydraulicSlider, VisualKind.TurbineBeam };
        private sealed class Encounter
        {
            public int Level; public float Z; public VisualKind Kind; public HazardSpec Static; public TimedHazard Timed;
        }

        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }

        private void Create(int level)
        {
            Cleanup(); root = new GameObject("Obstacle variant fixture"); root.SetActive(false);
            game = root.AddComponent<TetherRushGame>(); game.ConfigureLevel(level); root.SetActive(true);
            game.AutoSimulate = false; game.Feedback.AudioEnabled = false; game.StartRun();
        }

        private static IEnumerable<Encounter> Encounters(CourseDefinition course, int level)
        {
            foreach (var h in course.Hazards)
                yield return new Encounter { Level = level, Z = h.Position.z, Static = h,
                    Kind = h.Marked ? h.Visual : ObstacleVisuals.Resolve(course.Id, h.Position.z, h.Visual) };
            foreach (var h in course.TimedHazards)
                yield return new Encounter { Level = level, Z = h.Center.y, Timed = h,
                    Kind = ObstacleVisuals.Resolve(course.Id, h.Center.y,
                        h.Kind == TimedHazardKind.RotatingArm ? VisualKind.RotatingArm : VisualKind.MovingWall) };
        }

        private static Dictionary<VisualKind, Encounter> Representatives()
        {
            var selected = new Dictionary<VisualKind, Encounter>();
            for (int i = 0; i < LevelCatalog.Count; i++)
                foreach (var e in Encounters(LevelCatalog.Get(i), i))
                    if (System.Array.IndexOf(NewKinds, e.Kind) >= 0 && !selected.ContainsKey(e.Kind)) selected.Add(e.Kind, e);
            foreach (var kind in NewKinds) Assert.That(selected.ContainsKey(kind), Is.True, "No authored encounter: " + kind);
            return selected;
        }

        [Test]
        public void ResolverIsRepeatablePreservesRandomAndLeavesUnsupportedRolesUnchanged()
        {
            var before = Random.state;
            try
            {
                foreach (var kind in (VisualKind[])System.Enum.GetValues(typeof(VisualKind)))
                    for (int z = 1; z < 160; z += 7)
                    {
                        var resolved = ObstacleVisuals.Resolve("level-9", z, kind);
                        Assert.That(ObstacleVisuals.Resolve("level-9", z, kind), Is.EqualTo(resolved));
                        if (kind != VisualKind.SplitPylon && kind != VisualKind.ChevronBarrier &&
                            kind != VisualKind.MovingWall && kind != VisualKind.RotatingArm)
                            Assert.That(resolved, Is.EqualTo(kind), "Unsupported/marked role " + kind);
                        Assert.That(ObstacleVisuals.Resolve(null, z, kind), Is.EqualTo(kind));
                    }
                Assert.That(Random.state.Equals(before), Is.True, "Cosmetic selection consumed Unity random");
            }
            finally { Random.state = before; }
        }

        [Test]
        public void AuthoredAndEndlessInventoriesUseEveryVariantWithPairedSidesAndRealImportedMeshes()
        {
            var endlessKinds = new HashSet<VisualKind>();
            for (int i = 0; i < 20; i++)
            {
                var c = EndlessCourse.CreateChunk(i);
                foreach (var e in Encounters(c, -1)) endlessKinds.Add(e.Kind);
                AssertPairs(c);
            }
            foreach (var kind in NewKinds) Assert.That(endlessKinds.Contains(kind), Is.True, "Endless missing " + kind);
            for (int i = 0; i < LevelCatalog.Count; i++) AssertPairs(LevelCatalog.Get(i));
            foreach (var e in Representatives().Values)
            {
                Create(e.Level);
                var view = FindView(e);
                Vector3 size = Size(e);
                AssertVector(Vector3.Scale(LocalBounds(view).size, view.lossyScale), size, .025f, e.Kind + " imported local footprint at world scale");
                AssertVector(CombinedBounds(view).center, view.position, .025f, e.Kind + " centered art");
                Assert.That(view.GetComponentsInChildren<Collider>(true), Is.Empty);
                bool danger = false;
                foreach (var r in view.GetComponentsInChildren<MeshRenderer>())
                    foreach (var mat in r.sharedMaterials)
                        if (mat.name == "Fatal warning lights")
                        { danger = true; Assert.That(Vector4.Distance(mat.GetColor("_BaseColor"), VisualTheme.Danger), Is.LessThan(0.00001f)); }
                Assert.That(danger, Is.True, e.Kind + " danger accent");
            }
        }

        private static void AssertPairs(CourseDefinition c)
        {
            foreach (var left in c.Hazards)
            {
                if (left.Visual != VisualKind.SplitPylon || left.Position.x >= 0) continue;
                var right = System.Array.Find(c.Hazards, h => h.Visual == left.Visual && h.Position.z == left.Position.z && h.Position.x > 0);
                Assert.That(right, Is.Not.Null);
                Assert.That(ObstacleVisuals.Resolve(c.Id, left.Position.z, left.Visual),
                    Is.EqualTo(ObstacleVisuals.Resolve(c.Id, right.Position.z, right.Visual)));
            }
        }

        [Test]
        public void NewTimedMeshesFollowAuthoritativePoseAndFreezeResumeAndRestart()
        {
            var selected = Representatives();
            foreach (var kind in new[] { VisualKind.HydraulicSlider, VisualKind.TurbineBeam })
            {
                var e = selected[kind]; Create(e.Level); var view = FindView(e);
                game.SimulateStep(.4f); AssertPose(view, e.Timed);
                Vector3 position = view.position; Quaternion rotation = view.rotation;
                game.PauseRun(); game.SimulateStep(2);
                AssertVector(view.position, position, .0001f, "Paused translation");
                Assert.That(Quaternion.Angle(view.rotation, rotation), Is.LessThan(.001f));
                game.ResumeRun(); game.SimulateStep(.2f); AssertPose(view, e.Timed);
                Assert.That(game.Model.Distance, Is.GreaterThan(3));
                game.RestartRun(); AssertPose(view, e.Timed);
                Assert.That(game.Model.Distance, Is.Zero);
            }
        }

        private void AssertPose(Transform view, TimedHazard h)
        {
            var p = h.PositionAt(game.Model.Distance / 7);
            AssertVector(view.position, new Vector3(p.x, .45f, p.y), .001f, "Authoritative translation");
            Assert.That(Quaternion.Angle(view.rotation, Quaternion.Euler(0, -h.AngleAt(game.Model.Distance / 7) * Mathf.Rad2Deg, 0)), Is.LessThan(.002f));
            AssertVector(view.localScale, new Vector3(h.HalfExtents.x * 2, .9f, h.HalfExtents.y * 2), .025f, "Timed scale");
        }

        [Test]
        public void EndlessInstantiatesResolvedTimedModelsAndKeepsPosesAfterChunkRetirement()
        {
            Cleanup(); root = new GameObject("Endless obstacle fixture"); root.SetActive(false);
            game = root.AddComponent<TetherRushGame>(); game.ConfigureEndless(); root.SetActive(true);
            game.AutoSimulate = false; game.Feedback.AudioEnabled = false; game.StartRun();
            int chunk = 0, cue = 0, checkedNewTimed = 0;
            var course = EndlessCourse.CreateChunk(chunk);
            for (int n = 0; n < 6000 && game.Model.Distance < 5 * EndlessCourse.ChunkLength && game.Model.Phase == RunPhase.Running; n++)
            {
                if (game.Model.Distance >= (chunk + 1) * EndlessCourse.ChunkLength)
                { chunk++; course = EndlessCourse.CreateChunk(chunk); cue = 0; }
                bool press = false, release = false;
                if (cue < course.SafeRoute.Length && game.Model.Distance >= course.SafeRoute[cue].Distance)
                { press = course.SafeRoute[cue].Press; release = !press; cue++; }
                game.SimulateStep(1f / 60, press, release);
                if (n % 120 != 0 || game.Model.Distance < 150) continue;
                Assert.That(game.ActiveChunkCount, Is.LessThanOrEqualTo(4));
                foreach (var t in root.GetComponentsInChildren<Transform>()) Assert.That(t.name, Is.Not.EqualTo("Akış / 0"));
                // Include the streamed look-ahead: the default seed's first timed pattern
                // is chunk zero, and its next occurrence is chunk five.
                for (int liveIndex = chunk; liveIndex * EndlessCourse.ChunkLength < game.Model.Distance + 180; liveIndex++)
                    foreach (var e in Encounters(EndlessCourse.CreateChunk(liveIndex), -1))
                        if (e.Timed != null && (e.Kind == VisualKind.HydraulicSlider || e.Kind == VisualKind.TurbineBeam))
                        { AssertPose(FindView(e), e.Timed); checkedNewTimed++; }
            }
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Running), game.Model.FailureReason);
            Assert.That(game.Model.Distance, Is.GreaterThanOrEqualTo(5 * EndlessCourse.ChunkLength));
            Assert.That(checkedNewTimed, Is.GreaterThan(0), "No real new timed pose checked after stream retirement");
        }

        [Test]
        public void PulseCannotOpenNewNormalBarrierAndSphereContactStillEndsRun()
        {
            Encounter encounter = null;
            for (int i = 0; i < LevelCatalog.Count && encounter == null; i++)
            {
                var c = LevelCatalog.Get(i);
                foreach (var e in Encounters(c, i))
                    if (e.Kind == VisualKind.CapacitorBank && e.Z < 25) { encounter = e; break; }
            }
            Assert.That(encounter, Is.Not.Null, "Need early new center barrier before other hazards");
            Create(encounter.Level); var view = FindView(encounter);
            for (int n = 0; n < 5; n++) game.Model.CollectEnergy(9000 + n);
            for (int n = 0; n < 1000 && game.Model.Distance < encounter.Z - 4 && game.Model.Phase == RunPhase.Running; n++)
                game.SimulateStep(1f / 120);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Running));
            game.SimulateStep(1f / 120, press: true);
            game.SimulateStep(1f / 120, release: true);
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(view.gameObject.activeSelf, Is.True); Assert.That(game.ActivatedTargetCount, Is.Zero);
            for (int n = 0; n < 300 && game.Model.Phase == RunPhase.Running; n++) game.SimulateStep(1f / 120);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
            Assert.That(game.Model.Distance, Is.InRange(encounter.Z - 2, encounter.Z + 1));
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureFourRealAuthoredVariantEncountersAndCompleteTheirRoutes()
        {
            var inventory = new List<string>();
            foreach (var e in Representatives().Values)
            {
                Create(e.Level); int cue = 0; var course = LevelCatalog.Get(e.Level);
                Advance(course, ref cue, Mathf.Max(0, e.Z - 8));
                Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Running)); FindView(e);
                yield return Capture("Obstacles_" + e.Kind + "_Level" + (e.Level + 1) + ".png");
                inventory.Add("{\"kind\":\"" + e.Kind + "\",\"level\":" + (e.Level + 1) +
                    ",\"worldZ\":" + e.Z.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ",\"captureDistance\":" + game.Model.Distance.ToString(System.Globalization.CultureInfo.InvariantCulture) + "}");
                Advance(course, ref cue, course.Length + 1);
                Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Completed), "Capture route " + course.Id);
            }
            string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "output", "previews"));
            File.WriteAllText(Path.Combine(directory, "Obstacles_inventory.json"), "[" + string.Join(",", inventory) + "]");
        }

        private void Advance(CourseDefinition c, ref int cue, float distance)
        {
            for (int n = 0; n < 6000 && game.Model.Distance < distance && game.Model.Phase == RunPhase.Running; n++)
            {
                bool press = false, release = false;
                if (cue < c.SafeRoute.Length && game.Model.Distance >= c.SafeRoute[cue].Distance)
                { press = c.SafeRoute[cue].Press; release = !press; cue++; }
                game.SimulateStep(1f / 60, press, release);
            }
            Assert.That(game.Model.Phase == RunPhase.Completed || game.Model.Distance >= distance, Is.True,
                "Route stopped " + c.Id + " at " + game.Model.Distance + ": " + game.Model.FailureReason);
        }

        private Transform FindView(Encounter e)
        {
            var prefab = Resources.Load<GameObject>("ArtModels/" + e.Kind);
            Assert.That(prefab, Is.Not.Null, e.Kind + " FBX");
            var mesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
            // Opened Pulse locks retain their authoritative source until chunk retirement,
            // while the presentation snapshot retracts and the source stays hidden.
            foreach (var f in root.GetComponentsInChildren<MeshFilter>(true))
                if (f.sharedMesh == mesh)
                {
                    var t = f.transform;
                    while (t.parent != null && t.name != "Visual") t = t.parent;
                    if (t.name == "Visual" && Mathf.Abs(t.parent.position.z - e.Z) < .001f &&
                        (e.Static == null || Mathf.Abs(t.parent.position.x - e.Static.Position.x) < .001f)) return t.parent;
                }
            Assert.Fail("Actual game did not instantiate " + e.Kind + " at " + e.Z); return null;
        }
        private static Vector3 Size(Encounter e) => e.Static != null ?
            new Vector3(e.Static.Size.x, .9f, e.Static.Size.z) :
            new Vector3(e.Timed.HalfExtents.x * 2, .9f, e.Timed.HalfExtents.y * 2);
        private static Bounds CombinedBounds(Transform t)
        {
            var renderers = t.GetComponentsInChildren<MeshRenderer>(); Bounds b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds); return b;
        }
        private static Bounds LocalBounds(Transform rootTransform)
        {
            Bounds combined = new Bounds(); bool first = true;
            foreach (var filter in rootTransform.GetComponentsInChildren<MeshFilter>())
            {
                Bounds b = filter.sharedMesh.bounds;
                for (int x = -1; x <= 1; x += 2)
                    for (int y = -1; y <= 1; y += 2)
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 p = rootTransform.InverseTransformPoint(filter.transform.TransformPoint(
                                b.center + Vector3.Scale(b.extents, new Vector3(x, y, z))));
                            if (first) { combined = new Bounds(p, Vector3.zero); first = false; } else combined.Encapsulate(p);
                        }
            }
            return combined;
        }
        private static void AssertVector(Vector3 actual, Vector3 expected, float tolerance, string message)
        { Assert.That(Vector3.Distance(actual, expected), Is.LessThan(tolerance), message + " " + actual + " vs " + expected); }

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
                string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "output", "previews"));
                Directory.CreateDirectory(directory); File.WriteAllBytes(Path.Combine(directory, file), image.EncodeToPNG());
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
