using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickKids3D
{
    public class BuildManager : MonoBehaviour
    {
        public Camera worldCamera;
        public Transform brickRoot;
        public RuntimeUI runtimeUI;
        public int boardHalfSize = 10;

        public string SelectedBrickId { get; private set; } = "2x4";
        public Color SelectedColor { get; private set; } = new Color(0.90f, 0.10f, 0.08f);
        public int RotationStep { get; private set; }
        public bool DeleteMode { get; private set; }
        public bool IsPlacingGesture { get; private set; }
        public int CurrentSlot { get; private set; } = 1;

        private readonly List<BrickPiece> pieces = new List<BrickPiece>();
        private readonly Dictionary<Vector3Int, BrickPiece> occupied = new Dictionary<Vector3Int, BrickPiece>();
        private readonly Stack<BuildAction> undo = new Stack<BuildAction>();
        private readonly Stack<BuildAction> redo = new Stack<BuildAction>();
        private BrickPiece ghost;
        private bool ghostValid;
        private int activeFinger = -999;

        private class BuildAction
        {
            public bool wasPlacement;
            public BrickRecord record;
        }

        void Update()
        {
            HandleTouch();
            HandleMouse();
        }

        public void SetBrick(string id)
        {
            SelectedBrickId = id;
            DeleteMode = false;
            RefreshGhostShape();
            Notify();
        }

        public void SetColor(Color c)
        {
            SelectedColor = c;
            DeleteMode = false;
            RefreshGhostShape();
            Notify();
        }

        public void RotateSelected()
        {
            RotationStep = (RotationStep + 1) % 4;
            RefreshGhostShape();
            Notify();
        }

        public void ToggleDeleteMode()
        {
            DeleteMode = !DeleteMode;
            DestroyGhost();
            Notify();
        }

        public void SetSlot(int slot)
        {
            CurrentSlot = Mathf.Clamp(slot, 1, 3);
            Notify("SLOT " + CurrentSlot);
        }

        public void SaveCurrent()
        {
            var data = new BuildSaveData();
            foreach (var p in pieces) data.bricks.Add(ToRecord(p));
            SaveSystem.Save(CurrentSlot, data);
            Notify("DA LUU SLOT " + CurrentSlot);
        }

        public void LoadCurrent()
        {
            ClearAll(false);
            var data = SaveSystem.Load(CurrentSlot);
            foreach (var r in data.bricks)
                PlaceRecord(r, false);
            undo.Clear();
            redo.Clear();
            Notify("DA MO SLOT " + CurrentSlot);
        }

        public void ClearAll(bool keepUndo = true)
        {
            DestroyGhost();
            for (int i = pieces.Count - 1; i >= 0; i--)
                if (pieces[i] != null) Destroy(pieces[i].gameObject);
            pieces.Clear();
            occupied.Clear();
            if (!keepUndo)
            {
                undo.Clear();
                redo.Clear();
            }
            Notify("DA XOA TOAN BO");
        }

        public void Undo()
        {
            if (undo.Count == 0) return;
            BuildAction a = undo.Pop();
            if (a.wasPlacement)
            {
                BrickPiece p = FindPiece(a.record);
                if (p != null) RemovePiece(p, false);
            }
            else
            {
                PlaceRecord(a.record, false);
            }
            redo.Push(a);
            Notify("UNDO");
        }

        public void Redo()
        {
            if (redo.Count == 0) return;
            BuildAction a = redo.Pop();
            if (a.wasPlacement) PlaceRecord(a.record, false);
            else
            {
                BrickPiece p = FindPiece(a.record);
                if (p != null) RemovePiece(p, false);
            }
            undo.Push(a);
            Notify("REDO");
        }

        public void CaptureScreenshot()
        {
            string file = "BrickKids_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            ScreenCapture.CaptureScreenshot(file);
            Notify("DA CHUP ANH");
        }

        public void LoadDemo()
        {
            ClearAll(false);
            Color red = new Color(0.92f, 0.12f, 0.08f);
            Color yellow = new Color(1.00f, 0.72f, 0.05f);
            Color blue = new Color(0.05f, 0.32f, 0.90f);
            Color white = new Color(0.95f, 0.95f, 0.95f);

            // A simple colorful tower/house-like demo.
            // Base: 8 x 4 studs.
            PlaceRecord(Make("2x4", -4, 0, -2, 0, red), false);
            PlaceRecord(Make("2x4", -2, 0, -2, 0, red), false);
            PlaceRecord(Make("2x4", 0, 0, -2, 0, red), false);
            PlaceRecord(Make("2x4", 2, 0, -2, 0, red), false);

            // Cross-locking second layer.
            PlaceRecord(Make("2x4", -4, 1, -2, 1, yellow), false);
            PlaceRecord(Make("2x4", 0, 1, -2, 1, yellow), false);
            PlaceRecord(Make("2x4", -4, 1, 0, 1, blue), false);
            PlaceRecord(Make("2x4", 0, 1, 0, 1, blue), false);

            // Small roof/detail.
            PlaceRecord(Make("2x6", -3, 2, -1, 0, white), false);
            undo.Clear(); redo.Clear();
            Notify("MAU DEMO");
        }

        public void CancelPreviewGesture()
        {
            IsPlacingGesture = false;
            activeFinger = -999;
            DestroyGhost();
        }

        private void HandleTouch()
        {
            if (Input.touchCount == 0)
            {
                if (activeFinger >= 0) CancelPreviewGesture();
                return;
            }
            if (Input.touchCount > 1) return;

            Touch t = Input.GetTouch(0);
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
            {
                if (t.phase == TouchPhase.Began) CancelPreviewGesture();
                return;
            }

            if (t.phase == TouchPhase.Began)
            {
                if (DeleteMode)
                {
                    TryDeleteAt(t.position);
                    return;
                }

                if (TryUpdateGhost(t.position))
                {
                    IsPlacingGesture = true;
                    activeFinger = t.fingerId;
                }
            }
            else if (IsPlacingGesture && t.fingerId == activeFinger && (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
            {
                TryUpdateGhost(t.position);
            }
            else if (IsPlacingGesture && t.fingerId == activeFinger && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
            {
                if (t.phase == TouchPhase.Ended && ghost != null && ghostValid)
                    CommitGhost();
                CancelPreviewGesture();
            }
        }

        private void HandleMouse()
        {
            if (Input.touchCount > 0) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (DeleteMode) TryDeleteAt(Input.mousePosition);
                else if (TryUpdateGhost(Input.mousePosition)) IsPlacingGesture = true;
            }
            if (Input.GetMouseButton(0) && IsPlacingGesture) TryUpdateGhost(Input.mousePosition);
            if (Input.GetMouseButtonUp(0) && IsPlacingGesture)
            {
                if (ghost != null && ghostValid) CommitGhost();
                CancelPreviewGesture();
            }
        }

        private bool TryUpdateGhost(Vector2 screen)
        {
            if (worldCamera == null) return false;
            Ray ray = worldCamera.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                DestroyGhost();
                return false;
            }

            int gy = 0;
            BrickPiece hitPiece = hit.collider.GetComponentInParent<BrickPiece>();
            if (hitPiece != null) gy = hitPiece.GridY + 1;

            BrickSpec baseSpec = BrickCatalog.Get(SelectedBrickId);
            int w = RotationStep % 2 == 0 ? baseSpec.width : baseSpec.depth;
            int d = RotationStep % 2 == 0 ? baseSpec.depth : baseSpec.width;

            int gx = Mathf.FloorToInt(hit.point.x - w * 0.5f + 0.5f);
            int gz = Mathf.FloorToInt(hit.point.z - d * 0.5f + 0.5f);

            gx = Mathf.Clamp(gx, -boardHalfSize, boardHalfSize - w);
            gz = Mathf.Clamp(gz, -boardHalfSize, boardHalfSize - d);

            if (ghost == null || ghost.BrickId != SelectedBrickId || ghost.RotationStep != RotationStep)
            {
                DestroyGhost();
                ghost = BrickFactory.Create(SelectedBrickId, gx, gy, gz, RotationStep, SelectedColor, true, brickRoot);
            }
            else BrickFactory.Move(ghost, gx, gy, gz);

            ghostValid = CanPlace(gx, gy, gz, w, d);
            Color pc = ghostValid ? new Color(0.15f, 1f, 0.25f, 0.48f) : new Color(1f, 0.12f, 0.08f, 0.48f);
            ghost.SetPreviewColor(pc);
            return true;
        }

        private bool CanPlace(int gx, int gy, int gz, int w, int d)
        {
            for (int x = 0; x < w; x++)
                for (int z = 0; z < d; z++)
                    if (occupied.ContainsKey(new Vector3Int(gx + x, gy, gz + z))) return false;

            if (gy == 0) return true;
            for (int x = 0; x < w; x++)
                for (int z = 0; z < d; z++)
                    if (occupied.ContainsKey(new Vector3Int(gx + x, gy - 1, gz + z))) return true;
            return false;
        }

        private void CommitGhost()
        {
            if (ghost == null || !ghostValid) return;
            BrickRecord r = Make(ghost.BrickId, ghost.GridX, ghost.GridY, ghost.GridZ, ghost.RotationStep, SelectedColor);
            DestroyGhost();
            PlaceRecord(r, true);
        }

        private void PlaceRecord(BrickRecord r, bool recordUndo)
        {
            var color = new Color(r.r, r.g, r.b, r.a <= 0f ? 1f : r.a);
            BrickSpec s = BrickCatalog.Get(r.id);
            int w = r.rotation % 2 == 0 ? s.width : s.depth;
            int d = r.rotation % 2 == 0 ? s.depth : s.width;
            if (!CanPlace(r.x, r.y, r.z, w, d) && r.y != 0) return;
            if (HasOverlap(r.x, r.y, r.z, w, d)) return;

            var p = BrickFactory.Create(r.id, r.x, r.y, r.z, r.rotation, color, false, brickRoot);
            pieces.Add(p);
            Mark(p, true);
            if (recordUndo)
            {
                undo.Push(new BuildAction { wasPlacement = true, record = r });
                redo.Clear();
            }
        }

        private bool HasOverlap(int gx, int gy, int gz, int w, int d)
        {
            for (int x = 0; x < w; x++)
                for (int z = 0; z < d; z++)
                    if (occupied.ContainsKey(new Vector3Int(gx + x, gy, gz + z))) return true;
            return false;
        }

        private void TryDeleteAt(Vector2 screen)
        {
            if (worldCamera == null) return;
            Ray ray = worldCamera.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;
            BrickPiece p = hit.collider.GetComponentInParent<BrickPiece>();
            if (p != null && !p.IsPreview) RemovePiece(p, true);
        }

        private void RemovePiece(BrickPiece p, bool recordUndo)
        {
            if (p == null) return;
            BrickRecord r = ToRecord(p);
            Mark(p, false);
            pieces.Remove(p);
            Destroy(p.gameObject);
            if (recordUndo)
            {
                undo.Push(new BuildAction { wasPlacement = false, record = r });
                redo.Clear();
            }
            Notify("DA XOA 1 KHOI");
        }

        private void Mark(BrickPiece p, bool add)
        {
            for (int x = 0; x < p.Width; x++)
            {
                for (int z = 0; z < p.Depth; z++)
                {
                    var key = new Vector3Int(p.GridX + x, p.GridY, p.GridZ + z);
                    if (add) occupied[key] = p;
                    else if (occupied.TryGetValue(key, out var found) && found == p) occupied.Remove(key);
                }
            }
        }

        private BrickPiece FindPiece(BrickRecord r)
        {
            return pieces.Find(p => p != null && p.BrickId == r.id && p.GridX == r.x && p.GridY == r.y && p.GridZ == r.z && p.RotationStep == r.rotation);
        }

        private void RefreshGhostShape()
        {
            DestroyGhost();
        }

        private void DestroyGhost()
        {
            if (ghost != null) Destroy(ghost.gameObject);
            ghost = null;
            ghostValid = false;
        }

        private BrickRecord ToRecord(BrickPiece p)
        {
            Color c = p.PieceColor;
            return Make(p.BrickId, p.GridX, p.GridY, p.GridZ, p.RotationStep, c);
        }

        private BrickRecord Make(string id, int x, int y, int z, int rotation, Color c)
        {
            return new BrickRecord { id = id, x = x, y = y, z = z, rotation = rotation, r = c.r, g = c.g, b = c.b, a = c.a };
        }

        private void Notify(string message = null)
        {
            if (runtimeUI != null) runtimeUI.Refresh(message);
        }
    }
}
