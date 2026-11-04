using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TetherRush.Presentation
{
    /// <summary>Visual-only snapshots; the manager owns source visibility, motion and gameplay.</summary>
    public sealed class MechanicFeedback : MonoBehaviour
    {
        private const int Capacity = 8;
        private readonly List<Effect> effects = new List<Effect>(Capacity);
        private Material lineMaterial;
        private bool initialized;

        private sealed class Effect
        {
            public GameObject Root;
            public LineRenderer Line;
            public Vector3 Start, End;
            public float Age, Life;
        }

        public int ActiveEffectCount => effects.Count;

        public void Initialize()
        {
            initialized = true;
            if (lineMaterial != null) return;
            Shader shader = Shader.Find("Tether Rush/NeonEffect") ?? Shader.Find("Sprites/Default");
            if (shader == null) return;
            lineMaterial = new Material(shader) { name = "Mechanic relay line (owned)" };
            lineMaterial.SetColor("_BaseColor", Color.white);
        }

        public void RelayTriggered(Vector3 pad, Vector3 gate)
        {
            if (!isActiveAndEnabled || lineMaterial == null || !Finite(pad) || !Finite(gate)) return;
            var root = new GameObject("Relay travel (visual only)");
            var line = root.AddComponent<LineRenderer>();
            line.sharedMaterial = lineMaterial;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.widthMultiplier = .032f;
            line.numCapVertices = 2;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            var effect = new Effect { Root = root, Line = line, Start = pad + Vector3.up * .08f,
                End = gate + Vector3.up * .08f, Life = .35f };
            Add(effect);
            DrawLine(effect, 0);
        }

        public void Retract(Transform source)
        {
            if (!initialized || !isActiveAndEnabled || source == null || !Finite(source.position)) return;
            // An unparented identity basis avoids inherited rotation/nonuniform scale on FBX snapshots.
            var root = new GameObject("Retracting mesh snapshot (visual only)");
            root.transform.position = source.position;
            var renderers = source.GetComponentsInChildren<MeshRenderer>();
            int copied = 0;
            foreach (var renderer in renderers)
            {
                var filter = renderer.GetComponent<MeshFilter>();
                if (!renderer.enabled || filter == null || filter.sharedMesh == null) continue;
                var clone = new GameObject("Mesh snapshot", typeof(MeshFilter), typeof(MeshRenderer));
                clone.transform.SetParent(root.transform, false);
                clone.transform.SetPositionAndRotation(renderer.transform.position, renderer.transform.rotation);
                clone.transform.localScale = renderer.transform.lossyScale;
                clone.GetComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                var view = clone.GetComponent<MeshRenderer>();
                view.sharedMaterials = renderer.sharedMaterials;
                view.shadowCastingMode = ShadowCastingMode.Off;
                view.receiveShadows = false;
                copied++;
            }
            if (copied == 0) { Retire(root); return; }
            Vector3 start = root.transform.position;
            Add(new Effect { Root = root, Start = start,
                End = new Vector3(start.x < 0 ? -8 : 8, start.y - .12f, start.z), Life = .18f });
        }

        public void Tick(float dt)
        {
            if (!isActiveAndEnabled || dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                Effect effect = effects[i];
                effect.Age = Mathf.Min(effect.Life, effect.Age + dt);
                float t = effect.Age / effect.Life;
                if (effect.Root == null || t >= 1)
                {
                    Retire(effect.Root);
                    effects.RemoveAt(i);
                    continue;
                }
                if (effect.Line != null) DrawLine(effect, t);
                else effect.Root.transform.position = Vector3.Lerp(effect.Start, effect.End, t * t * (3 - 2 * t));
            }
        }

        private static void DrawLine(Effect effect, float t)
        {
            // A short traveling signal rather than a full-length flash across the track.
            effect.Line.SetPosition(0, Vector3.Lerp(effect.Start, effect.End, t));
            effect.Line.SetPosition(1, Vector3.Lerp(effect.Start, effect.End, Mathf.Min(1, t + .14f)));
            Color color = VisualTheme.Tether;
            color.a = .55f * (1 - t);
            effect.Line.startColor = effect.Line.endColor = color;
        }

        private void Add(Effect effect)
        {
            if (effects.Count == Capacity)
            {
                Retire(effects[0].Root);
                effects.RemoveAt(0);
            }
            effects.Add(effect);
        }

        public void ResetPresentation()
        {
            for (int i = 0; i < effects.Count; i++) Retire(effects[i].Root);
            effects.Clear();
        }

        private static void Retire(GameObject root)
        {
            if (root == null) return;
            root.SetActive(false); // Destroy is deferred, so eviction/retry hides the copy immediately.
            Destroy(root);
        }

        private static bool Finite(Vector3 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z);

        private void OnEnable() { if (initialized) Initialize(); }
        private void OnDisable() { Release(); }
        private void OnDestroy() { Release(); }
        private void Release()
        {
            ResetPresentation();
            if (lineMaterial != null) Destroy(lineMaterial);
            lineMaterial = null;
        }
    }
}
