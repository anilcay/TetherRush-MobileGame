using UnityEngine;

namespace TetherRush.Presentation
{
    /// <summary>Render-only atmosphere and contact shading; never participates in collisions.</summary>
    public sealed class FacilityStage : MonoBehaviour
    {
        private Transform left, right, leftShadow, rightShadow;
        private Material sky, shadow, previousSky;
        private Vector4 previousFog;
        private float previousStart, previousEnd, deckY = -.11f;

        public void Initialize(Transform world, Transform leftCore, Transform rightCore, Camera camera)
        {
            left = leftCore; right = rightCore;
            previousSky = RenderSettings.skybox;
            previousFog = Shader.GetGlobalVector("_TRFogColor");
            previousStart = Shader.GetGlobalFloat("_TRFogStart");
            previousEnd = Shader.GetGlobalFloat("_TRFogEnd");
            sky = new Material(Shader.Find("Tether Rush/FacilitySky")) { name = "Facility atmosphere" };
            sky.SetColor("_BaseColor", Color.white);
            sky.SetColor("_HorizonColor", VisualTheme.Atmosphere);
            sky.SetColor("_ZenithColor", VisualTheme.Void);
            RenderSettings.skybox = sky;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.gameObject.AddComponent<Skybox>().material = sky;
            Shader.SetGlobalColor("_TRFogColor", VisualTheme.Atmosphere);
            Shader.SetGlobalFloat("_TRFogStart", 25f);
            Shader.SetGlobalFloat("_TRFogEnd", 85f);
            shadow = new Material(Shader.Find("Tether Rush/SoftShadow")) { name = "Core contact shade" };
            shadow.SetColor("_BaseColor", new Color(.015f, .025f, .05f, .65f));
            foreach (var renderer in world.GetComponentsInChildren<MeshRenderer>())
                if (renderer.name == "Deck") { deckY = renderer.bounds.max.y + .006f; break; }
            leftShadow = CreateShadow(world, "Left core contact shade");
            rightShadow = CreateShadow(world, "Right core contact shade");
            Tick();
        }

        private Transform CreateShadow(Transform parent, string label)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            obj.name = label; obj.transform.SetParent(parent, false);
            var collider = obj.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            var renderer = obj.GetComponent<MeshRenderer>(); renderer.sharedMaterial = shadow;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            obj.transform.rotation = Quaternion.Euler(90, 0, 0);
            obj.transform.localScale = Vector3.one * 1.35f;
            return obj.transform;
        }

        public void Tick()
        {
            if (leftShadow == null) return;
            leftShadow.position = new Vector3(left.position.x, deckY, left.position.z);
            rightShadow.position = new Vector3(right.position.x, deckY, right.position.z);
        }

        private void OnDestroy()
        {
            if (RenderSettings.skybox == sky) RenderSettings.skybox = previousSky;
            Shader.SetGlobalVector("_TRFogColor", previousFog);
            Shader.SetGlobalFloat("_TRFogStart", previousStart);
            Shader.SetGlobalFloat("_TRFogEnd", previousEnd);
            if (sky != null) Destroy(sky);
            if (shadow != null) Destroy(shadow);
        }
    }
}
