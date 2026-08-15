using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickKids3D
{
    public class OrbitCamera : MonoBehaviour
    {
        public Transform target;
        public BuildManager buildManager;
        public float distance = 18f;
        public float yaw = 45f;
        public float pitch = 42f;
        public float minDistance = 6f;
        public float maxDistance = 35f;
        public float rotateSpeed = 0.18f;
        public float mouseRotateSpeed = 0.25f;
        public float zoomSpeed = 0.02f;

        private Vector2 lastMouse;

        void LateUpdate()
        {
            HandleTouch();
            HandleMouse();
            pitch = Mathf.Clamp(pitch, 12f, 82f);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target != null ? target.position : Vector3.zero;
            transform.position = focus - rot * Vector3.forward * distance;
            transform.rotation = rot;
        }

        private void HandleTouch()
        {
            if (Input.touchCount >= 2)
            {
                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);
                Vector2 prevA = a.position - a.deltaPosition;
                Vector2 prevB = b.position - b.deltaPosition;
                float prevDist = Vector2.Distance(prevA, prevB);
                float curDist = Vector2.Distance(a.position, b.position);
                distance -= (curDist - prevDist) * zoomSpeed;

                Vector2 avgDelta = (a.deltaPosition + b.deltaPosition) * 0.5f;
                yaw += avgDelta.x * rotateSpeed;
                pitch -= avgDelta.y * rotateSpeed;
                if (buildManager != null) buildManager.CancelPreviewGesture();
                return;
            }

            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if (buildManager != null && buildManager.IsPlacingGesture) return;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId)) return;
                if (t.phase == TouchPhase.Moved)
                {
                    yaw += t.deltaPosition.x * rotateSpeed;
                    pitch -= t.deltaPosition.y * rotateSpeed;
                }
            }
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(1)) lastMouse = Input.mousePosition;
            if (Input.GetMouseButton(1))
            {
                Vector2 cur = Input.mousePosition;
                Vector2 delta = cur - lastMouse;
                lastMouse = cur;
                yaw += delta.x * mouseRotateSpeed;
                pitch -= delta.y * mouseRotateSpeed;
            }
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) distance -= wheel * 1.4f;
        }
    }
}
