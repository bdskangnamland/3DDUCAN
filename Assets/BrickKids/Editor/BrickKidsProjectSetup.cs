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
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "vn.somet.brickkids3d");

            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Android,
                ScriptingImplementation.IL2CPP
            );

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

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outDir + "/BrickKids3D.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReportOrThrow(options);
        }

        private static void BuildReportOrThrow(BuildPlayerOptions options)
        {
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception(
                    "BrickKids Android build failed: " + report.summary.result
                );
            }
        }

        private static void EnsureScene()
        {
            // IMPORTANT:
            // Build a clean empty scene. The actual game is started by
            // BrickKidsRuntimeStarter after the scene loads.
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );

            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes =
                new[] { new EditorBuildSettingsScene(ScenePath, true) };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
#endif
