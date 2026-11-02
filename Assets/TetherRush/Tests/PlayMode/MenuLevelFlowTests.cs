using System.Collections;
using System.IO;
using NUnit.Framework;
using TetherRush.Core;
using TetherRush.Course;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TetherRush.Tests
{
    public sealed class MenuLevelFlowTests
    {
        private GameObject root;
        private TetherRushApp app;
        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }
        private void CreateApp()
        {
            root = new GameObject("Menu flow fixture"); root.SetActive(false);
            app = root.AddComponent<TetherRushApp>(); app.ConfigureSession(new PlayerProgress()); root.SetActive(true);
        }
        private static void StepRoute(TetherRushGame game, CourseDefinition course, ref int cue, float step = 1f / 60)
        {
            bool press = false, release = false;
            if (cue < course.SafeRoute.Length && game.Model.Distance >= course.SafeRoute[cue].Distance)
            { press = course.SafeRoute[cue].Press; release = !press; cue++; }
            game.SimulateStep(step, press, release);
        }
        private static System.Collections.Generic.IEnumerable<int> LevelIndices()
        { for (int i = 0; i < LevelCatalog.Count; i++) yield return i; }
        [TestCaseSource(nameof(LevelIndices))]
        public void AuthoredLevelCanCompleteAndPersistsOnlyRealCompletion(int index)
        {
            CreateApp(); for (int i = 0; i < index; i++) app.Progress.CompleteLevel(i, 0); app.PlayLevel(index); var game = app.Game; game.AutoSimulate = false;
            Assert.That(app.Progress.IsCompleted(index), Is.False);
            var course = LevelCatalog.Get(index); int cue = 0;
            for (int i = 0; i < 4000 && game.Model.Phase == RunPhase.Running; i++) StepRoute(game, course, ref cue);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Completed), game.Model.FailureReason + " at " + game.Model.Distance);
            Assert.That(app.Progress.IsCompleted(index), Is.True);
            Assert.That(app.Progress.BestBonus(index), Is.EqualTo(game.Model.Bonus));
            if (index == 2 || index == 4) Assert.That(game.ActivatedTargetCount, Is.EqualTo(1));
            if (index == 0)
            {
                FindButton("Other course").onClick.Invoke();
                Assert.That(app.Game.SelectedLevelIndex, Is.EqualTo(1));
                Assert.That(app.Game.Model.Phase, Is.EqualTo(RunPhase.Running));
                app.Game.AutoSimulate = false;
                Assert.That(Shader.GetGlobalFloat("_TRFogEnd"), Is.EqualTo(85));
            }
            app.ShowLevels(); Assert.That(app.Game, Is.Null); Assert.That(app.Page, Is.EqualTo(MenuPage.Levels));
        }
        [Test]
        public void RealMenuButtonsSettingsAndPausedHomeUseOneEventSystemAndNoPulse()
        {
            CreateApp(); Assert.That(app.Page, Is.EqualTo(MenuPage.Home));
            FindButton("Levels").onClick.Invoke(); Assert.That(app.Page, Is.EqualTo(MenuPage.Levels));
            app.Progress.CompleteLevel(0, 0); app.Progress.CompleteLevel(1, 0); app.ShowLevels();
            FindButton("Level 3").onClick.Invoke(); Assert.That(app.Game.SelectedLevelIndex, Is.EqualTo(2));
            var game = app.Game; game.AutoSimulate = false; game.SimulateStep(.1f, press: true);
            for (int i = 0; i < 5; i++) game.Model.CollectEnergy(1000 + i);
            game.PauseRun(); Assert.That(game.Model.PulseCount, Is.Zero);
            FindButton("Main menu").onClick.Invoke(); Assert.That(app.Page, Is.EqualTo(MenuPage.Levels));
            Assert.That(game.Model.PulseCount, Is.Zero); Assert.That(app.Progress.IsCompleted(2), Is.False);
            app.ShowSettings();
            root.GetComponentInChildren<Toggle>().isOn = false;
            root.GetComponentInChildren<Slider>().value = .23f;
            Assert.That(app.Progress.AudioEnabled, Is.False); Assert.That(app.Progress.Volume, Is.EqualTo(.23f).Within(.001));
            app.PlayLevel(0); Assert.That(app.Game.Feedback.AudioEnabled, Is.False);
            Assert.That(app.Game.Feedback.Volume, Is.EqualTo(.23f).Within(.001));
            Assert.That(root.GetComponentsInChildren<EventSystem>(true).Length, Is.EqualTo(1));
        }
        private Button FindButton(string name)
        {
            foreach (var button in root.GetComponentsInChildren<Button>()) if (button.name == name) return button;
            Assert.Fail("Visible button missing: " + name); return null;
        }
        [UnityTest]
        public IEnumerator EndlessCrossesFifteenChunksWithBoundedWorldAndRetryResets()
        {
            CreateApp(); app.StartEndless(); var game = app.Game; game.AutoSimulate = false;
            int index = 0, cue = 0; var course = EndlessCourse.CreateChunk(index);
            while (game.Model.Distance < 15 * EndlessCourse.ChunkLength && game.Model.Phase == RunPhase.Running)
            {
                if (game.Model.Distance >= (index + 1) * EndlessCourse.ChunkLength)
                {
                    index++; course = EndlessCourse.CreateChunk(index); cue = 0; yield return null;
                    Assert.That(game.ActiveChunkCount, Is.LessThanOrEqualTo(4));
                    Assert.That(game.Model.TrackedCollectibleCount, Is.LessThan(80));
                    Assert.That(game.GetComponentsInChildren<MeshRenderer>(true).Length, Is.LessThan(350));
                }
                StepRoute(game, course, ref cue, 1f / 30);
            }
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Running), game.Model.FailureReason + " at " + game.Model.Distance);
            Assert.That(game.Model.Distance, Is.GreaterThan(1600));
            game.PauseRun(); float distance = game.Model.Distance;
            game.SimulateStep(2); Assert.That(game.Model.Distance, Is.EqualTo(distance)); game.ResumeRun();
            game.Model.CancelInput();
            for (int i = 0; i < 1800 && game.Model.Phase == RunPhase.Running; i++) game.SimulateStep(1f / 30);
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Failed));
            int record = game.Score; Assert.That(app.Progress.BestEndlessScore, Is.EqualTo(record));
            game.RestartRun(); yield return null;
            Assert.That(game.Model.Distance, Is.Zero); Assert.That(game.Score, Is.Zero);
            Assert.That(game.ActiveChunkCount, Is.EqualTo(2)); Assert.That(game.RemainingEnergyCount, Is.LessThanOrEqualTo(10));
            Assert.That(app.Progress.BestEndlessScore, Is.EqualTo(record));
        }
        [UnityTest]
        public IEnumerator CaptureActualMenuLevelsSettingsAndNewCourses()
        {
            CreateApp(); yield return null;
            yield return Capture("Map25_Menu_Home.png");
            app.ShowLevels(); yield return Capture("Map25_Menu_Levels.png");
            app.ShowEndless(); yield return Capture("Map25_Menu_Endless.png");
            app.ShowSettings(); yield return Capture("Map25_Menu_Settings.png");
            app.ShowHome(); yield return Capture("Map25_Menu_HomeTall.png", 1170);
            app.Progress.CompleteLevel(0, 0); app.Progress.CompleteLevel(1, 0);
            app.PlayLevel(2); app.Game.AutoSimulate = false; int cue = 0; var course = LevelCatalog.Get(2);
            while (app.Game.Model.Distance < 50 && app.Game.Model.Phase == RunPhase.Running) StepRoute(app.Game, course, ref cue);
            Assert.That(app.Game.Model.Phase, Is.EqualTo(RunPhase.Running));
            yield return Capture("Map25_Menu_Level3.png");
            app.Game.PauseRun(); yield return Capture("Map25_Menu_Paused.png");
            app.ShowHome(); yield return null; app.StartEndless(); app.Game.AutoSimulate = false;
            cue = 0; course = EndlessCourse.CreateChunk(0);
            while (app.Game.Model.Distance < 35 && app.Game.Model.Phase == RunPhase.Running) StepRoute(app.Game, course, ref cue);
            Assert.That(app.Game.Model.Phase, Is.EqualTo(RunPhase.Running));
            yield return Capture("Map25_Menu_EndlessRun.png");
        }
        private IEnumerator Capture(string file, int height = 960)
        {
            yield return null; // Settle activation and UI culling before changing canvas render mode.
            Camera camera = app.Game != null ? app.Game.GetComponentInChildren<Camera>() : root.GetComponentInChildren<Camera>();
            Canvas canvas = app.Game != null ? app.Game.GetComponentInChildren<Canvas>() : app.Menu.Canvas;
            Assert.That(RenderPipeline.SupportsRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest()), Is.True);
            var scaler = canvas.GetComponent<CanvasScaler>(); bool enabled = scaler.enabled;
            var target = new RenderTexture(540, height, 24, RenderTextureFormat.ARGB32);
            var oldActive = RenderTexture.active; var oldTarget = camera.targetTexture;
            var mode = canvas.renderMode; var oldCamera = canvas.worldCamera; float scale = canvas.scaleFactor, oldPlane = canvas.planeDistance;
            Texture2D image = null;
            try
            {
                target.Create(); camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera; canvas.planeDistance = 1; scaler.enabled = false; canvas.scaleFactor = 1;
                Canvas.ForceUpdateCanvases(); yield return null; Canvas.ForceUpdateCanvases();
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target; image = new Texture2D(540, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 540, height), 0, 0); image.Apply();
                string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "output", "previews"));
                Directory.CreateDirectory(directory); File.WriteAllBytes(Path.Combine(directory, file), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget; canvas.renderMode = mode; canvas.worldCamera = oldCamera; canvas.planeDistance = oldPlane;
                scaler.enabled = enabled; canvas.scaleFactor = scale; RenderTexture.active = oldActive;
                if (image != null) Object.DestroyImmediate(image); target.Release(); Object.DestroyImmediate(target);
            }
        }
    }
}
