using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickKids3D
{
    public class BrickKidsBootstrap : MonoBehaviour
    {
        public static readonly Rect WorkspaceViewport =
            new Rect(0.095f, 0.155f, 0.84f, 0.745f);

        private bool worldCreated;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            EnsureWorld();
        }

        public void EnsureWorld()
        {
            if (worldCreated || FindObjectOfType<BuildManager>() != null)
            {
                worldCreated = true;
                return;
            }

            worldCreated = true;

            try
            {
                CreateBackgroundCamera();
                Camera worldCamera = CreateWorldCamera();
                EnsureEventSystem();

                Transform brickRoot = new GameObject("BrickRoot").transform;

                GameObject managerObject = new GameObject("BuildManager");
                BuildManager manager = managerObject.AddComponent<BuildManager>();
                manager.worldCamera = worldCamera;
                manager.brickRoot = brickRoot;
                manager.boardHalfSize = 9;

                BoardFactory.CreateStudioFloor();
                BoardFactory.CreateBaseplate(manager.boardHalfSize);

                Transform focus = new GameObject("CameraFocus").transform;
                focus.position = new Vector3(0f, 1.7f, 0f);

                OrbitCamera orbit = worldCamera.gameObject.AddComponent<OrbitCamera>();
                orbit.target = focus;
                orbit.buildManager = manager;
                manager.orbitCamera = orbit;

                GameObject uiObject = new GameObject("RuntimeUIBuilder");
                RuntimeUI ui = uiObject.AddComponent<RuntimeUI>();
                ui.manager = manager;
                manager.runtimeUI = ui;
                ui.Build();

                RenderSettings.fog = false;
                QualitySettings.antiAliasing = 2;
            }
            catch (Exception exception)
            {
                Debug.LogError("BrickKids startup failed: " + exception);
            }
        }

        private void CreateBackgroundCamera()
        {
            GameObject cameraObject = new GameObject("Background Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.depth = -10f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.060f, 0.085f);
            camera.cullingMask = 0;
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = false;
        }

        private Camera CreateWorldCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.depth = 0f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.91f, 0.945f, 0.97f);
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 150f;
            camera.allowHDR = false;
            camera.allowMSAA = true;
            camera.rect = WorkspaceViewport;

            cameraObject.transform.position = new Vector3(-9f, 11f, -9f);
            cameraObject.transform.LookAt(new Vector3(0f, 1.5f, 0f));
            return camera;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }
}
