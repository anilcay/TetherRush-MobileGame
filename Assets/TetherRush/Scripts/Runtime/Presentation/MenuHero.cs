using System.Collections.Generic;
using TetherRush.Art;
using UnityEngine;
using UnityEngine.UI;

namespace TetherRush.Presentation
{
    /// <summary>Isolated live preview of our Blender assets. Owns all GPU resources; no gameplay or audio.</summary>
    public sealed class MenuHero : MonoBehaviour
    {
        private GameObject stage;
        private Camera previewCamera;
        private RenderTexture texture;
        private RawImage target;
        private readonly List<Material> materials = new List<Material>();
        public void Initialize(RawImage image)
        {
            if (stage != null) return;
            target = image;
            stage = new GameObject("Blender menu preview stage"); stage.transform.SetParent(transform,false);
            // VisualLibrary normalizes imported bounds at the world origin. Move the
            // completed stage afterwards so local mesh offsets stay correct.
            var origin = Vector3.zero;
            stage.transform.position = origin;
            var dark = Material("Menu recess",VisualTheme.Inlay);
            var silver = Material("Menu edge",VisualTheme.Silver);
            var library = new VisualLibrary(dark,silver);
            library.Create(VisualKind.TrackTile,"Menu Blender deck",stage.transform,origin + new Vector3(0,-.52f,0),new Vector3(4.5f,.32f,2.2f),Material("Menu deck",VisualTheme.Track));
            var a = library.Create(VisualKind.Core,"Menu Blender core A",stage.transform,origin + new Vector3(-1.25f,.15f,0),Vector3.one * .95f,Material("Menu peach",VisualTheme.CoreA));
            var b = library.Create(VisualKind.Core,"Menu Blender core B",stage.transform,origin + new Vector3(1.25f,.15f,0),Vector3.one * .95f,Material("Menu lavender",VisualTheme.CoreB));
            a.transform.rotation = Quaternion.Euler(15,25,8); b.transform.rotation = Quaternion.Euler(-10,-25,-8);
            var line = new GameObject("Menu tether",typeof(LineRenderer)); line.transform.SetParent(stage.transform,false);
            var renderer = line.GetComponent<LineRenderer>(); renderer.useWorldSpace = false; renderer.positionCount=2;
            renderer.SetPosition(0,origin+new Vector3(-1.25f,.15f,0)); renderer.SetPosition(1,origin+new Vector3(1.25f,.15f,0));
            renderer.startWidth=renderer.endWidth=.045f;
            var tetherMaterial = new Material(Shader.Find("Tether Rush/NeonEffect")); materials.Add(tetherMaterial);
            tetherMaterial.SetColor("_BaseColor",UiResources.Accent); renderer.sharedMaterial=tetherMaterial;
            foreach (var child in stage.GetComponentsInChildren<Transform>()) child.gameObject.layer=30;
            var cameraObject = new GameObject("Menu preview camera",typeof(Camera)); cameraObject.transform.SetParent(stage.transform,false);
            previewCamera=cameraObject.GetComponent<Camera>(); previewCamera.cullingMask=1<<30;
            previewCamera.clearFlags=CameraClearFlags.SolidColor; previewCamera.backgroundColor=UiResources.Background;
            previewCamera.orthographic=true; previewCamera.orthographicSize=1.9f;
            previewCamera.nearClipPlane=.1f; previewCamera.farClipPlane=30;
            previewCamera.transform.position=origin+new Vector3(3.6f,4.8f,-6);
            previewCamera.transform.LookAt(origin+new Vector3(0,-.1f,0));
            texture=new RenderTexture(768,480,24,RenderTextureFormat.ARGB32) {name="Menu Blender preview",antiAliasing=2};
            texture.Create(); previewCamera.targetTexture=texture; target.texture=texture;
            stage.transform.position = new Vector3(500,0,0);
            SetVisible(false);
        }
        private Material Material(string label,Color color)
        {
            var material=new Material(Shader.Find("Tether Rush/NeonSurface")) {name=label};
            material.SetColor("_BaseColor",color); material.SetFloat("_Smoothness",.6f); material.SetFloat("_RimStrength",.12f);
            materials.Add(material); return material;
        }
        public void SetVisible(bool visible) { if(stage!=null) stage.SetActive(visible); }
        private void OnDestroy()
        {
            if(target!=null) target.texture=null;
            if(previewCamera!=null) previewCamera.targetTexture=null;
            if(texture!=null) {texture.Release(); Destroy(texture);}
            if(stage!=null) Destroy(stage);
            foreach(var material in materials) if(material!=null) Destroy(material);
        }
    }
}
