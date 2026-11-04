using NUnit.Framework;
using TetherRush.Core;
using TetherRush.Presentation;
using UnityEngine;

namespace TetherRush.Tests
{
    public sealed class OptimizationPresentationTests
    {
        private GameObject root;
        private Transform left, right;
        private GameFeedback feedback;
        private LineRenderer tether;
        private Material surface, beam;
        private Mesh snapshotMesh;
        private const float Radius = 4;

        private void Create(string beamShader = "Tether Rush/NeonEffect")
        {
            root = new GameObject("Optimization presentation fixture");
            surface = new Material(Shader.Find("Tether Rush/NeonSurface"));
            beam = new Material(Shader.Find(beamShader));
            left = Core("Left", new Vector3(-1, .55f, 7));
            right = Core("Right", new Vector3(1, .55f, 7));
            var go = new GameObject("Test tether", typeof(LineRenderer));
            go.transform.SetParent(root.transform, false);
            tether = go.GetComponent<LineRenderer>(); tether.sharedMaterial = beam;
            feedback = root.AddComponent<GameFeedback>();
            feedback.Initialize(left, right, go.transform, null, beam, Radius);
            feedback.AudioEnabled = false;
        }

        private Transform Core(string name, Vector3 position)
        {
            var core = new GameObject(name); core.transform.SetParent(root.transform, false);
            core.transform.position = position;
            var visual = new GameObject("Visual", typeof(MeshRenderer));
            visual.transform.SetParent(core.transform, false);
            visual.GetComponent<MeshRenderer>().sharedMaterial = surface;
            return core.transform;
        }

        [TearDown]
        public void Cleanup()
        {
            if (root != null) Object.DestroyImmediate(root);
            if (surface != null) Object.DestroyImmediate(surface);
            if (beam != null) Object.DestroyImmediate(beam);
            if (snapshotMesh != null) Object.DestroyImmediate(snapshotMesh);
        }

        [Test]
        public void BulkHalosKeepAuthoredWorldGeometryAndSimulationRoots()
        {
            Create();
            left.position += new Vector3(-.4f, .2f, 2); right.position += new Vector3(.4f, .2f, 2);
            Vector3 leftPosition = left.position, rightPosition = right.position;
            const float elapsed = .125f;
            feedback.Tick(elapsed, true, true, RunPhase.Running);
            var halos = Lines("Core orbital halo");
            Assert.That(halos.Length, Is.EqualTo(2));
            for (int core = 0; core < 2; core++)
            {
                Vector3 center = core == 0 ? leftPosition : rightPosition;
                float radius = .35f + .012f * Mathf.Sin(elapsed * 3 + core * Mathf.PI);
                for (int p = 0; p < 48; p++)
                {
                    float angle = p * Mathf.PI * 2 / 48;
                    Vector3 expected = center + new Vector3(Mathf.Cos(angle), .08f * Mathf.Sin(angle + elapsed * .7f), Mathf.Sin(angle)) * radius;
                    AssertVector(halos[core].GetPosition(p), expected);
                }
            }
            AssertVector(left.position, leftPosition); AssertVector(right.position, rightPosition);
            Assert.That(left.rotation, Is.EqualTo(Quaternion.identity));
            Assert.That(right.rotation, Is.EqualTo(Quaternion.identity));
        }

        [Test]
        public void BulkPulseRingReachesGameplayRadiusAtSixtyFivePercentAndFreezesPaused()
        {
            Create();
            var center = new Vector3(.4f, .6f, 9);
            feedback.Pulse(center);
            feedback.Tick(.38f * .65f, false, false, RunPhase.Running);
            var ring = Lines("Pulse / feedback ring")[0];
            Assert.That(ring.enabled, Is.True);
            float size = Radius - ring.widthMultiplier * .5f;
            for (int p = 0; p < 64; p++)
            {
                float angle = p * Mathf.PI * 2 / 64;
                AssertVector(ring.GetPosition(p), center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * size);
            }
            Assert.That(Vector3.Distance(ring.GetPosition(0), center) + ring.widthMultiplier * .5f, Is.EqualTo(Radius).Within(.00001f));
            Vector3 before = ring.GetPosition(17); Color color = ring.startColor;
            feedback.Tick(.5f, false, false, RunPhase.Paused);
            AssertVector(ring.GetPosition(17), before); Assert.That(ring.startColor, Is.EqualTo(color));
        }

        [Test]
        public void StableTintsPreserveOtherPropertiesAndResetRestoresReadyTransitions()
        {
            Create();
            var core = left.GetComponentInChildren<MeshRenderer>();
            var block = new MaterialPropertyBlock();
            int marker = Shader.PropertyToID("_OptimizationFixtureMarker");
            core.GetPropertyBlock(block);
            block.SetFloat(marker, 37); core.SetPropertyBlock(block);
            tether.GetPropertyBlock(block);
            block.SetFloat(marker, 37); tether.SetPropertyBlock(block);
            Color authoredColor = surface.GetColor("_BaseColor");
            feedback.Tick(.1f, false, false, RunPhase.Running);
            feedback.Tick(.1f, false, false, RunPhase.Running);
            AssertTint(tether, VisualTheme.Tether);
            core.GetPropertyBlock(block);
            Assert.That(block.GetFloat(marker), Is.EqualTo(37));
            AssertColor(block.GetColor("_EmissionColor"), VisualTheme.CoreA * .13f);
            tether.GetPropertyBlock(block); Assert.That(block.GetFloat(marker), Is.EqualTo(37));
            feedback.Tick(.1f, true, true, RunPhase.Running);
            AssertTint(tether, VisualTheme.TetherReady);
            core.GetPropertyBlock(block); AssertColor(block.GetColor("_EmissionColor"), VisualTheme.CoreA * .19f);
            feedback.Pulse(Vector3.zero); feedback.Tick(.1f, false, false, RunPhase.Running);
            core.GetPropertyBlock(block); AssertColor(block.GetColor("_EmissionColor"), VisualTheme.CoreA * (.13f + .65f * .28f));
            feedback.ResetPresentation();
            Assert.That(tether.HasPropertyBlock(), Is.False, "Retry retains original tether override clearing behavior");
            core.GetPropertyBlock(block); AssertColor(block.GetColor("_EmissionColor"), VisualTheme.CoreA * .13f);
            Assert.That(block.GetFloat(marker), Is.EqualTo(37));
            feedback.Tick(.1f, false, false, RunPhase.Running);
            AssertTint(tether, VisualTheme.Tether);
            feedback.Tick(.1f, true, true, RunPhase.Running);
            AssertTint(tether, VisualTheme.TetherReady);
            Assert.That(core.sharedMaterial, Is.SameAs(surface));
            Assert.That(surface.GetColor("_BaseColor"), Is.EqualTo(authoredColor));
        }

        [TestCase("Tether Rush/NeonSurface")]
        [TestCase("Sprites/Default")]
        public void EmissiveAndSpriteTetherMaterialsKeepSupportedTintBehavior(string shader)
        {
            Create(shader);
            foreach (float elapsed in new[] { .1f, .2f })
            {
                feedback.Tick(.1f, false, true, RunPhase.Running);
                var block = new MaterialPropertyBlock(); tether.GetPropertyBlock(block);
                float glow = 1.2f + .2f * Mathf.Sin(elapsed * 21);
                var tint = VisualTheme.TetherReady;
                if (beam.HasProperty("_EmissionColor"))
                    AssertColor(block.GetColor("_EmissionColor"), new Color(tint.r * glow, tint.g * glow, tint.b * glow, tint.a));
                AssertColor(block.GetColor("_Color"), tint);
            }
        }

        [TestCase(RunPhase.Failed)]
        [TestCase(RunPhase.Completed)]
        public void HiddenAuraStopsGeometryWorkWhileTerminalEffectsFinishAndRetryClearsTrails(RunPhase phase)
        {
            Create(); feedback.Tick(.1f, false, false, RunPhase.Running);
            var halo = Lines("Core orbital halo")[0];
            Vector3 before = halo.GetPosition(12);
            if (phase == RunPhase.Failed) feedback.Failed(); else feedback.Completed();
            left.position += Vector3.forward * 5; right.position += Vector3.forward * 5;
            feedback.Tick(.05f, false, false, phase);
            Assert.That(halo.enabled, Is.False); AssertVector(halo.GetPosition(12), before);
            Assert.That(Lines("Pickup spark")[0].enabled, Is.True);
            var ring = Lines("Pulse / feedback ring")[0];
            Assert.That(ring.enabled, Is.True); Assert.That(ring.startColor.a, Is.LessThan(1));
            feedback.Tick(1, false, false, phase);
            foreach (var line in Lines("Pickup spark")) Assert.That(line.enabled, Is.False);
            foreach (var line in Lines("Pulse / feedback ring")) Assert.That(line.enabled, Is.False);
            feedback.ResetPresentation();
            var trails = Lines("Core wake");
            for (int core = 0; core < 2; core++)
                for (int p = 0; p < trails[core].positionCount; p++)
                    AssertVector(trails[core].GetPosition(p), core == 0 ? left.position : right.position);
            Assert.That(halo.enabled, Is.True);
        }

        [Test]
        public void RecycledGhostsMatchChangingSubmeshSlotsWithoutMutatingSource()
        {
            Create();
            var source = new GameObject("Snapshot source", typeof(MeshFilter), typeof(MeshRenderer));
            source.transform.SetParent(root.transform, false);
            snapshotMesh = new Mesh();
            snapshotMesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            source.GetComponent<MeshFilter>().sharedMesh = snapshotMesh;
            source.GetComponent<MeshRenderer>().sharedMaterial = surface;
            for (int iteration = 0; iteration < 50; iteration++)
            {
                int count = iteration < 24 || iteration >= 48 ? 1 : 3;
                snapshotMesh.subMeshCount = count;
                for (int sub = 0; sub < count; sub++) snapshotMesh.SetTriangles(new[] { 0, 1, 2 }, sub);
                feedback.TargetOpened(source, false);
                int visible = 0;
                foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
                    if (renderer.name == "Target mesh echo" && renderer.enabled)
                    {
                        visible++;
                        Assert.That(renderer.GetComponent<MeshFilter>().sharedMesh, Is.SameAs(snapshotMesh));
                        var slots = renderer.sharedMaterials;
                        Assert.That(slots.Length, Is.EqualTo(count));
                        foreach (var slot in slots) Assert.That(slot, Is.SameAs(slots[0]));
                    }
                Assert.That(visible, Is.EqualTo(1));
                feedback.Tick(.5f, false, false, RunPhase.Completed);
            }
            Assert.That(source.GetComponent<MeshRenderer>().sharedMaterial, Is.SameAs(surface));
            Assert.That(source.transform.localScale, Is.EqualTo(Vector3.one));
        }

        private LineRenderer[] Lines(string name) => System.Array.FindAll(root.GetComponentsInChildren<LineRenderer>(), line => line.name == name);
        private static void AssertVector(Vector3 actual, Vector3 expected) => Assert.That(Vector3.Distance(actual, expected), Is.LessThan(.00001f));
        private static void AssertColor(Color actual, Color expected)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(.00001f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(.00001f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(.00001f));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(.00001f));
        }
        private static void AssertTint(Renderer renderer, Color expected)
        {
            var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block);
            AssertColor(block.GetColor("_BaseColor"), expected);
        }
    }
}
