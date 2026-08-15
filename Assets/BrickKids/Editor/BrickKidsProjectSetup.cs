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
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            BuildPipeline.BuildPlayer(new[] { ScenePath }, outDir + "/BrickKids3D.apk", BuildTarget.Android, BuildOptions.None);
        }

        private static void EnsureScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ScenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var bootstrap = new GameObject("BrickKidsBootstrap");
                bootstrap.AddComponent<BrickKidsBootstrap>();
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            bool exists = false;
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path == ScenePath) exists = true;
            if (!exists)
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
#endif
