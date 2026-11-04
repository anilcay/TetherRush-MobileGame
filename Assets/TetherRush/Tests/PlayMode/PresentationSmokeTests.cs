using System.Collections;
using System.IO;
using NUnit.Framework;
using TetherRush.Art;
using TetherRush.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TetherRush.Tests
{
    /// <summary>Production FBX, feedback isolation and actual URP-camera preview checks.</summary>
    public sealed class PresentationSmokeTests
    {
        private const float Step = 1f / 60f;
        private GameObject root;
        private TetherRushGame game;

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
        }

        private void CreateGame(bool challenge = false)
        {
            if (root != null) Object.DestroyImmediate(root);
            root = new GameObject("Presentation smoke fixture");
            root.SetActive(false);
            game = root.AddComponent<TetherRushGame>();
            game.ConfigureCourse(challenge);
            root.SetActive(true);
            game.AutoSimulate = false;
            game.Feedback.AudioEnabled = false;
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Ready));
            Assert.That(game.IsAdvancedCourse, Is.EqualTo(challenge));
        }

        [Test]
        public void AllEighteenBlenderModelsHaveCenteredUnitBoundsAndSupportedShaders()
        {
            var kinds = (VisualKind[])System.Enum.GetValues(typeof(VisualKind));
            Assert.That(kinds.Length, Is.EqualTo(18));
            foreach (var kind in kinds)
            {
                var prefab = Resources.Load<GameObject>("ArtModels/" + kind);
                Assert.That(prefab, Is.Not.Null, "Missing real Blender FBX: " + kind);
                var instance = Object.Instantiate(prefab);
                try
                {
                    AssertVector(instance.transform.position, Vector3.zero, kind + " pivot");
                    Assert.That(instance.GetComponentsInChildren<Collider>(true), Is.Empty, kind + " must not import colliders");
                    var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                    Assert.That(renderers.Length, Is.GreaterThan(0), kind + " mesh renderer");
                    Bounds bounds = renderers[0].bounds;
                    foreach (var renderer in renderers)
                    {
                        var filter = renderer.GetComponent<MeshFilter>();
                        Assert.That(filter, Is.Not.Null, kind + " MeshFilter");
                        Assert.That(filter.sharedMesh, Is.Not.Null, kind + " mesh");
                        Assert.That(filter.sharedMesh.vertexCount, Is.GreaterThan(0), kind + " vertices");
                        bounds.Encapsulate(renderer.bounds);
                    }
                    AssertVector(bounds.center, Vector3.zero, kind + " bounds center", .02f);
                    AssertVector(bounds.size, Vector3.one, kind + " normalized size", .02f);
                }
                finally { Object.DestroyImmediate(instance); }
            }
            foreach (string label in new[] { "Tether Rush/NeonSurface", "Tether Rush/NeonEffect", "Tether Rush/NeonGlow", "Tether Rush/FacilitySky", "Tether Rush/SoftShadow" })
            {
                var shader = Shader.Find(label);
                Assert.That(shader, Is.Not.Null, label);
                Assert.That(shader.isSupported, Is.True, label + " is unsupported by this graphics device");
            }
        }

        [Test]
        public void CoreColorsRemainDistinctFromBeamInNormalAndFullEnergyStates()
        {
            CreateGame();
            var beam = FindTether().GetComponent<LineRenderer>();
            foreach (bool ready in new[] { false, true })
            {
                game.Feedback.Tick(.1f, true, ready, RunPhase.Ready);
                var properties = new MaterialPropertyBlock(); beam.GetPropertyBlock(properties);
                Color beamColor = properties.GetColor("_BaseColor");
                Assert.That(beamColor.g, Is.GreaterThan(beamColor.r), "Beam stays in the cool cyan family");
                foreach (Transform core in new[] { game.LeftCore, game.RightCore })
                {
                    bool found = false;
                    foreach (var renderer in core.GetComponentsInChildren<MeshRenderer>())
                        foreach (var material in renderer.sharedMaterials)
                            if (material.name == "Core A" || material.name == "Core B")
                            {
                                Color color = material.GetColor("_BaseColor");
                                float separation = new Vector3(color.r - beamColor.r, color.g - beamColor.g, color.b - beamColor.b).magnitude;
                                Assert.That(separation, Is.GreaterThan(.3f), "Core body must remain visually separate from tether");
                                found = true;
                            }
                    Assert.That(found, Is.True, "Actual Blender core has a body material");
                }
            }
        }

        [Test]
        public void FeedbackPreservesGameplayRootsFreezesWhilePausedAndClearsOnRetry()
        {
            CreateGame();
            Transform tether = FindTether();
            var leftPose = new Pose(game.LeftCore.position, game.LeftCore.rotation);
            var rightPose = new Pose(game.RightCore.position, game.RightCore.rotation);
            Vector3 tetherPosition = tether.position, tetherScale = tether.localScale;
            Quaternion tetherRotation = tether.rotation;
            game.Feedback.Tick(.2f, true, true, RunPhase.Ready);
            AssertPose(game.LeftCore, leftPose); AssertPose(game.RightCore, rightPose);
            AssertVector(tether.position, tetherPosition, "feedback tether root position");
            AssertVector(tether.localScale, tetherScale, "feedback tether root scale");
            Assert.That(Quaternion.Angle(tether.rotation, tetherRotation), Is.LessThan(.001f));
            Assert.That(game.Model.Distance, Is.Zero);

            game.StartRun(); game.SimulateStep(.2f);
            AssertGameplayPositions();
            ReachCharge(); AdvanceTo(96); game.SimulateStep(Step, release: true);
            game.SimulateStep(.12f);
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.ActivatedTargetCount, Is.EqualTo(1));
            AssertGameplayPositions();
            foreach (var echo in root.GetComponentsInChildren<MeshRenderer>())
                if (echo.name == "Target mesh echo" && echo.enabled)
                    Assert.That(echo.sharedMaterials.Length, Is.EqualTo(echo.GetComponent<MeshFilter>().sharedMesh.subMeshCount));
            LineRenderer ring = FindVisibleRing();
            Assert.That(ring, Is.Not.Null, "Real release should create a visible pooled Pulse ring");
            Vector3[] before = new Vector3[ring.positionCount]; ring.GetPositions(before);
            Color ringColor = ring.startColor;
            game.PauseRun();
            leftPose = new Pose(game.LeftCore.position, game.LeftCore.rotation);
            rightPose = new Pose(game.RightCore.position, game.RightCore.rotation);
            float distance = game.Model.Distance;
            for (int i = 0; i < 5; i++) game.SimulateStep(.2f);
            Assert.That(game.Model.Distance, Is.EqualTo(distance));
            AssertPose(game.LeftCore, leftPose); AssertPose(game.RightCore, rightPose);
            Assert.That(ring.enabled, Is.True);
            Assert.That(ring.startColor, Is.EqualTo(ringColor));
            for (int i = 0; i < before.Length; i++) AssertVector(ring.GetPosition(i), before[i], "paused ring");
            var sources = root.GetComponentsInChildren<AudioSource>(true);
            Assert.That(sources.Length, Is.EqualTo(1), "One shared feedback voice");
            game.Feedback.AudioEnabled = true;
            Assert.That(sources[0].mute, Is.False);
            game.Feedback.AudioEnabled = false;
            Assert.That(sources[0].mute, Is.True);
            game.RestartRun();
            Assert.That(sources[0].isPlaying, Is.False, "Retry stops an in-flight cue");
            Assert.That(FindVisibleRing(), Is.Null, "Retry clears transient rings");
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.name == "Target mesh echo") Assert.That(renderer.enabled, Is.False, "Retry clears mesh echoes");
            AssertGameplayPositions();
            foreach (var wake in root.GetComponentsInChildren<LineRenderer>(true))
                if (wake.name == "Core wake")
                {
                    Vector3 start = wake.GetPosition(0);
                    Assert.That(Mathf.Abs(start.z - game.Model.Distance), Is.LessThan(.001f), "Retry wake starts at the new core pose");
                    for (int i = 1; i < wake.positionCount; i++) AssertVector(wake.GetPosition(i), start, "Retry clears the old wake path");
                }
        }

        [UnityTest]
        public IEnumerator RenderActualReadyFoundationPulseAndChallengePreviews()
        {
            CreateGame();
            yield return null;
            Assert.That(GraphicsSettings.currentRenderPipeline, Is.InstanceOf<UniversalRenderPipelineAsset>(), "Preview requires actual configured URP");
            Assert.That(RenderPipeline.SupportsRenderRequest(root.GetComponentInChildren<Camera>(), new UniversalRenderPipeline.SingleCameraRequest()),
                Is.True, "Initialize and validate the actual render pipeline before offscreen requests");
            Assert.That(RenderPipelineManager.currentPipeline, Is.InstanceOf<UniversalRenderPipeline>());
            game.SimulateStep(Step);
            yield return Capture("UIRefresh_Legacy_Ready.png");
            game.StartRun(); ReachCharge(); AdvanceTo(90);
            Assert.That(game.Model.Energy, Is.EqualTo(game.Model.Tuning.EnergyCapacity));
            yield return Capture("UIRefresh_Legacy_Foundation.png");
            AdvanceTo(96); game.SimulateStep(Step, release: true); game.SimulateStep(.12f);
            Assert.That(game.Model.PulseCount, Is.EqualTo(1));
            Assert.That(game.ActivatedTargetCount, Is.EqualTo(1));
            Assert.That(FindVisibleRing(), Is.Not.Null);
            yield return Capture("UIRefresh_Legacy_Pulse.png");

            CreateGame(true); yield return null;
            game.StartRun(); AdvanceTo(25); game.SimulateStep(Step, press: true);
            AdvanceTo(55); game.SimulateStep(Step, release: true);
            AdvanceTo(85); game.SimulateStep(Step, press: true); AdvanceTo(88);
            Assert.That(game.TimedHazardCount, Is.EqualTo(3));
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Running));
            yield return Capture("UIRefresh_Legacy_Challenge.png");
            CreateGame(); yield return null;
            game.StartRun(); game.SimulateStep(Step, press: true);
            for (int i = 0; i < 600 && game.Model.Phase == RunPhase.Running; i++) game.SimulateStep(Step);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
            yield return Capture("UIRefresh_Legacy_Failed.png");
        }

        private IEnumerator Capture(string fileName)
        {
            var camera = root.GetComponentInChildren<Camera>();
            var canvas = root.GetComponentInChildren<Canvas>();
            var scaler = canvas.GetComponent<CanvasScaler>();
            Assert.That(camera, Is.Not.Null); Assert.That(canvas, Is.Not.Null);
            RenderTexture previousTarget = camera.targetTexture, previousActive = RenderTexture.active;
            RenderMode previousMode = canvas.renderMode;
            Camera previousCanvasCamera = canvas.worldCamera;
            float previousPlane = canvas.planeDistance, previousScale = canvas.scaleFactor;
            Rect previousRect = camera.rect;
            bool previousScalerEnabled = scaler != null && scaler.enabled;
            var target = new RenderTexture(540, 960, 24, RenderTextureFormat.ARGB32) { name = "Polish actual game preview" };
            Texture2D texture = null;
            try
            {
                target.Create(); camera.targetTexture = target; camera.rect = new Rect(0, 0, 1, 1);
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                if (scaler != null) scaler.enabled = false;
                canvas.scaleFactor = 1;
                Canvas.ForceUpdateCanvases();
                yield return null; // No EndOfFrame dependency: this also works in batch mode.
                Canvas.ForceUpdateCanvases();
                Assert.That(camera.pixelWidth, Is.EqualTo(540)); Assert.That(camera.pixelHeight, Is.EqualTo(960));
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target;
                texture = new Texture2D(540, 960, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); texture.Apply();
                var pixels = texture.GetPixels32(); int nonBackground = 0;
                Color32 first = pixels[0];
                for (int i = 0; i < pixels.Length; i += 17)
                    if (Mathf.Abs(pixels[i].r - first.r) + Mathf.Abs(pixels[i].g - first.g) + Mathf.Abs(pixels[i].b - first.b) > 18) nonBackground++;
                Assert.That(nonBackground, Is.GreaterThan(100), "Render readback must contain scene/UI content");
                string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "output", "previews"));
                Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, fileName); File.WriteAllBytes(path, texture.EncodeToPNG());
                Assert.That(new FileInfo(path).Length, Is.GreaterThan(3000));
                TestContext.WriteLine("Actual URP game capture: " + path);
            }
            finally
            {
                camera.targetTexture = previousTarget; camera.rect = previousRect;
                canvas.renderMode = previousMode; canvas.worldCamera = previousCanvasCamera;
                canvas.planeDistance = previousPlane; canvas.scaleFactor = previousScale;
                if (scaler != null) scaler.enabled = previousScalerEnabled;
                RenderTexture.active = previousActive;
                if (texture != null) Object.DestroyImmediate(texture);
                target.Release(); Object.DestroyImmediate(target);
                Canvas.ForceUpdateCanvases();
            }
        }

        private Transform FindTether()
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>()) if (child.name == "Energy tether") return child;
            Assert.Fail("Game did not create its tether root"); return null;
        }
        private LineRenderer FindVisibleRing()
        {
            foreach (var line in root.GetComponentsInChildren<LineRenderer>(true))
                if (line.name == "Pulse / feedback ring" && line.enabled) return line;
            return null;
        }
        private void ReachCharge()
        {
            AdvanceTo(30); game.SimulateStep(Step, press: true); AdvanceTo(86);
            Assert.That(game.Model.Energy, Is.EqualTo(game.Model.Tuning.EnergyCapacity));
        }
        private void AdvanceTo(float distance)
        {
            for (int i = 0; i < 2400 && game.Model.Distance < distance && game.Model.Phase == RunPhase.Running; i++) game.SimulateStep(Step);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Running), game.Model.FailureReason);
            Assert.That(game.Model.Distance, Is.GreaterThanOrEqualTo(distance));
        }
        private void AssertGameplayPositions()
        {
            AssertVector(game.LeftCore.position, new Vector3(-game.Model.HalfSeparation, .55f, game.Model.Distance), "left simulation root");
            AssertVector(game.RightCore.position, new Vector3(game.Model.HalfSeparation, .55f, game.Model.Distance), "right simulation root");
            var line = FindTether().GetComponent<LineRenderer>();
            Assert.That(line, Is.Not.Null);
            AssertVector(line.GetPosition(0), game.LeftCore.position, "tether left endpoint");
            AssertVector(line.GetPosition(1), game.RightCore.position, "tether right endpoint");
        }
        private static void AssertPose(Transform transform, Pose pose)
        {
            AssertVector(transform.position, pose.position, transform.name + " position");
            Assert.That(Quaternion.Angle(transform.rotation, pose.rotation), Is.LessThan(.001f), transform.name + " rotation");
        }
        private static void AssertVector(Vector3 actual, Vector3 expected, string label, float tolerance = .001f)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(tolerance), label + " X");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(tolerance), label + " Y");
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(tolerance), label + " Z");
        }
    }
}
