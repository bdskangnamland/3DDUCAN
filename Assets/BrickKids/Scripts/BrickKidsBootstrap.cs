using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickKids3D
{
    public class BrickKidsBootstrap : MonoBehaviour
    {
        private bool worldCreated;

        void Awake()
        {
            Application.targetFrameRate = 60;
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

            // Camera must exist first. Even if a decoration/material fails,
            // the player will never be left with a no-camera grey/black screen.
            Camera cam = EnsureCamera();

            Transform root = new GameObject("BrickRoot").transform;

            var managerGO = new GameObject("BuildManager");
            var manager = managerGO.AddComponent<BuildManager>();
            manager.worldCamera = cam;
            manager.brickRoot = root;

            try
            {
                EnsureEventSystem();
            }
            catch (Exception e)
            {
                Debug.LogError("BrickKids EventSystem error: " + e);
            }

            try
            {
                CreateBoard();
            }
            catch (Exception e)
            {
                Debug.LogError("BrickKids board error: " + e);
            }

            try
            {
                CreateOrbit(cam, manager);
            }
            catch (Exception e)
            {
                Debug.LogError("BrickKids orbit camera error: " + e);
            }

            try
            {
                var uiGO = new GameObject("RuntimeUIBuilder");
                var ui = uiGO.AddComponent<RuntimeUI>();
                ui.manager = manager;
                manager.runtimeUI = ui;
                ui.Build();
            }
            catch (Exception e)
            {
                Debug.LogError("BrickKids UI error: " + e);
            }

            try
            {
                CreateLights();
            }
            catch (Exception e)
            {
                Debug.LogError("BrickKids light error: " + e);
            }
        }

        private Camera EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam != null)
                return cam;

            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";

            cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.63f, 0.82f, 0.95f);
            cam.fieldOfView = 48f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;

            // Valid initial view even before OrbitCamera gets its first LateUpdate.
            camGO.transform.position = new Vector3(-9f, 11f, -9f);
            camGO.transform.LookAt(new Vector3(0f, 1.5f, 0f));

            return cam;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
                return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private void CreateBoard()
        {
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "BuildBoard";
            board.transform.position = new Vector3(0, -0.08f, 0);
            board.transform.localScale = new Vector3(20f, 0.15f, 20f);

            // Use the primitive's existing built-in material instead of
            // new Material(Shader.Find("Standard")). Standard can be stripped on Android.
            var renderer = board.GetComponent<Renderer>();
            if (renderer != null)
            {
                try
                {
                    Material mat = renderer.material;
                    if (mat != null)
                        mat.color = new Color(0.87f, 0.90f, 0.93f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Board material skipped: " + e.Message);
                }
            }

            CreateGridLines();
        }

        private void CreateOrbit(Camera cam, BuildManager manager)
        {
            var focus = new GameObject("CameraFocus").transform;
            focus.position = new Vector3(0, 1.5f, 0);

            var orbit = cam.gameObject.AddComponent<OrbitCamera>();
            orbit.target = focus;
            orbit.buildManager = manager;
        }

        private void CreateLights()
        {
            var lightGO = new GameObject("Sun");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var fillGO = new GameObject("Fill Light");
            var fill = fillGO.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.45f;
            fillGO.transform.rotation = Quaternion.Euler(35f, 140f, 0f);

            RenderSettings.ambientLight = new Color(0.62f, 0.66f, 0.72f);
        }

        private void CreateGridLines()
        {
            Shader shader = FindSafeShader();
            if (shader == null)
            {
                Debug.LogWarning("BrickKids: no safe line shader found; grid lines skipped.");
                return;
            }

            var lineRoot = new GameObject("GridLines").transform;

            for (int i = -10; i <= 10; i++)
            {
                MakeLine(lineRoot, new Vector3(i, 0.01f, -10), new Vector3(i, 0.01f, 10), shader);
                MakeLine(lineRoot, new Vector3(-10, 0.01f, i), new Vector3(10, 0.01f, i), shader);
            }
        }

        private Shader FindSafeShader()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("UI/Default");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");
            return shader;
        }

        private void MakeLine(Transform parent, Vector3 a, Vector3 b, Shader shader)
        {
            if (shader == null) return;

            var go = new GameObject("GridLine");
            go.transform.SetParent(parent, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
            lr.startWidth = 0.012f;
            lr.endWidth = 0.012f;
            lr.useWorldSpace = true;

            try
            {
                lr.material = new Material(shader);
            }
            catch
            {
                // Lines are optional. Never stop startup because of a shader.
            }

            Color c = new Color(0.45f, 0.50f, 0.56f, 0.38f);
            lr.startColor = c;
            lr.endColor = c;
        }
    }
}
