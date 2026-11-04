using NUnit.Framework;
using TetherRush.Presentation;
using UnityEngine;

namespace TetherRush.Tests
{
    public sealed class MechanicFeedbackTests
    {
        private GameObject root, source;
        private MechanicFeedback feedback;
        [SetUp] public void Setup()
        {
            root = new GameObject("Mechanic feedback fixture");
            feedback = root.AddComponent<MechanicFeedback>(); feedback.Initialize();
            source = Object.Instantiate(Resources.Load<GameObject>("ArtModels/HydraulicSlider"));
            source.transform.position = new Vector3(1, .45f, 5);
            source.transform.localScale = new Vector3(1.3f, .9f, .9f);
        }
        [TearDown] public void Cleanup()
        {
            if (root != null) Object.DestroyImmediate(root);
            if (source != null) Object.DestroyImmediate(source);
        }
        [Test]
        public void RetractCopiesOnlyVisualAssetsAndLeavesSourceUntouched()
        {
            source.AddComponent<BoxCollider>(); source.AddComponent<AudioSource>().playOnAwake = false;
            Vector3 position = source.transform.position, scale = source.transform.localScale;
            Quaternion rotation = source.transform.rotation;
            feedback.Retract(source.transform);
            var snapshot = Snapshot(); Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot.GetComponentsInChildren<Collider>(), Is.Empty);
            Assert.That(snapshot.GetComponentsInChildren<AudioSource>(), Is.Empty);
            Assert.That(snapshot.GetComponentInChildren<MeshFilter>().sharedMesh,
                Is.SameAs(source.GetComponentInChildren<MeshFilter>().sharedMesh));
            feedback.Tick(.05f);
            Assert.That(source.transform.position, Is.EqualTo(position));
            Assert.That(source.transform.localScale, Is.EqualTo(scale));
            Assert.That(source.transform.rotation, Is.EqualTo(rotation));
        }
        [Test]
        public void ZeroDeltaFreezesAndResetImmediatelyHidesUnparentedSnapshots()
        {
            feedback.Retract(source.transform); var snapshot = Snapshot();
            Vector3 position = snapshot.transform.position;
            feedback.Tick(0); Assert.That(snapshot.transform.position, Is.EqualTo(position));
            feedback.ResetPresentation();
            Assert.That(feedback.ActiveEffectCount, Is.Zero);
            Assert.That(snapshot.activeSelf, Is.False);
            Assert.That(Snapshot(), Is.Null);
        }
        [Test]
        public void RepeatedEffectsStayBoundedAndDisableCleansEveryView()
        {
            for (int i = 0; i < 20; i++)
            {
                feedback.Retract(source.transform); feedback.RelayTriggered(Vector3.zero, Vector3.forward * 14);
                Assert.That(feedback.ActiveEffectCount, Is.LessThanOrEqualTo(8));
            }
            feedback.Tick(1); Assert.That(feedback.ActiveEffectCount, Is.Zero);
            feedback.Retract(source.transform); root.SetActive(false);
            Assert.That(feedback.ActiveEffectCount, Is.Zero);
            Assert.That(Snapshot(), Is.Null);
        }
        private static GameObject Snapshot() => GameObject.Find("Retracting mesh snapshot (visual only)");
    }
}
