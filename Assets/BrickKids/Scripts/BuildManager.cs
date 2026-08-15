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
        public OrbitCamera orbitCamera;
        public int boardHalfSize = 9;

        public string SelectedBrickId { get; private set; } = "2x4";
        public Color SelectedColor { get; private set; } = new Color(0.93f, 0.18f, 0.13f);
        public int RotationStep { get; private set; }
        public bool DeleteMode { get; private set; }
        public bool IsPlacingGesture { get; private set; }
        public bool CanUndo { get { return undo.Count > 0; } }
        public bool CanRedo { get { return redo.Count > 0; } }
        public int PieceCount { get { return pieces.Count; } }

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

        private void Update()
        {
            HandleTouch();
            HandleMouse();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveBackup();
        }

        private void OnApplicationQuit()
        {
            SaveBackup();
        }

        public void SetBrick(string id)
        {
            SelectedBrickId = id;
            DeleteMode = false;
            DestroyGhost();
            Notify();
        }

        public void SetColor(Color color)
        {
            SelectedColor = color;
            DeleteMode = false;
            DestroyGhost();
            Notify();
        }

        public void RotateSelected()
        {
            RotationStep = (RotationStep + 1) % 4;
            DestroyGhost();
            Notify();
        }

        public void ToggleDeleteMode()
        {
            DeleteMode = !DeleteMode;
            DestroyGhost();
            Notify();
        }

        public void SaveCurrent()
        {
            try
            {
                SaveSystem.Save(1, MakeSaveData());
                Notify(UIFeedback.Save);
            }
            catch
            {
                Notify(UIFeedback.Error);
            }
        }

        public void LoadCurrent()
        {
            if (!SaveSystem.Exists(1))
            {
                Notify(UIFeedback.Error);
                return;
            }

            BuildSaveData data = SaveSystem.Load(1);
            ClearAllInternal(true);

            for (int i = 0; i < data.bricks.Count; i++)
                PlaceRecord(data.bricks[i], false);

            undo.Clear();
            redo.Clear();
            Notify(UIFeedback.Load);
        }

        public void ClearAll(bool clearHistory)
        {
            ClearAllInternal(clearHistory);
            Notify(UIFeedback.Clear);
        }

        public void Undo()
        {
            if (undo.Count == 0) return;

            BuildAction action = undo.Pop();

            if (action.wasPlacement)
            {
                BrickPiece piece = FindPiece(action.record);
                if (piece != null) RemovePiece(piece, false);
            }
            else
            {
                PlaceRecord(action.record, false);
            }

            redo.Push(action);
            Notify();
        }

        public void Redo()
        {
            if (redo.Count == 0) return;

            BuildAction action = redo.Pop();

            if (action.wasPlacement)
            {
                PlaceRecord(action.record, false);
            }
            else
            {
                BrickPiece piece = FindPiece(action.record);
                if (piece != null) RemovePiece(piece, false);
            }

            undo.Push(action);
            Notify();
        }

        public void CaptureScreenshot()
        {
            try
            {
                string file = "BrickKids_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                ScreenCapture.CaptureScreenshot(file);
                Notify(UIFeedback.Screenshot);
            }
            catch
            {
                Notify(UIFeedback.Error);
            }
        }

        public void ResetCamera()
        {
            if (orbitCamera != null) orbitCamera.ResetView();
        }

        public void ZoomIn()
        {
            if (orbitCamera != null) orbitCamera.ZoomBy(-2.2f);
        }

        public void ZoomOut()
        {
            if (orbitCamera != null) orbitCamera.ZoomBy(2.2f);
        }

        public void LoadTemplate(int templateIndex)
        {
            ClearAllInternal(true);

            if (templateIndex == 0) BuildHouse();
            else if (templateIndex == 1) BuildCar();
            else if (templateIndex == 2) BuildRobot();
            else BuildTower();

            undo.Clear();
            redo.Clear();
            Notify(UIFeedback.Template);
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

            Touch touch = Input.GetTouch(0);

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                if (touch.phase == TouchPhase.Began) CancelPreviewGesture();
                return;
            }

            if (!IsInsideWorkspace(touch.position))
            {
                if (touch.phase == TouchPhase.Began) CancelPreviewGesture();
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                if (DeleteMode)
                {
                    TryDeleteAt(touch.position);
                    return;
                }

                if (TryUpdateGhost(touch.position))
                {
                    IsPlacingGesture = true;
                    activeFinger = touch.fingerId;
                }
            }
            else if (IsPlacingGesture &&
                     touch.fingerId == activeFinger &&
                     (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
            {
                TryUpdateGhost(touch.position);
            }
            else if (IsPlacingGesture &&
                     touch.fingerId == activeFinger &&
                     (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                if (touch.phase == TouchPhase.Ended && ghost != null && ghostValid)
                    CommitGhost();

                CancelPreviewGesture();
            }
        }

        private void HandleMouse()
        {
            if (Input.touchCount > 0) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mouse = Input.mousePosition;
            if (!IsInsideWorkspace(mouse)) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (DeleteMode) TryDeleteAt(mouse);
                else if (TryUpdateGhost(mouse)) IsPlacingGesture = true;
            }

            if (Input.GetMouseButton(0) && IsPlacingGesture)
                TryUpdateGhost(mouse);

            if (Input.GetMouseButtonUp(0) && IsPlacingGesture)
            {
                if (ghost != null && ghostValid) CommitGhost();
                CancelPreviewGesture();
            }
        }

        private bool IsInsideWorkspace(Vector2 screen)
        {
            if (worldCamera == null) return false;
            return worldCamera.pixelRect.Contains(screen);
        }

        private bool TryUpdateGhost(Vector2 screen)
        {
            if (worldCamera == null || !IsInsideWorkspace(screen)) return false;

            Ray ray = worldCamera.ScreenPointToRay(screen);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, 100f))
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

            if (ghost == null ||
                ghost.BrickId != SelectedBrickId ||
                ghost.RotationStep != RotationStep)
            {
                DestroyGhost();
                ghost = BrickFactory.Create(
                    SelectedBrickId,
                    gx,
                    gy,
                    gz,
                    RotationStep,
                    SelectedColor,
                    true,
                    brickRoot);
            }
            else
            {
                BrickFactory.Move(ghost, gx, gy, gz);
            }

            ghostValid = CanPlace(gx, gy, gz, w, d);

            Color previewColor = ghostValid
                ? new Color(0.18f, 0.92f, 0.42f, 0.46f)
                : new Color(1.00f, 0.20f, 0.18f, 0.46f);

            ghost.SetPreviewColor(previewColor);
            return true;
        }

        private bool CanPlace(int gx, int gy, int gz, int w, int d)
        {
            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < d; z++)
                {
                    if (occupied.ContainsKey(new Vector3Int(gx + x, gy, gz + z)))
                        return false;
                }
            }

            if (gy == 0) return true;

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < d; z++)
                {
                    if (occupied.ContainsKey(new Vector3Int(gx + x, gy - 1, gz + z)))
                        return true;
                }
            }

            return false;
        }

        private void CommitGhost()
        {
            if (ghost == null || !ghostValid) return;

            BrickRecord record = MakeRecord(
                ghost.BrickId,
                ghost.GridX,
                ghost.GridY,
                ghost.GridZ,
                ghost.RotationStep,
                SelectedColor);

            DestroyGhost();
            PlaceRecord(record, true);
            Notify();
        }

        private void PlaceRecord(BrickRecord record, bool recordUndo)
        {
            Color color = new Color(
                record.r,
                record.g,
                record.b,
                record.a <= 0f ? 1f : record.a);

            BrickSpec spec = BrickCatalog.Get(record.id);
            int w = record.rotation % 2 == 0 ? spec.width : spec.depth;
            int d = record.rotation % 2 == 0 ? spec.depth : spec.width;

            if (!CanPlace(record.x, record.y, record.z, w, d) && record.y != 0) return;
            if (HasOverlap(record.x, record.y, record.z, w, d)) return;

            BrickPiece piece = BrickFactory.Create(
                record.id,
                record.x,
                record.y,
                record.z,
                record.rotation,
                color,
                false,
                brickRoot);

            pieces.Add(piece);
            Mark(piece, true);

            if (recordUndo)
            {
                undo.Push(new BuildAction { wasPlacement = true, record = record });
                redo.Clear();
            }
        }

        private bool HasOverlap(int gx, int gy, int gz, int w, int d)
        {
            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < d; z++)
                {
                    if (occupied.ContainsKey(new Vector3Int(gx + x, gy, gz + z)))
                        return true;
                }
            }

            return false;
        }

        private void TryDeleteAt(Vector2 screen)
        {
            if (worldCamera == null || !IsInsideWorkspace(screen)) return;

            Ray ray = worldCamera.ScreenPointToRay(screen);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, 100f)) return;

            BrickPiece piece = hit.collider.GetComponentInParent<BrickPiece>();
            if (piece != null && !piece.IsPreview)
                RemovePiece(piece, true);
        }

        private void RemovePiece(BrickPiece piece, bool recordUndo)
        {
            if (piece == null) return;

            BrickRecord record = ToRecord(piece);
            Mark(piece, false);
            pieces.Remove(piece);
            Destroy(piece.gameObject);

            if (recordUndo)
            {
                undo.Push(new BuildAction { wasPlacement = false, record = record });
                redo.Clear();
                Notify();
            }
        }

        private void Mark(BrickPiece piece, bool add)
        {
            for (int x = 0; x < piece.Width; x++)
            {
                for (int z = 0; z < piece.Depth; z++)
                {
                    Vector3Int key = new Vector3Int(
                        piece.GridX + x,
                        piece.GridY,
                        piece.GridZ + z);

                    if (add)
                    {
                        occupied[key] = piece;
                    }
                    else
                    {
                        BrickPiece found;
                        if (occupied.TryGetValue(key, out found) && found == piece)
                            occupied.Remove(key);
                    }
                }
            }
        }

        private BrickPiece FindPiece(BrickRecord record)
        {
            return pieces.Find(delegate(BrickPiece piece)
            {
                return piece != null &&
                       piece.BrickId == record.id &&
                       piece.GridX == record.x &&
                       piece.GridY == record.y &&
                       piece.GridZ == record.z &&
                       piece.RotationStep == record.rotation;
            });
        }

        private void DestroyGhost()
        {
            if (ghost != null) Destroy(ghost.gameObject);
            ghost = null;
            ghostValid = false;
        }

        private void ClearAllInternal(bool clearHistory)
        {
            DestroyGhost();

            for (int i = pieces.Count - 1; i >= 0; i--)
                if (pieces[i] != null) Destroy(pieces[i].gameObject);

            pieces.Clear();
            occupied.Clear();

            if (clearHistory)
            {
                undo.Clear();
                redo.Clear();
            }
        }

        private BuildSaveData MakeSaveData()
        {
            BuildSaveData data = new BuildSaveData();

            for (int i = 0; i < pieces.Count; i++)
                if (pieces[i] != null) data.bricks.Add(ToRecord(pieces[i]));

            return data;
        }

        private void SaveBackup()
        {
            try
            {
                if (pieces.Count > 0)
                    SaveSystem.Save(2, MakeSaveData());
            }
            catch
            {
            }
        }

        private BrickRecord ToRecord(BrickPiece piece)
        {
            Color color = piece.PieceColor;
            return MakeRecord(
                piece.BrickId,
                piece.GridX,
                piece.GridY,
                piece.GridZ,
                piece.RotationStep,
                color);
        }

        private BrickRecord MakeRecord(
            string id,
            int x,
            int y,
            int z,
            int rotation,
            Color color)
        {
            return new BrickRecord
            {
                id = id,
                x = x,
                y = y,
                z = z,
                rotation = rotation,
                r = color.r,
                g = color.g,
                b = color.b,
                a = color.a
            };
        }

        private void Notify(UIFeedback feedback)
        {
            if (runtimeUI == null) return;
            runtimeUI.RefreshState();
            if (feedback != UIFeedback.None)
                runtimeUI.ShowFeedback(feedback);
        }

        private void Notify()
        {
            Notify(UIFeedback.None);
        }

        private void BuildHouse()
        {
            Color baseColor = new Color(0.86f, 0.18f, 0.14f);
            Color wall = new Color(0.97f, 0.73f, 0.12f);
            Color roof = new Color(0.14f, 0.45f, 0.90f);

            for (int x = -4; x <= 2; x += 2)
                PlaceRecord(MakeRecord("2x8", x, 0, -4, 0, baseColor), false);

            PlaceRecord(MakeRecord("2x8", -4, 1, -4, 1, wall), false);
            PlaceRecord(MakeRecord("2x8", -4, 1, 2, 1, wall), false);
            PlaceRecord(MakeRecord("2x4", -4, 1, -2, 0, wall), false);
            PlaceRecord(MakeRecord("2x4", 2, 1, -2, 0, wall), false);

            PlaceRecord(MakeRecord("2x8", -4, 2, -3, 1, roof), false);
            PlaceRecord(MakeRecord("2x8", -4, 2, -1, 1, roof), false);
            PlaceRecord(MakeRecord("2x8", -4, 2, 1, 1, roof), false);
        }

        private void BuildCar()
        {
            Color blue = new Color(0.07f, 0.39f, 0.92f);
            Color yellow = new Color(1.00f, 0.70f, 0.08f);
            Color black = new Color(0.08f, 0.09f, 0.11f);

            PlaceRecord(MakeRecord("2x8", -4, 0, -2, 1, blue), false);
            PlaceRecord(MakeRecord("2x8", -4, 0, 0, 1, blue), false);
            PlaceRecord(MakeRecord("2x4", -2, 1, -2, 1, yellow), false);
            PlaceRecord(MakeRecord("2x4", -2, 1, 0, 1, yellow), false);

            PlaceRecord(MakeRecord("1x2", -3, 0, -3, 1, black), false);
            PlaceRecord(MakeRecord("1x2", 2, 0, -3, 1, black), false);
            PlaceRecord(MakeRecord("1x2", -3, 0, 2, 1, black), false);
            PlaceRecord(MakeRecord("1x2", 2, 0, 2, 1, black), false);
        }

        private void BuildRobot()
        {
            Color body = new Color(0.12f, 0.69f, 0.78f);
            Color head = new Color(0.93f, 0.23f, 0.18f);
            Color feet = new Color(0.14f, 0.17f, 0.23f);

            PlaceRecord(MakeRecord("2x2", -2, 0, -1, 0, feet), false);
            PlaceRecord(MakeRecord("2x2", 0, 0, -1, 0, feet), false);
            PlaceRecord(MakeRecord("2x4", -2, 1, -1, 1, body), false);
            PlaceRecord(MakeRecord("2x4", -2, 2, -1, 1, body), false);
            PlaceRecord(MakeRecord("2x2", -1, 3, -1, 0, head), false);
            PlaceRecord(MakeRecord("1x2", -2, 3, -1, 0, head), false);
            PlaceRecord(MakeRecord("1x2", 1, 3, -1, 0, head), false);
        }

        private void BuildTower()
        {
            Color red = new Color(0.90f, 0.16f, 0.12f);
            Color yellow = new Color(1.00f, 0.72f, 0.08f);
            Color green = new Color(0.10f, 0.68f, 0.31f);
            Color blue = new Color(0.08f, 0.39f, 0.92f);

            PlaceRecord(MakeRecord("2x6", -3, 0, -1, 1, red), false);
            PlaceRecord(MakeRecord("2x6", -1, 1, -3, 0, yellow), false);
            PlaceRecord(MakeRecord("2x6", -3, 2, -1, 1, green), false);
            PlaceRecord(MakeRecord("2x6", -1, 3, -3, 0, blue), false);
            PlaceRecord(MakeRecord("2x4", -2, 4, -1, 1, red), false);
            PlaceRecord(MakeRecord("2x2", -1, 5, -1, 0, yellow), false);
        }
    }
}
