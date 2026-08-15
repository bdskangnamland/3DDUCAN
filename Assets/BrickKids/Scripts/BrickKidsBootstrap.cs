using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickKids3D
{
    public class BrickKidsBootstrap : MonoBehaviour
    {
        void Awake()
        {
            Application.targetFrameRate = 60;
            BuildWorld();
        }

        private void BuildWorld()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            var root = new GameObject("BrickRoot").transform;

            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "BuildBoard";
            board.transform.position = new Vector3(0, -0.08f, 0);
            board.transform.localScale = new Vector3(20f, 0.15f, 20f);
            var boardRenderer = board.GetComponent<Renderer>();
            boardRenderer.material = new Material(Shader.Find("Standard"));
            boardRenderer.material.color = new Color(0.87f, 0.90f, 0.93f);
            CreateGridLines();

            var focus = new GameObject("CameraFocus").transform;
            focus.position = new Vector3(0, 1.5f, 0);

            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.63f, 0.82f, 0.95f);
            cam.fieldOfView = 48f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;

            var managerGO = new GameObject("BuildManager");
            var manager = managerGO.AddComponent<BuildManager>();
            manager.worldCamera = cam;
            manager.brickRoot = root;

            var orbit = camGO.AddComponent<OrbitCamera>();
            orbit.target = focus;
            orbit.buildManager = manager;

            var uiGO = new GameObject("RuntimeUIBuilder");
            var ui = uiGO.AddComponent<RuntimeUI>();
            ui.manager = manager;
            manager.runtimeUI = ui;
            ui.Build();

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
            var lineRoot = new GameObject("GridLines").transform;
            Shader shader = Shader.Find("Sprites/Default");
            for (int i = -10; i <= 10; i++)
            {
                MakeLine(lineRoot, new Vector3(i, 0.01f, -10), new Vector3(i, 0.01f, 10), shader);
                MakeLine(lineRoot, new Vector3(-10, 0.01f, i), new Vector3(10, 0.01f, i), shader);
            }
        }

        private void MakeLine(Transform parent, Vector3 a, Vector3 b, Shader shader)
        {
            var go = new GameObject("GridLine");
            go.transform.SetParent(parent, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, a); lr.SetPosition(1, b);
            lr.startWidth = 0.012f; lr.endWidth = 0.012f;
            lr.material = new Material(shader);
            lr.startColor = new Color(0.45f, 0.50f, 0.56f, 0.38f);
            lr.endColor = lr.startColor;
            lr.useWorldSpace = true;
        }
    }
}
