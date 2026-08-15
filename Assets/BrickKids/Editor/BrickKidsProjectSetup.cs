#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrickKids3D.EditorTools
{
    [InitializeOnLoad]
    public static class BrickKidsProjectSetup
    {
        private const string ScenePath = "Assets/BrickKidsDemo.unity";

        static BrickKidsProjectSetup()
        {
            EditorApplication.delayCall += EnsureScene;
        }

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

            // Android moi: bat buoc tao native 64-bit de chay tren thiet bi ARM64-only.
            // IL2CPP cho phep build ARM64.
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Android,
                ScriptingImplementation.IL2CPP
            );

            // Tao APK universal: ho tro ca tablet/phone ARM64 moi va ARMv7 cu.
            PlayerSettings.Android.targetArchitectures =
                AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;

            PlayerSettings.Android.buildApkPerCpuArchitecture = false;

            // Android 6.0+; target API tu dong dung API cao nhat co trong bo build.
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

            BuildPipeline.BuildPlayer(options);
        }

        private static void EnsureScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (!File.Exists(ScenePath))
            {
                Scene scene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single
                );

                var bootstrap = new GameObject("BrickKidsBootstrap");
                bootstrap.AddComponent<BrickKidsBootstrap>();
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            bool exists = false;
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.path == ScenePath)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                EditorBuildSettings.scenes =
                    new[] { new EditorBuildSettingsScene(ScenePath, true) };
            }
        }
    }
}
#endif
