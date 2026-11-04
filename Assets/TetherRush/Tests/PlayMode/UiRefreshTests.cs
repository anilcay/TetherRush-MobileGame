using NUnit.Framework;
using TetherRush.Core;
using TetherRush.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace TetherRush.Tests
{
    public sealed class UiRefreshTests
    {
        private GameObject root;
        [TearDown] public void Cleanup() { if (root != null) Object.DestroyImmediate(root); }

        [Test]
        public void LicensedFontsContainTurkishAndSpritesHaveCorrectBorders()
        {
            Assert.That(UiResources.Body.name, Is.EqualTo("Barlow-SemiBold"));
            Assert.That(UiResources.Display.name, Is.EqualTo("BarlowCondensed-ExtraBold"));
            foreach (char c in "ÇĞİÖŞÜçğıöşü")
            {
                Assert.That(UiResources.Body.HasCharacter(c), Is.True, "Body " + c);
                Assert.That(UiResources.Display.HasCharacter(c), Is.True, "Display " + c);
            }
            Assert.That(UiResources.PanelSprite.border, Is.EqualTo(Vector4.one * 8));
            Assert.That(UiResources.CircleSprite.border, Is.EqualTo(Vector4.zero));
        }

        [Test]
        public void LateLevelHidesOrdinaryCoachButAlwaysShowsPulseRange()
        {
            root = new GameObject("HUD UI acceptance"); var hud = root.AddComponent<GameHud>();
            hud.Initialize(false, null, null, null, null);
            var state = new HudState { Phase = RunPhase.Running, EnergyCapacity = 5, Hint = "Basılı tut", TeachControls = false };
            hud.Present(state, .1f);
            var instruction = FindText("Instruction");
            Assert.That(instruction.gameObject.activeSelf, Is.False);
            state.Hint = "MENZİLDE • Şimdi BIRAK!"; hud.Present(state, 10);
            Assert.That(instruction.gameObject.activeSelf, Is.True);
            Assert.That(instruction.text, Is.EqualTo("MENZİLDE · BIRAK"));
            state.Hint = "Dar geçit"; state.TeachControls = true; hud.Present(state, .1f);
            Assert.That(instruction.gameObject.activeSelf, Is.True);
            hud.Present(state, 3); Assert.That(instruction.gameObject.activeSelf, Is.False);
            state.Phase = RunPhase.Paused; state.CanGoHome = true;
            hud.Present(state, 0); Canvas.ForceUpdateCanvases();
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                Assert.That(button.name, Does.Not.Contain("Pulse"));
                if (button.gameObject.activeInHierarchy)
                    Assert.That(((RectTransform)button.transform).rect.height, Is.GreaterThanOrEqualTo(72));
            }
        }

        [Test]
        public void MenuUsesRealBlenderModelsAndDisablesPreviewOutsideHome()
        {
            root = new GameObject("Menu UI acceptance"); root.SetActive(false);
            var app = root.AddComponent<TetherRushApp>(); app.ConfigureSession(new PlayerProgress()); root.SetActive(true);
            var hero = root.GetComponent<MenuHero>();
            Assert.That(hero, Is.Not.Null);
            var image = root.GetComponentInChildren<RawImage>();
            Assert.That(image.texture, Is.TypeOf<RenderTexture>());
            var meshes = root.GetComponentsInChildren<MeshRenderer>();
            Assert.That(meshes.Length, Is.GreaterThanOrEqualTo(3));
            foreach (var mesh in meshes)
                Assert.That(mesh.bounds.center.x, Is.InRange(497f, 503f), "Imported bounds must follow the isolated stage");
            var preview = FindPreviewCamera();
            Assert.That(preview.gameObject.activeInHierarchy, Is.True);
            Assert.That(preview.enabled, Is.False, "Static preview is rendered on demand");
            app.ShowSettings(); Assert.That(preview.gameObject.activeInHierarchy, Is.False);
            app.ShowHome(); Assert.That(preview.gameObject.activeInHierarchy, Is.True);
            app.PlayLevel(0); app.Game.AutoSimulate = false;
            Assert.That(preview.gameObject.activeInHierarchy, Is.False);
        }
        private Text FindText(string name)
        {
            foreach (var text in root.GetComponentsInChildren<Text>(true)) if (text.name == name) return text;
            Assert.Fail("Missing text " + name); return null;
        }
        private Camera FindPreviewCamera()
        {
            foreach (var camera in root.GetComponentsInChildren<Camera>(true)) if (camera.name == "Menu preview camera") return camera;
            Assert.Fail("Missing preview camera"); return null;
        }
    }
}
