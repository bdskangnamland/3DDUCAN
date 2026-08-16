using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace BrickKids3D
{
    public class BrickKidsBootstrap : MonoBehaviour
    {
        public static readonly Rect WorkspaceViewport =
            new Rect(
                0.072f,
                0.205f,
                0.864f,
                0.695f);

        private bool worldCreated;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout =
                SleepTimeout.NeverSleep;

            EnsureWorld();
        }

        public void EnsureWorld()
        {
            if (worldCreated ||
                FindObjectOfType<BuildManager>() != null)
            {
                worldCreated = true;
                return;
            }

            worldCreated = true;

            try
            {
                ConfigureQuality();

                Camera backgroundCamera =
                    CreateBackgroundCamera();

                Camera worldCamera =
                    CreateWorldCamera();

                EnsureEventSystem();

                Transform brickRoot =
                    new GameObject(
                        "BrickRoot").transform;

                GameObject managerObject =
                    new GameObject(
                        "BuildManager");

                BuildManager manager =
                    managerObject.AddComponent<BuildManager>();

                manager.worldCamera =
                    worldCamera;

                manager.brickRoot =
                    brickRoot;

                Transform focus =
                    new GameObject(
                        "CameraFocus").transform;

                focus.position =
                    new Vector3(
                        0f,
                        1.7f,
                        0f);

                OrbitCamera orbit =
                    worldCamera.gameObject.AddComponent<OrbitCamera>();

                orbit.target =
                    focus;

                orbit.buildManager =
                    manager;

                manager.orbitCamera =
                    orbit;

                GameObject surfaceObject =
                    new GameObject(
                        "InfiniteBuildSurface");

                InfiniteWorldSurface surface =
                    surfaceObject.AddComponent<InfiniteWorldSurface>();

                surface.focus =
                    focus;

                surface.orbitCamera =
                    orbit;

                surface.Build();

                Light sun;
                Light fill;
                CreateLights(
                    out sun,
                    out fill);

                GameObject environmentObject =
                    new GameObject(
                        "EnvironmentController");

                EnvironmentController environment =
                    environmentObject.AddComponent<EnvironmentController>();

                environment.worldCamera =
                    worldCamera;

                environment.backgroundCamera =
                    backgroundCamera;

                environment.worldSurface =
                    surface;

                environment.sun =
                    sun;

                environment.fill =
                    fill;

                manager.environmentController =
                    environment;

                GameObject uiObject =
                    new GameObject(
                        "RuntimeUIBuilder");

                RuntimeUI ui =
                    uiObject.AddComponent<RuntimeUI>();

                ui.manager =
                    manager;

                manager.runtimeUI =
                    ui;

                ui.Build();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "BrickKids startup failed: " +
                    exception);
            }
        }

        private void ConfigureQuality()
        {
            QualitySettings.antiAliasing = 4;
            QualitySettings.shadows =
                ShadowQuality.All;
            QualitySettings.shadowResolution =
                ShadowResolution.High;
            QualitySettings.shadowCascades = 2;
            QualitySettings.shadowDistance = 110f;
            QualitySettings.shadowNearPlaneOffset = 2f;
            QualitySettings.vSyncCount = 0;

            // Helps shadow precision when the user pans far away from world origin.
            GraphicsSettings.cameraRelativeShadowCulling = true;

            RenderSettings.fog = false;
            RenderSettings.ambientMode =
                AmbientMode.Flat;
        }

        private Camera CreateBackgroundCamera()
        {
            GameObject cameraObject =
                new GameObject(
                    "Background Camera");

            Camera camera =
                cameraObject.AddComponent<Camera>();

            camera.depth = -10f;
            camera.clearFlags =
                CameraClearFlags.SolidColor;
            camera.backgroundColor =
                new Color(
                    0.085f,
                    0.105f,
                    0.13f);
            camera.cullingMask = 0;
            camera.rect =
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f);
            camera.allowHDR = false;
            camera.allowMSAA = false;

            return camera;
        }

        private Camera CreateWorldCamera()
        {
            GameObject cameraObject =
                new GameObject(
                    "Main Camera");

            cameraObject.tag =
                "MainCamera";

            Camera camera =
                cameraObject.AddComponent<Camera>();

            camera.depth = 0f;
            camera.clearFlags =
                CameraClearFlags.SolidColor;
            camera.backgroundColor =
                new Color(
                    0.25f,
                    0.30f,
                    0.35f);
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.12f;
            camera.farClipPlane = 3000f;
            camera.allowHDR = false;
            camera.allowMSAA = true;
            camera.rect =
                WorkspaceViewport;

            cameraObject.transform.position =
                new Vector3(
                    -11f,
                    13f,
                    -11f);

            cameraObject.transform.LookAt(
                new Vector3(
                    0f,
                    1.7f,
                    0f));

            return camera;
        }

        private void CreateLights(
            out Light sun,
            out Light fill)
        {
            GameObject sunObject =
                new GameObject(
                    "Sun");

            sun =
                sunObject.AddComponent<Light>();

            sun.type =
                LightType.Directional;
            sun.intensity = 1.06f;
            sun.color =
                new Color(
                    1.0f,
                    0.96f,
                    0.90f);
            sun.shadows =
                LightShadows.Soft;
            sun.shadowStrength = 0.62f;
            sun.shadowBias = 0.045f;
            sun.shadowNormalBias = 0.32f;
            sun.shadowNearPlane = 0.2f;

            sunObject.transform.rotation =
                Quaternion.Euler(
                    48f,
                    -34f,
                    0f);

            GameObject fillObject =
                new GameObject(
                    "Fill Light");

            fill =
                fillObject.AddComponent<Light>();

            fill.type =
                LightType.Directional;
            fill.intensity = 0.31f;
            fill.color =
                new Color(
                    0.74f,
                    0.84f,
                    1.0f);
            fill.shadows =
                LightShadows.None;

            fillObject.transform.rotation =
                Quaternion.Euler(
                    36f,
                    145f,
                    0f);
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystemObject =
                new GameObject(
                    "EventSystem");

            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }
}
