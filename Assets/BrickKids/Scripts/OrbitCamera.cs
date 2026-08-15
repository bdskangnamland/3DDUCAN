using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickKids3D
{
    public class OrbitCamera : MonoBehaviour
    {
        public Transform target;
        public BuildManager buildManager;
        public float distance = 18f;
        public float yaw = 42f;
        public float pitch = 43f;
        public float minDistance = 7f;
        public float maxDistance = 30f;
        public float rotateSpeed = 0.16f;
        public float mouseRotateSpeed = 0.24f;
        public float zoomSpeed = 0.018f;

        private Vector2 lastMouse;
        private float visualDistance;
        private float visualYaw;
        private float visualPitch;
        private bool initialized;

        private void Start()
        {
            SnapToTargets();
        }

        public void ResetView()
        {
            yaw = 42f;
            pitch = 43f;
            distance = 18f;
        }

        public void ZoomBy(float amount)
        {
            distance = Mathf.Clamp(distance + amount, minDistance, maxDistance);
        }

        private void LateUpdate()
        {
            HandleTouch();
            HandleMouse();

            pitch = Mathf.Clamp(pitch, 18f, 78f);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            if (!initialized) SnapToTargets();

            float blend = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);
            visualYaw = Mathf.LerpAngle(visualYaw, yaw, blend);
            visualPitch = Mathf.Lerp(visualPitch, pitch, blend);
            visualDistance = Mathf.Lerp(visualDistance, distance, blend);

            Quaternion rotation = Quaternion.Euler(visualPitch, visualYaw, 0f);
            Vector3 focus = target != null ? target.position : Vector3.zero;
            transform.position = focus - rotation * Vector3.forward * visualDistance;
            transform.rotation = rotation;
        }

        private void SnapToTargets()
        {
            visualYaw = yaw;
            visualPitch = pitch;
            visualDistance = distance;
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

                Vector2 averageDelta = (a.deltaPosition + b.deltaPosition) * 0.5f;
                yaw += averageDelta.x * rotateSpeed;
                pitch -= averageDelta.y * rotateSpeed;

                if (buildManager != null) buildManager.CancelPreviewGesture();
                return;
            }

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (buildManager != null && buildManager.IsPlacingGesture) return;
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

                if (touch.phase == TouchPhase.Moved)
                {
                    yaw += touch.deltaPosition.x * rotateSpeed;
                    pitch -= touch.deltaPosition.y * rotateSpeed;
                }
            }
        }

        private void HandleMouse()
        {
            if (Input.touchCount > 0) return;

            if (Input.GetMouseButtonDown(1))
                lastMouse = Input.mousePosition;

            if (Input.GetMouseButton(1))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                Vector2 current = Input.mousePosition;
                Vector2 delta = current - lastMouse;
                lastMouse = current;
                yaw += delta.x * mouseRotateSpeed;
                pitch -= delta.y * mouseRotateSpeed;
            }

            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f)
                distance -= wheel * 1.35f;
        }
    }
}
