using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickKids3D
{
    public class OrbitCamera : MonoBehaviour
    {
        public Transform target;
        public BuildManager buildManager;

        public float distance = 20f;
        public float yaw = 42f;
        public float pitch = 43f;
        public float minDistance = 3.2f;
        public float maxDistance = 800f;
        public float rotateSpeed = 0.16f;
        public float mouseRotateSpeed = 0.24f;
        public float zoomSpeed = 0.020f;

        private Vector2 lastMouse;
        private Vector2 lastMiddleMouse;
        private float visualDistance;
        private float visualYaw;
        private float visualPitch;
        private Vector3 visualFocus;
        private bool initialized;

        private void Start()
        {
            SnapToTargets();
        }

        public void ResetView()
        {
            yaw = 42f;
            pitch = 43f;
            distance = 20f;
            if (target != null) target.position = new Vector3(0f, 1.7f, 0f);
        }

        public void ZoomBy(float amount)
        {
            float scaled = Mathf.Max(1f, distance * 0.14f);
            distance = Mathf.Clamp(
                distance + Mathf.Sign(amount) * scaled,
                minDistance,
                maxDistance);
        }

        public void FitBounds(Bounds bounds)
        {
            if (target == null) return;

            Vector3 center = bounds.center;
            target.position = new Vector3(center.x, Mathf.Max(1.2f, center.y), center.z);

            float radius = Mathf.Max(
                2.5f,
                Mathf.Max(bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z)));

            // Slightly more room for portrait-like tall builds.
            distance = Mathf.Clamp(radius * 3.2f + 4f, minDistance, maxDistance);
            pitch = Mathf.Clamp(38f + bounds.extents.y * 0.25f, 32f, 58f);
        }

        private void LateUpdate()
        {
            HandleTouch();
            HandleMouse();

            pitch = Mathf.Clamp(pitch, 12f, 82f);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            if (!initialized) SnapToTargets();

            float blend = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);
            visualYaw = Mathf.LerpAngle(visualYaw, yaw, blend);
            visualPitch = Mathf.Lerp(visualPitch, pitch, blend);
            visualDistance = Mathf.Lerp(visualDistance, distance, blend);

            Vector3 targetFocus = target != null ? target.position : Vector3.zero;
            visualFocus = Vector3.Lerp(visualFocus, targetFocus, blend);

            Quaternion rotation = Quaternion.Euler(visualPitch, visualYaw, 0f);
            transform.position =
                visualFocus - rotation * Vector3.forward * visualDistance;
            transform.rotation = rotation;
        }

        private void SnapToTargets()
        {
            visualYaw = yaw;
            visualPitch = pitch;
            visualDistance = distance;
            visualFocus = target != null ? target.position : Vector3.zero;
            initialized = true;
        }

        private void HandleTouch()
        {
            if (Input.touchCount >= 2)
            {
                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);

                Vector2 previousA = a.position - a.deltaPosition;
                Vector2 previousB = b.position - b.deltaPosition;
                float previousDistance = Vector2.Distance(previousA, previousB);
                float currentDistance = Vector2.Distance(a.position, b.position);

                distance -= (currentDistance - previousDistance) * zoomSpeed;

                Vector2 averageDelta =
                    (a.deltaPosition + b.deltaPosition) * 0.5f;

                if (buildManager != null &&
                    buildManager.CameraNavigationMode)
                {
                    PanByScreenDelta(averageDelta);
                }
                else
                {
                    yaw += averageDelta.x * rotateSpeed;
                    pitch -= averageDelta.y * rotateSpeed;
                }

                if (buildManager != null)
                    buildManager.CancelPreviewGesture();

                return;
            }

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;

                if (buildManager != null &&
                    buildManager.CameraNavigationMode)
                {
                    if (touch.phase == TouchPhase.Moved)
                    {
                        PanByScreenDelta(touch.deltaPosition);
                    }
                    return;
                }

                if (buildManager != null &&
                    buildManager.IsPlacingGesture)
                    return;
            }
        }

        private void HandleMouse()
        {
            if (Input.touchCount > 0) return;

            if (Input.GetMouseButtonDown(1))
                lastMouse = Input.mousePosition;

            if (Input.GetMouseButton(1))
            {
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject())
                    return;

                Vector2 current = Input.mousePosition;
                Vector2 delta = current - lastMouse;
                lastMouse = current;

                yaw += delta.x * mouseRotateSpeed;
                pitch -= delta.y * mouseRotateSpeed;
            }

            if (Input.GetMouseButtonDown(2))
                lastMiddleMouse = Input.mousePosition;

            if (Input.GetMouseButton(2))
            {
                Vector2 current = Input.mousePosition;
                Vector2 delta = current - lastMiddleMouse;
                lastMiddleMouse = current;
                PanByScreenDelta(delta);
            }

            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f)
            {
                float step = Mathf.Max(1.2f, distance * 0.10f);
                distance -= wheel * step;
            }
        }

        private void PanByScreenDelta(Vector2 delta)
        {
            if (target == null) return;

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();

            float scale = Mathf.Max(0.006f, distance * 0.0024f);

            Vector3 move =
                (-right * delta.x - forward * delta.y) * scale;

            target.position += new Vector3(move.x, 0f, move.z);
        }
    }
}
