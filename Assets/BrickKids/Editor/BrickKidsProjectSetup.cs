#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrickKids3D.EditorTools
{
    public static class BrickKidsProjectSetup
    {
        private const string ScenePath = "Assets/BrickKidsDemo.unity";
        private const string AppIconPath = "Assets/BrickKids/AppIcon.png";

        [MenuItem("Brick Kids 3D/Open Demo Scene")]
        public static void OpenDemoScene()
        {
            EnsureScene();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Brick Kids 3D/Build Android APK")]
        public static void BuildAndroid()
        {
            EnsureScene();

            string outDir = "Builds/Android";
            Directory.CreateDirectory(outDir);

            PlayerSettings.productName = "Brick Kids 3D";
            PlayerSettings.companyName = "Somet";
            PlayerSettings.SetApplicationIdentifier(
                BuildTargetGroup.Android,
                "vn.somet.brickkids3d");

            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Android,
                ScriptingImplementation.IL2CPP);

            PlayerSettings.Android.targetArchitectures =
                AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.colorSpace = ColorSpace.Gamma;

            Texture2D appIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            if (appIcon != null)
            {
                int[] iconSizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Android);
                Texture2D[] icons = new Texture2D[iconSizes.Length];
                for (int i = 0; i < icons.Length; i++) icons[i] = appIcon;
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, icons);
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outDir + "/BrickKids3D.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception(
                    "BrickKids Android build failed: " + report.summary.result);
            }
        }

        private static void EnsureScene()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes =
                new[] { new EditorBuildSettingsScene(ScenePath, true) };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
