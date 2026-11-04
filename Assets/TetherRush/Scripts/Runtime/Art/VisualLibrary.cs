using System;
using System.Collections.Generic;
using UnityEngine;

namespace TetherRush.Art
{
    public enum VisualKind
    {
        Core, TrackTile, Barrier, MovingWall, RotatingArm, EnergyGate, BreakableBlock,
        EnergyPickup, BonusCrystal, PulseBadge, FinishMarker, SplitPylon, ChevronBarrier, RelayPylon,
        ClampBarrier, CapacitorBank, HydraulicSlider, TurbineBeam
    }

    /// <summary>Blender exports are normalized independently from the authoritative gameplay geometry.</summary>
    public sealed class VisualLibrary
    {
        private readonly Dictionary<VisualKind, GameObject> models = new Dictionary<VisualKind, GameObject>();
        private readonly Material dark, accent;
        public VisualLibrary(Material dark, Material accent) { this.dark = dark; this.accent = accent; }

        public GameObject Create(VisualKind kind, string label, Transform parent, Vector3 position, Vector3 size, Material body, Material edge = null)
        {
            if (!models.TryGetValue(kind, out var prefab))
            {
                prefab = Resources.Load<GameObject>("ArtModels/" + kind);
                models[kind] = prefab;
            }
            var root = new GameObject(label);
            root.transform.SetParent(parent, false);
            if (prefab == null)
            {
                // Keeps a partially imported checkout usable; asset smoke tests require every real FBX.
                var fallback = GameObject.CreatePrimitive(kind == VisualKind.Core ? PrimitiveType.Sphere : PrimitiveType.Cube);
                fallback.name = "Visual"; fallback.transform.SetParent(root.transform, false);
                fallback.GetComponent<Renderer>().sharedMaterial = body;
                var collider = fallback.GetComponent<Collider>(); collider.enabled = false; UnityEngine.Object.Destroy(collider);
                Debug.LogWarning("Missing Blender model: " + kind);
            }
            else
            {
                var visual = UnityEngine.Object.Instantiate(prefab, root.transform, false);
                visual.name = "Visual";
                var renderers = visual.GetComponentsInChildren<MeshRenderer>();
                if (renderers.Length == 0) throw new InvalidOperationException("Model has no mesh: " + kind);
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                    var slots = renderer.sharedMaterials;
                    for (int i = 0; i < slots.Length; i++)
                    {
                        string name = slots[i] != null ? slots[i].name : "Body";
                        slots[i] = name.IndexOf("Dark", StringComparison.OrdinalIgnoreCase) >= 0 ? dark :
                            name.IndexOf("Accent", StringComparison.OrdinalIgnoreCase) >= 0 ? (edge != null ? edge : accent) : body;
                    }
                    renderer.sharedMaterials = slots;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
                if (Mathf.Min(bounds.size.x, bounds.size.y, bounds.size.z) < .00001f)
                    throw new InvalidOperationException("Degenerate Blender bounds: " + kind);
                visual.transform.position -= bounds.center;
                root.transform.localScale = new Vector3(1 / bounds.size.x, 1 / bounds.size.y, 1 / bounds.size.z);
            }
            root.transform.localScale = Vector3.Scale(root.transform.localScale, size);
            root.transform.position = position;
            return root;
        }
    }
}
