using System.Collections;
using System.IO;
using NUnit.Framework;
using TetherRush.Core;
using TetherRush.Course;
using TetherRush.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TetherRush.Tests
{
    public sealed class CoinPresentationTests
    {
        private GameObject root;
        private TetherRushApp app;
        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }
        private void Create()
        {
            root = new GameObject("Coin acceptance fixture"); root.SetActive(false);
            app = root.AddComponent<TetherRushApp>(); app.ConfigureSession(new PlayerProgress()); root.SetActive(true);
            app.PlayLevel(0); app.Game.AutoSimulate = false;
        }
        [Test]
        public void WorldUsesCoinFbxAndHudUsesTheMatchingNonBlockingSprite()
        {
            Create(); var prefab = Resources.Load<GameObject>("ArtModels/Coin"); Assert.That(prefab, Is.Not.Null);
            var mesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
            bool found = false;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
                if (filter.sharedMesh == mesh) { found = true; Assert.That(filter.GetComponent<Renderer>().sharedMaterials.Length, Is.EqualTo(3)); }
            Assert.That(found, Is.True);
            var sprite = UiResources.CoinSprite; Assert.That(sprite, Is.Not.Null);
            var icon = FindCoinIcon(); Assert.That(icon.sprite, Is.SameAs(sprite));
            Assert.That(icon.raycastTarget, Is.False); Assert.That(icon.preserveAspect, Is.True);
        }
        [Test]
        public void CoinIdlePoseFreezesDuringPauseAndCollectionKeepsBonusScoring()
        {
            Create(); var game = app.Game; var course = LevelCatalog.Get(0); int cue = 0;
            Advance(course, ref cue, 35);
            var prefab = Resources.Load<GameObject>("ArtModels/Coin"); var mesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
            Transform visual = null;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>()) if (filter.sharedMesh == mesh) { visual = filter.transform.parent; break; }
            Assert.That(visual, Is.Not.Null);
            var position = visual.localPosition; var rotation = visual.localRotation;
            game.PauseRun(); game.SimulateStep(2);
            Assert.That(visual.localPosition, Is.EqualTo(position)); Assert.That(visual.localRotation, Is.EqualTo(rotation));
            game.ResumeRun(); game.SimulateStep(1f / 120, press: true);
            Advance(course, ref cue, 40); Assert.That(game.Model.Bonus, Is.EqualTo(2));
            int bonus = game.Model.Bonus; game.SimulateStep(0); Assert.That(game.Model.Bonus, Is.EqualTo(bonus));
            Assert.That(game.Score, Is.EqualTo(PlayerSaveStore.CalculateScore(game.Model.Distance, 2)));
        }
        [UnityTest]
        public IEnumerator CaptureRealCoinAndHudAndCompletedMapCounter()
        {
            Create(); int cue = 0; var course = LevelCatalog.Get(0); Advance(course, ref cue, 35);
            yield return Capture("AudioCoin_Game.png");
            Advance(course, ref cue, course.Length + 1);
            Assert.That(app.Game.Model.Phase, Is.EqualTo(RunPhase.Completed));
            Assert.That(app.Progress.BestBonus(0), Is.EqualTo(2));
            app.ShowLevels(); app.Menu.FocusLevel(0); yield return null;
            bool matched = false;
            foreach (var image in root.GetComponentsInChildren<Image>())
                if (image.name == "Focused bonus coin") { matched = true; Assert.That(image.sprite, Is.SameAs(UiResources.CoinSprite)); }
            Assert.That(matched, Is.True); yield return Capture("AudioCoin_Map.png");
        }
        private Image FindCoinIcon()
        {
            foreach (var image in root.GetComponentsInChildren<Image>()) if (image.name == "Coin") return image;
            Assert.Fail("Missing coin HUD image"); return null;
        }
        private void Advance(CourseDefinition course, ref int cue, float z)
        {
            for (int n = 0; n < 8000 && app.Game.Model.Distance < z && app.Game.Model.Phase == RunPhase.Running; n++)
            {
                bool press = false, release = false;
                if (cue < course.SafeRoute.Length && app.Game.Model.Distance >= course.SafeRoute[cue].Distance)
                { press = course.SafeRoute[cue].Press; release = !press; cue++; }
                app.Game.SimulateStep(1f / 120, press, release);
            }
            Assert.That(app.Game.Model.Phase == RunPhase.Completed || app.Game.Model.Distance >= z, Is.True);
        }
        private IEnumerator Capture(string name)
        {
            yield return null;
            Camera camera = app.Game != null ? app.Game.GetComponentInChildren<Camera>() : root.GetComponentInChildren<Camera>();
            Canvas canvas = app.Game != null ? app.Game.GetComponentInChildren<Canvas>() : app.Menu.Canvas;
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
                string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "output", "previews"));
                Directory.CreateDirectory(folder); File.WriteAllBytes(Path.Combine(folder, name), image.EncodeToPNG());
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
