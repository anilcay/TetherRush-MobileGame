using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TetherRush.Editor
{
    /// <summary>Reproducible scenes and local test builds. No signing secrets or publishing.</summary>
    public static class ProjectTools
    {
        public const string MenuScenePath = "Assets/TetherRush/Scenes/MainMenu.unity";
        public const string ScenePath = "Assets/TetherRush/Scenes/Prototype.unity";
        public const string ChallengeScenePath = "Assets/TetherRush/Scenes/Challenge.unity";
        private const string MobilePipelinePath = "Assets/Settings/Mobile_RPAsset.asset";

        [MenuItem("Tether Rush/Prepare Project")]
        public static void PrepareProject()
        {
            PlayerSettings.companyName = "Anil Cay";
            PlayerSettings.productName = "Tether Rush";
            PlayerSettings.bundleVersion = "0.9.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.anilcay.tetherrush");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 10;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobilePipelinePath);
            if (pipeline == null) throw new BuildFailedException("Mobile URP asset is missing.");
            pipeline.renderScale = 1f;
            pipeline.msaaSampleCount = 2;
            pipeline.supportsHDR = false;
            pipeline.shadowDistance = 0f;
            GraphicsSettings.defaultRenderPipeline = pipeline;
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
                QualitySettings.vSyncCount = 0;
                QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
            }
            QualitySettings.SetQualityLevel(0, false);
            EditorUtility.SetDirty(pipeline);
            KeepRuntimeShaders();
            if (!File.Exists(MenuScenePath)) CreateMenuScene();
            if (!File.Exists(ScenePath)) CreatePrototypeScene();
            if (!File.Exists(ChallengeScenePath)) CreateChallengeScene();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MenuScenePath, true), new EditorBuildSettingsScene(ScenePath, true), new EditorBuildSettingsScene(ChallengeScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("TETHER_RUSH_PREPARED Unity=" + Application.unityVersion + " Android ARM64 / Portrait / URP17.3");
        }

        [MenuItem("Tether Rush/Create Main Menu Scene")]
        public static void CreateMenuScene()
        {
            Directory.CreateDirectory("Assets/TetherRush/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Tether Rush / App").AddComponent<TetherRushApp>();
            EditorSceneManager.SaveScene(scene, MenuScenePath);
            AssetDatabase.Refresh();
        }

        [MenuItem("Tether Rush/Create Timing Course Scene")]
        public static void CreateChallengeScene()
        {
            Directory.CreateDirectory("Assets/TetherRush/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Tether Rush — Timing").AddComponent<TetherRushGame>().ConfigureCourse(true);
            EditorSceneManager.SaveScene(scene, ChallengeScenePath);
            AssetDatabase.Refresh();
        }

        [MenuItem("Tether Rush/Recreate Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            Directory.CreateDirectory("Assets/TetherRush/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Tether Rush — M1").AddComponent<TetherRushGame>();
            // Explicit build references retain primitive meshes/components used by runtime construction.
            var references = new GameObject("Build References (inactive)");
            foreach (var primitive in new[] { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder, PrimitiveType.Quad })
            {
                var child = GameObject.CreatePrimitive(primitive);
                child.transform.SetParent(references.transform, false);
            }
            references.SetActive(false);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
        }

        [MenuItem("Tether Rush/Build Android Test APK")]
        public static void BuildAndroid()
        {
            PrepareProject();
            EditorUserBuildSettings.buildAppBundle = false;
            Build(BuildTarget.Android, "output/android/TetherRush-Optimized.apk");
        }

        [MenuItem("Tether Rush/Build Windows Preview")]
        public static void BuildWindows()
        {
            PrepareProject();
            Build(BuildTarget.StandaloneWindows64, "output/windows-optimized/TetherRush.exe");
        }

        private static void Build(BuildTarget target, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MenuScenePath, ScenePath, ChallengeScenePath },
                locationPathName = path,
                target = target,
                options = BuildOptions.Development | BuildOptions.StrictMode
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException(target + " build failed: " + report.summary.result);
            Debug.Log("TETHER_RUSH_BUILD_OK " + Path.GetFullPath(path) + " bytes=" + report.summary.totalSize);
        }

        private static void KeepRuntimeShaders()
        {
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            var shaders = settings.FindProperty("m_AlwaysIncludedShaders");
            foreach (var shaderName in new[] { "Universal Render Pipeline/Unlit", "Sprites/Default", "UI/Default", "Tether Rush/NeonSurface", "Tether Rush/NeonEffect", "Tether Rush/NeonGlow", "Tether Rush/FacilitySky", "Tether Rush/SoftShadow" })
            {
                var shader = Shader.Find(shaderName);
                if (shader == null) throw new BuildFailedException("Missing shader: " + shaderName);
                bool found = false;
                for (int i = 0; i < shaders.arraySize; i++)
                    if (shaders.GetArrayElementAtIndex(i).objectReferenceValue == shader) found = true;
                if (found) continue;
                int index = shaders.arraySize;
                shaders.InsertArrayElementAtIndex(index);
                shaders.GetArrayElementAtIndex(index).objectReferenceValue = shader;
            }
            settings.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
