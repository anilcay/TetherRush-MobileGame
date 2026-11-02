using System.Collections;
using System.IO;
using NUnit.Framework;
using TetherRush.Core;
using TetherRush.Course;
using TetherRush.Presentation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TetherRush.Tests
{
    public sealed class LevelMapFlowTests
    {
        private GameObject root;
        private TetherRushApp app;
        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }
        private void CreateApp(PlayerProgress profile = null)
        {
            root = new GameObject("25 level map fixture"); root.SetActive(false);
            app = root.AddComponent<TetherRushApp>(); app.ConfigureSession(profile ?? new PlayerProgress()); root.SetActive(true);
            app.ShowLevels();
        }
        private Button Node(int number)
        {
            foreach (var b in root.GetComponentsInChildren<Button>()) if (b.name == "Level " + number) return b;
            Assert.Fail("Missing numbered map node " + number); return null;
        }
        [Test]
        public void AllTwentyFiveNodesExistAndLockedRoutesCannotStart()
        {
            CreateApp(); Assert.That(app.Menu.MapNodeCount, Is.EqualTo(25));
            for (int n = 1; n <= 25; n++) Assert.That(Node(n).interactable, Is.EqualTo(n == 1), "node " + n);
            app.PlayLevel(24); Assert.That(app.Game, Is.Null); Assert.That(app.Page, Is.EqualTo(MenuPage.Levels));
            Node(25).onClick.Invoke(); Assert.That(app.Game, Is.Null, "Programmatic click cannot bypass app or map lock");
            app.Progress.CompleteLevel(0, 2); app.ShowLevels();
            Assert.That(Node(1).interactable, Is.True); Assert.That(Node(2).interactable, Is.True); Assert.That(Node(3).interactable, Is.False);
            Node(1).onClick.Invoke(); app.Game.AutoSimulate = false;
            Assert.That(app.Game.SelectedLevelIndex, Is.Zero, "Completed levels remain replayable");
        }
        [UnityTest]
        public IEnumerator DraggingTheMapDoesNotLaunchTheNodeAndTappingDoes()
        {
            var profile = new PlayerProgress(); for (int i = 0; i < 5; i++) profile.CompleteLevel(i, 0);
            CreateApp(profile); yield return null; Canvas.ForceUpdateCanvases(); app.Menu.FocusLevel(5);
            var scroll = root.GetComponentInChildren<ScrollRect>(); var button = (LevelMapNodeButton)Node(6);
            var p = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
            var data = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, position = p, pressPosition = p };
            float before = app.Menu.MapScrollPosition;
            scroll.OnInitializePotentialDrag(data); scroll.OnBeginDrag(data);
            data.position = p + Vector2.up * Mathf.Min(160, Screen.height * .2f); data.dragging = true;
            scroll.OnDrag(data); button.OnPointerClick(data); scroll.OnEndDrag(data);
            Assert.That(app.Game, Is.Null); Assert.That(app.Page, Is.EqualTo(MenuPage.Levels));
            Assert.That(Mathf.Abs(app.Menu.MapScrollPosition - before), Is.GreaterThan(.001f));
            data.dragging = false; button.OnPointerClick(data);
            Assert.That(app.Game, Is.Null, "Displaced release is rejected even after drag flag clears");
            data.position = data.pressPosition; button.OnPointerClick(data);
            Assert.That(app.Game, Is.Not.Null); app.Game.AutoSimulate = false;
            Assert.That(app.Game.SelectedLevelIndex, Is.EqualTo(5));
        }
        [UnityTest]
        public IEnumerator CompletionOpensTheNextNodeAndReturnRestoresItsMapFocus()
        {
            var profile = new PlayerProgress(); for (int i = 0; i < 11; i++) profile.CompleteLevel(i, 0);
            CreateApp(profile); app.PlayLevel(11); var game = app.Game; game.AutoSimulate = false;
            var course = LevelCatalog.Get(11); int cue = 0;
            for (int i = 0; i < 4000 && game.Model.Phase == RunPhase.Running; i++)
            {
                bool press = false, release = false;
                if (cue < course.SafeRoute.Length && game.Model.Distance >= course.SafeRoute[cue].Distance)
                { press = course.SafeRoute[cue].Press; release = !press; cue++; }
                game.SimulateStep(1f / 60, press, release);
            }
            Assert.That(game.Model.Phase, Is.EqualTo(RunPhase.Completed), game.Model.FailureReason);
            Assert.That(profile.IsUnlocked(12), Is.True); Assert.That(profile.IsUnlocked(13), Is.False);
            foreach (var button in root.GetComponentsInChildren<Button>())
                if (button.name == "Main menu") { button.onClick.Invoke(); break; }
            yield return null;
            Assert.That(app.Page, Is.EqualTo(MenuPage.Levels));
            Assert.That(Node(13).interactable, Is.True); Assert.That(Node(14).interactable, Is.False);
            Assert.That(app.Menu.MapScrollPosition, Is.InRange(.35f, .65f));
        }
        [UnityTest]
        public IEnumerator CaptureActualStartProgressEndAndTallLevelMaps()
        {
            CreateApp(); yield return null; yield return Capture("Map25_Start.png");
            app.Menu.FocusLevel(24); yield return Capture("Map25_EndLocked.png");
            app.ShowHome(); yield return Capture("Map25_Home.png");
            for (int i = 0; i < 12; i++) app.Progress.CompleteLevel(i, i % 5);
            app.ShowLevels(); yield return Capture("Map25_Progress.png");
            yield return Capture("Map25_ProgressTall.png", 1170);
            for (int i = 12; i < 25; i++) app.Progress.CompleteLevel(i, 2);
            app.ShowLevels(); yield return Capture("Map25_Complete.png");
            Assert.That(app.Menu.MapNodeCount, Is.EqualTo(25));
            app.Menu.FocusLevel(0); Assert.That(app.Menu.MapScrollPosition, Is.EqualTo(0).Within(.001));
            app.Menu.FocusLevel(24); Assert.That(app.Menu.MapScrollPosition, Is.EqualTo(1).Within(.001));
        }
        private IEnumerator Capture(string file, int height = 960)
        {
            yield return null;
            var camera = root.GetComponentInChildren<Camera>(); var canvas = app.Menu.Canvas;
            Assert.That(RenderPipeline.SupportsRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest()), Is.True);
            var scaler = canvas.GetComponent<CanvasScaler>(); bool wasEnabled = scaler.enabled;
            var target = new RenderTexture(540, height, 24, RenderTextureFormat.ARGB32);
            var oldActive = RenderTexture.active; var oldTarget = camera.targetTexture;
            var oldMode = canvas.renderMode; var oldCamera = canvas.worldCamera; float scale = canvas.scaleFactor, plane = canvas.planeDistance;
            Texture2D image = null;
            try
            {
                target.Create(); camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera; canvas.planeDistance = 1; scaler.enabled = false; canvas.scaleFactor = 1;
                Canvas.ForceUpdateCanvases(); yield return null; Canvas.ForceUpdateCanvases();
                // Warm the actual UI render once so dynamic font atlas/clipping updates have settled.
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                yield return null; Canvas.ForceUpdateCanvases();
                if (file.Contains("Progress"))
                {
                    var view = root.GetComponentInChildren<ScrollRect>().viewport;
                    var corners = new Vector3[4]; view.GetWorldCorners(corners);
                    float center = (camera.WorldToScreenPoint(corners[0]).y + camera.WorldToScreenPoint(corners[1]).y) * .5f;
                    Assert.That(camera.WorldToScreenPoint(Node(13).transform.position).y, Is.EqualTo(center).Within(3), "Focused node remains centered across portrait viewport sizes");
                }
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target; image = new Texture2D(540, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 540, height), 0, 0); image.Apply();
                string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "output", "previews"));
                Directory.CreateDirectory(directory); File.WriteAllBytes(Path.Combine(directory, file), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget; canvas.renderMode = oldMode; canvas.worldCamera = oldCamera; canvas.planeDistance = plane;
                scaler.enabled = wasEnabled; canvas.scaleFactor = scale; RenderTexture.active = oldActive;
                if (image != null) Object.DestroyImmediate(image); target.Release(); Object.DestroyImmediate(target);
            }
        }
    }
}
