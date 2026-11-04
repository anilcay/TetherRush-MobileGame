using System.Collections;
using NUnit.Framework;
using Unity.Profiling;
using TetherRush.Core;
using TetherRush.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TetherRush.Tests
{
    public sealed class OptimizationUiTests
    {
        private GameObject root;
        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }

        [Test]
        public void UnchangedHudStatesAllocateNothingAndChangedValuesRemainFresh()
        {
            root = new GameObject("Optimized HUD fixture");
            var hud = root.AddComponent<GameHud>(); hud.Initialize(false, null, null, null, null);
            var state = new HudState
            {
                Phase = RunPhase.Running, Endless = true, Score = 1234, BestScore = 5678,
                PaceMultiplier = 1.2f, Bonus = 3, EnergyCapacity = 5, TeachControls = true,
                Hint = "ÖĞRETİCİ • Basılı tut"
            };
            AssertStableAllocation(hud, state);
            Assert.That(FindText("Instruction").text, Is.EqualTo("Basılı tut"));
            Assert.That(FindText("Score").text, Is.EqualTo(state.Score.ToString("N0")));
            state.Score++; state.Bonus++; state.BestScore++; state.PaceMultiplier = 1.4f;
            hud.Present(state, 0);
            Assert.That(FindText("Score").text, Is.EqualTo(state.Score.ToString("N0")));
            Assert.That(FindText("Bonus").text, Is.EqualTo("4"));
            Assert.That(FindText("Best").text, Is.EqualTo("REKOR  " + state.BestScore.ToString("N0") + "  ·  " + state.PaceMultiplier.ToString("0.0") + "×"));
            state.Phase = RunPhase.Failed; state.FailureReason = "Temas";
            AssertStableAllocation(hud, state);
            Assert.That(FindText("State number").text, Is.EqualTo(state.Score.ToString("N0")));
            Assert.That(FindText("State explanation").text, Does.EndWith("\nTemas"));
            state.Phase = RunPhase.Paused; AssertStableAllocation(hud, state);
            state.Endless = false; state.Phase = RunPhase.Completed; AssertStableAllocation(hud, state);
            state.Phase = RunPhase.Ready; AssertStableAllocation(hud, state);
        }

        private static void AssertStableAllocation(GameHud hud, HudState state)
        {
            for (int i = 0; i < 10; i++) hud.Present(state, 0);
            using (var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "GC.Alloc", 8192,
                ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
            {
                System.GC.KeepAlive(new byte[4096]); recorder.Stop();
                Assert.That(recorder.Valid, Is.True);
                Assert.That(recorder.Count, Is.GreaterThanOrEqualTo(1), "GC recorder must detect a known allocation");
                recorder.Reset(); recorder.Start();
                for (int i = 0; i < 100; i++) hud.Present(state, 0);
                recorder.Stop();
                Assert.That(recorder.Valid, Is.True);
                Assert.That(recorder.Count, Is.LessThan(8192), "Allocation sample buffer must not overflow");
                // GC.Alloc is a marker: Count is allocation events; Value is duration, not bytes.
                Assert.That(recorder.Count, Is.Zero, state.Phase.ToString());
            }
        }

        [UnityTest]
        public IEnumerator StaticPreviewStopsRenderingAndRecoversLostTextureAndVisibility()
        {
            root = new GameObject("Optimized menu fixture"); root.SetActive(false);
            var app = root.AddComponent<TetherRushApp>(); app.ConfigureSession(new PlayerProgress()); root.SetActive(true);
            var image = root.GetComponentInChildren<RawImage>();
            var texture = (RenderTexture)image.texture;
            Camera preview = null;
            foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                if (camera.name == "Menu preview camera") preview = camera;
            Assert.That(preview, Is.Not.Null); Assert.That(preview.enabled, Is.False);
            int renders = 0;
            System.Action<ScriptableRenderContext, Camera> onCamera = (_, camera) => { if (camera == preview) renders++; };
            RenderPipelineManager.beginCameraRendering += onCamera;
            try
            {
                for (int i = 0; i < 12 && !image.enabled; i++) yield return null;
                Assert.That(image.enabled, Is.True, "Only expose the preview after both warm passes");
                Assert.That(texture.IsCreated(), Is.True);
                AssertPreviewHasGeometry(texture);
                int warmed = renders;
                for (int i = 0; i < 30; i++) yield return null;
                Assert.That(renders - warmed, Is.Zero, "Static home must not submit idle preview renders");
                texture.Release();
                for (int i = 0; i < 4; i++) yield return null;
                Assert.That(texture.IsCreated() && image.enabled, Is.True);
                Assert.That(image.texture, Is.SameAs(texture));
                Assert.That(renders - warmed, Is.EqualTo(2));
                AssertPreviewHasGeometry(texture);
                app.ShowSettings(); Assert.That(preview.gameObject.activeInHierarchy, Is.False);
                app.ShowHome();
                for (int i = 0; i < 3; i++) yield return null;
                root.SetActive(false); root.SetActive(true); app.ShowHome();
                for (int i = 0; i < 3; i++) yield return null;
                Assert.That(preview.enabled, Is.False);
                Assert.That(preview.gameObject.activeInHierarchy && image.enabled, Is.True);
                Assert.That(image.texture, Is.SameAs(texture));
                Assert.That(texture.width, Is.EqualTo(768)); Assert.That(texture.height, Is.EqualTo(480));
            }
            finally { RenderPipelineManager.beginCameraRendering -= onCamera; }
            Object.DestroyImmediate(root); root = null;
            yield return null; // MenuHero releases immediately, then destroys its owned texture at frame end.
            Assert.That(texture == null, Is.True, "Preview owns and destroys its sole render target");
        }

        private static void AssertPreviewHasGeometry(RenderTexture texture)
        {
            var previous = RenderTexture.active;
            var pixels = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = texture;
                pixels.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0); pixels.Apply();
                float min = 1, max = 0;
                for (int y = 20; y < texture.height; y += 40)
                    for (int x = 20; x < texture.width; x += 40)
                    {
                        float brightness = pixels.GetPixel(x, y).grayscale;
                        min = Mathf.Min(min, brightness); max = Mathf.Max(max, brightness);
                    }
                Assert.That(max - min, Is.GreaterThan(.05f), "Initialized preview must contain visible geometry");
            }
            finally { RenderTexture.active = previous; Object.DestroyImmediate(pixels); }
        }

        private Text FindText(string name)
        {
            foreach (var text in root.GetComponentsInChildren<Text>(true)) if (text.name == name) return text;
            Assert.Fail("Missing " + name); return null;
        }
    }
}
