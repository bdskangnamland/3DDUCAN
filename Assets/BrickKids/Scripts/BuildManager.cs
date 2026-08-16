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
        public EnvironmentController environmentController;

        public string SelectedBrickId { get; private set; } = "2x4";
        public Color SelectedColor { get; private set; } =
            new Color(0.93f, 0.18f, 0.13f);

        public int RotationStep { get; private set; }
        public bool DeleteMode { get; private set; }
        public bool CameraNavigationMode { get; private set; }
        public bool IsPlacingGesture { get; private set; }

        public bool CanUndo { get { return undo.Count > 0; } }
        public bool CanRedo { get { return redo.Count > 0; } }
        public int PieceCount { get { return pieces.Count; } }

        private readonly List<BrickPiece> pieces =
            new List<BrickPiece>();

        private readonly Dictionary<Vector3Int, BrickPiece> occupied =
            new Dictionary<Vector3Int, BrickPiece>();

        private readonly Stack<BuildAction> undo =
            new Stack<BuildAction>();

        private readonly Stack<BuildAction> redo =
            new Stack<BuildAction>();

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
            SetItem(id);
        }

        public void SetItem(string id)
        {
            if (!BrickCatalog.Contains(id)) return;

            SelectedBrickId = id;
            DeleteMode = false;
            CameraNavigationMode = false;
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
            CameraNavigationMode = false;
            DestroyGhost();
            Notify();
        }

        public void ToggleCameraNavigationMode()
        {
            CameraNavigationMode = !CameraNavigationMode;
            DeleteMode = false;
            CancelPreviewGesture();
            Notify();
        }

        public void SetEnvironmentTheme(int index)
        {
            if (environmentController != null)
                environmentController.SetTheme(index);

            if (runtimeUI != null)
                runtimeUI.RefreshState();
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
            FocusAll();
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
                if (piece != null)
                    RemovePiece(piece, false);
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
                if (piece != null)
                    RemovePiece(piece, false);
            }

            undo.Push(action);
            Notify();
        }

        public void CaptureScreenshot()
        {
            try
            {
                string file =
                    "BrickKids_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                    ".png";

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
            if (orbitCamera != null)
                orbitCamera.ResetView();
        }

        public void FocusAll()
        {
            if (orbitCamera == null) return;

            Bounds bounds;
            if (TryGetBuildBounds(out bounds))
                orbitCamera.FitBounds(bounds);
            else
                orbitCamera.ResetView();
        }

        public void ZoomIn()
        {
            if (orbitCamera != null)
                orbitCamera.ZoomBy(-1f);
        }

        public void ZoomOut()
        {
            if (orbitCamera != null)
                orbitCamera.ZoomBy(1f);
        }

        public bool TryGetBuildBounds(out Bounds bounds)
        {
            bool hasBounds = false;
            bounds = new Bounds(Vector3.zero, Vector3.one);

            for (int i = 0; i < pieces.Count; i++)
            {
                BrickPiece piece = pieces[i];
                if (piece == null) continue;

                Renderer[] renderers =
                    piece.GetComponentsInChildren<Renderer>(true);

                if (renderers.Length > 0)
                {
                    for (int r = 0; r < renderers.Length; r++)
                    {
                        if (!renderers[r].enabled) continue;

                        if (!hasBounds)
                        {
                            bounds = renderers[r].bounds;
                            hasBounds = true;
                        }
                        else
                        {
                            bounds.Encapsulate(renderers[r].bounds);
                        }
                    }
                }
                else
                {
                    if (!hasBounds)
                    {
                        bounds = new Bounds(piece.transform.position, Vector3.one);
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(piece.transform.position);
                    }
                }
            }

            return hasBounds;
        }

        public void LoadTemplate(int templateIndex)
        {
            ClearAllInternal(true);

            if (templateIndex == 0) BuildHouseScene();
            else if (templateIndex == 1) BuildStreetScene();
            else if (templateIndex == 2) BuildGardenScene();
            else BuildTowerScene();

            undo.Clear();
            redo.Clear();
            FocusAll();
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
            if (CameraNavigationMode)
            {
                CancelPreviewGesture();
                return;
            }

            if (Input.touchCount == 0)
            {
                if (activeFinger >= 0)
                    CancelPreviewGesture();
                return;
            }

            if (Input.touchCount > 1)
                return;

            Touch touch = Input.GetTouch(0);

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(
                    touch.fingerId))
            {
                if (touch.phase == TouchPhase.Began)
                    CancelPreviewGesture();
                return;
            }

            if (!IsInsideWorkspace(touch.position))
            {
                if (touch.phase == TouchPhase.Began)
                    CancelPreviewGesture();
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
            else if (
                IsPlacingGesture &&
                touch.fingerId == activeFinger &&
                (touch.phase == TouchPhase.Moved ||
                 touch.phase == TouchPhase.Stationary))
            {
                TryUpdateGhost(touch.position);
            }
            else if (
                IsPlacingGesture &&
                touch.fingerId == activeFinger &&
                (touch.phase == TouchPhase.Ended ||
                 touch.phase == TouchPhase.Canceled))
            {
                if (touch.phase == TouchPhase.Ended &&
                    ghost != null &&
                    ghostValid)
                {
                    CommitGhost();
                }

                CancelPreviewGesture();
            }
        }

        private void HandleMouse()
        {
            if (CameraNavigationMode) return;
            if (Input.touchCount > 0) return;

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mouse = Input.mousePosition;

            if (!IsInsideWorkspace(mouse))
                return;

            if (Input.GetMouseButtonDown(0))
            {
                if (DeleteMode)
                    TryDeleteAt(mouse);
                else if (TryUpdateGhost(mouse))
                    IsPlacingGesture = true;
            }

            if (Input.GetMouseButton(0) &&
                IsPlacingGesture)
            {
                TryUpdateGhost(mouse);
            }

            if (Input.GetMouseButtonUp(0) &&
                IsPlacingGesture)
            {
                if (ghost != null && ghostValid)
                    CommitGhost();

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
            if (worldCamera == null ||
                !IsInsideWorkspace(screen))
                return false;

            BrickSpec selectedSpec =
                BrickCatalog.Get(SelectedBrickId);

            Ray ray =
                worldCamera.ScreenPointToRay(screen);

            Vector3 placementPoint;
            int gy;

            if (!TryGetPlacementPoint(
                ray,
                selectedSpec,
                out placementPoint,
                out gy))
            {
                DestroyGhost();
                return false;
            }

            int w =
                RotationStep % 2 == 0
                ? selectedSpec.width
                : selectedSpec.depth;

            int d =
                RotationStep % 2 == 0
                ? selectedSpec.depth
                : selectedSpec.width;

            // No clamp: logical construction coordinates are unlimited.
            int gx =
                Mathf.FloorToInt(
                    placementPoint.x -
                    w * 0.5f +
                    0.5f);

            int gz =
                Mathf.FloorToInt(
                    placementPoint.z -
                    d * 0.5f +
                    0.5f);

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
                BrickFactory.Move(
                    ghost,
                    gx,
                    gy,
                    gz);
            }

            ghostValid =
                CanPlace(
                    selectedSpec,
                    gx,
                    gy,
                    gz,
                    w,
                    d);

            Color previewColor =
                ghostValid
                ? new Color(
                    0.18f,
                    0.92f,
                    0.42f,
                    0.46f)
                : new Color(
                    1.00f,
                    0.20f,
                    0.18f,
                    0.46f);

            ghost.SetPreviewColor(
                previewColor);

            return true;
        }

        private bool TryGetPlacementPoint(
            Ray ray,
            BrickSpec selectedSpec,
            out Vector3 point,
            out int gy)
        {
            point = Vector3.zero;
            gy = 0;

            // Ground-only objects always use the true infinite ground plane.
            // This lets roads, trees and vehicles be placed on visual road/water
            // surfaces without being forced to "stack" by their colliders.
            if (selectedSpec.groundOnly)
            {
                Plane ground =
                    new Plane(
                        Vector3.up,
                        Vector3.zero);

                float enter;
                if (!ground.Raycast(ray, out enter))
                    return false;

                point = ray.GetPoint(enter);
                gy = 0;
                return true;
            }

            RaycastHit hit;
            if (Physics.Raycast(
                ray,
                out hit,
                2000f))
            {
                BrickPiece hitPiece =
                    hit.collider.GetComponentInParent<BrickPiece>();

                if (hitPiece != null)
                {
                    point = hit.point;

                    if (hitPiece.IsSurface)
                    {
                        gy = 0;
                    }
                    else
                    {
                        gy =
                            hitPiece.GridY +
                            hitPiece.HeightLayers;
                    }

                    return true;
                }
            }

            Plane fallbackGround =
                new Plane(
                    Vector3.up,
                    Vector3.zero);

            float fallbackEnter;
            if (!fallbackGround.Raycast(
                ray,
                out fallbackEnter))
                return false;

            point =
                ray.GetPoint(fallbackEnter);
            gy = 0;
            return true;
        }

        private bool CanPlace(
            BrickSpec spec,
            int gx,
            int gy,
            int gz,
            int w,
            int d)
        {
            if (spec.groundOnly && gy != 0)
                return false;

            // Roads, sidewalks, parking and water can layer visually.
            // They do not consume solid construction cells.
            if (spec.isSurface)
                return gy == 0;

            int h =
                Mathf.Max(
                    1,
                    spec.heightLayers);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int z = 0; z < d; z++)
                    {
                        if (occupied.ContainsKey(
                            new Vector3Int(
                                gx + x,
                                gy + y,
                                gz + z)))
                        {
                            return false;
                        }
                    }
                }
            }

            if (gy == 0)
                return true;

            // A piece only needs at least one supported cell beneath it,
            // matching toy-brick behaviour and allowing overhangs.
            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < d; z++)
                {
                    if (occupied.ContainsKey(
                        new Vector3Int(
                            gx + x,
                            gy - 1,
                            gz + z)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CommitGhost()
        {
            if (ghost == null ||
                !ghostValid)
                return;

            BrickRecord record =
                MakeRecord(
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

        private void PlaceRecord(
            BrickRecord record,
            bool recordUndo)
        {
            string id =
                BrickCatalog.Contains(record.id)
                ? record.id
                : "2x4";

            BrickSpec spec =
                BrickCatalog.Get(id);

            Color color =
                new Color(
                    record.r,
                    record.g,
                    record.b,
                    record.a <= 0f
                        ? 1f
                        : record.a);

            int rotation =
                ((record.rotation % 4) + 4) % 4;

            int w =
                rotation % 2 == 0
                ? spec.width
                : spec.depth;

            int d =
                rotation % 2 == 0
                ? spec.depth
                : spec.width;

            if (!CanPlace(
                spec,
                record.x,
                record.y,
                record.z,
                w,
                d))
            {
                return;
            }

            BrickPiece piece =
                BrickFactory.Create(
                    id,
                    record.x,
                    record.y,
                    record.z,
                    rotation,
                    color,
                    false,
                    brickRoot);

            pieces.Add(piece);
            Mark(piece, true);

            if (recordUndo)
            {
                BrickRecord normalized =
                    MakeRecord(
                        id,
                        record.x,
                        record.y,
                        record.z,
                        rotation,
                        color);

                undo.Push(
                    new BuildAction
                    {
                        wasPlacement = true,
                        record = normalized
                    });

                redo.Clear();
            }
        }

        private void TryDeleteAt(Vector2 screen)
        {
            if (worldCamera == null ||
                !IsInsideWorkspace(screen))
                return;

            Ray ray =
                worldCamera.ScreenPointToRay(screen);

            RaycastHit hit;

            if (!Physics.Raycast(
                ray,
                out hit,
                2000f))
                return;

            BrickPiece piece =
                hit.collider.GetComponentInParent<BrickPiece>();

            if (piece != null &&
                !piece.IsPreview)
            {
                RemovePiece(piece, true);
            }
        }

        private void RemovePiece(
            BrickPiece piece,
            bool recordUndo)
        {
            if (piece == null) return;

            BrickRecord record =
                ToRecord(piece);

            Mark(piece, false);
            pieces.Remove(piece);
            Destroy(piece.gameObject);

            if (recordUndo)
            {
                undo.Push(
                    new BuildAction
                    {
                        wasPlacement = false,
                        record = record
                    });

                redo.Clear();
                Notify();
            }
        }

        private void Mark(
            BrickPiece piece,
            bool add)
        {
            if (piece == null ||
                piece.IsSurface)
                return;

            int height =
                Mathf.Max(
                    1,
                    piece.HeightLayers);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0;
                     x < piece.Width;
                     x++)
                {
                    for (int z = 0;
                         z < piece.Depth;
                         z++)
                    {
                        Vector3Int key =
                            new Vector3Int(
                                piece.GridX + x,
                                piece.GridY + y,
                                piece.GridZ + z);

                        if (add)
                        {
                            occupied[key] =
                                piece;
                        }
                        else
                        {
                            BrickPiece found;

                            if (occupied.TryGetValue(
                                key,
                                out found) &&
                                found == piece)
                            {
                                occupied.Remove(key);
                            }
                        }
                    }
                }
            }
        }

        private BrickPiece FindPiece(
            BrickRecord record)
        {
            return pieces.Find(
                delegate(BrickPiece piece)
                {
                    return
                        piece != null &&
                        piece.BrickId == record.id &&
                        piece.GridX == record.x &&
                        piece.GridY == record.y &&
                        piece.GridZ == record.z &&
                        piece.RotationStep ==
                        record.rotation;
                });
        }

        private void DestroyGhost()
        {
            if (ghost != null)
                Destroy(
                    ghost.gameObject);

            ghost = null;
            ghostValid = false;
        }

        private void ClearAllInternal(
            bool clearHistory)
        {
            DestroyGhost();

            for (int i =
                    pieces.Count - 1;
                 i >= 0;
                 i--)
            {
                if (pieces[i] != null)
                {
                    Destroy(
                        pieces[i].gameObject);
                }
            }

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
            BuildSaveData data =
                new BuildSaveData();

            for (int i = 0;
                 i < pieces.Count;
                 i++)
            {
                if (pieces[i] != null)
                    data.bricks.Add(
                        ToRecord(
                            pieces[i]));
            }

            return data;
        }

        private void SaveBackup()
        {
            try
            {
                if (pieces.Count > 0)
                {
                    SaveSystem.Save(
                        2,
                        MakeSaveData());
                }
            }
            catch
            {
            }
        }

        private BrickRecord ToRecord(
            BrickPiece piece)
        {
            Color color =
                piece.PieceColor;

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

        private void Notify(
            UIFeedback feedback)
        {
            if (runtimeUI == null)
                return;

            runtimeUI.RefreshState();

            if (feedback !=
                UIFeedback.None)
            {
                runtimeUI.ShowFeedback(
                    feedback);
            }
        }

        private void Notify()
        {
            Notify(
                UIFeedback.None);
        }

        // ---------- Ready-made scenes ----------

        private void BuildHouseScene()
        {
            Color red =
                new Color(
                    0.86f,
                    0.18f,
                    0.14f);

            Color cream =
                new Color(
                    0.96f,
                    0.72f,
                    0.18f);

            Color roof =
                new Color(
                    0.12f,
                    0.38f,
                    0.82f);

            // Path and garden.
            PlaceSimple(
                "sidewalk",
                -1,
                0,
                -9,
                0,
                Color.white);

            PlaceSimple(
                "tree_round",
                -7,
                0,
                -2,
                0,
                Color.white);

            PlaceSimple(
                "bush",
                5,
                0,
                -2,
                0,
                Color.white);

            // A compact four-layer house block so the roof always has support.
            for (int layer = 0;
                 layer < 4;
                 layer++)
            {
                Color layerColor =
                    layer % 2 == 0
                    ? red
                    : cream;

                for (int z = -3;
                     z <= 3;
                     z += 2)
                {
                    PlaceSimple(
                        "2x8",
                        -4,
                        layer,
                        z,
                        1,
                        layerColor);
                }
            }

            // Architectural details sit on the front of the house.
            PlaceSimple(
                "door",
                -1,
                0,
                -4,
                0,
                new Color(
                    0.45f,
                    0.22f,
                    0.10f));

            PlaceSimple(
                "window",
                2,
                0,
                -4,
                0,
                new Color(
                    0.92f,
                    0.92f,
                    0.96f));

            PlaceSimple(
                "roof",
                -2,
                4,
                -1,
                0,
                roof);
        }

        private void BuildStreetScene()
        {
            PlaceSimple(
                "road_straight",
                -2,
                0,
                -9,
                0,
                Color.white);

            PlaceSimple(
                "road_straight",
                -2,
                0,
                -3,
                0,
                Color.white);

            PlaceSimple(
                "road_straight",
                -2,
                0,
                3,
                0,
                Color.white);

            PlaceSimple(
                "crosswalk",
                -2,
                0,
                -1,
                0,
                Color.white);

            PlaceSimple(
                "car",
                -1,
                0,
                -6,
                0,
                new Color(
                    0.90f,
                    0.18f,
                    0.10f));

            PlaceSimple(
                "bus",
                -1,
                0,
                2,
                0,
                new Color(
                    0.10f,
                    0.44f,
                    0.90f));

            PlaceSimple(
                "lamp",
                -4,
                0,
                -3,
                0,
                Color.white);

            PlaceSimple(
                "bench",
                4,
                0,
                -2,
                0,
                new Color(
                    0.50f,
                    0.28f,
                    0.12f));
        }

        private void BuildGardenScene()
        {
            PlaceSimple(
                "water",
                -2,
                0,
                -2,
                0,
                Color.white);

            PlaceSimple(
                "tree_pine",
                -6,
                0,
                -5,
                0,
                Color.white);

            PlaceSimple(
                "tree_round",
                4,
                0,
                -5,
                0,
                Color.white);

            PlaceSimple(
                "tree_round",
                -6,
                0,
                3,
                0,
                Color.white);

            PlaceSimple(
                "bush",
                4,
                0,
                3,
                0,
                Color.white);

            for (int i = -3;
                 i <= 3;
                 i += 2)
            {
                PlaceSimple(
                    "flower",
                    i,
                    0,
                    4,
                    0,
                    Color.white);
            }

            PlaceSimple(
                "rock",
                2,
                0,
                1,
                0,
                Color.white);

            PlaceSimple(
                "bench",
                -1,
                0,
                5,
                1,
                new Color(
                    0.48f,
                    0.28f,
                    0.14f));
        }

        private void BuildTowerScene()
        {
            Color red =
                new Color(
                    0.90f,
                    0.16f,
                    0.12f);

            Color yellow =
                new Color(
                    1.00f,
                    0.72f,
                    0.08f);

            Color green =
                new Color(
                    0.10f,
                    0.68f,
                    0.31f);

            Color blue =
                new Color(
                    0.08f,
                    0.39f,
                    0.92f);

            PlaceSimple(
                "2x8",
                -4,
                0,
                -1,
                1,
                red);

            PlaceSimple(
                "2x8",
                -1,
                1,
                -4,
                0,
                yellow);

            PlaceSimple(
                "2x8",
                -4,
                2,
                -1,
                1,
                green);

            PlaceSimple(
                "2x8",
                -1,
                3,
                -4,
                0,
                blue);

            PlaceSimple(
                "2x6",
                -3,
                4,
                -1,
                1,
                red);

            PlaceSimple(
                "2x4",
                -2,
                5,
                -1,
                1,
                yellow);

            PlaceSimple(
                "2x2",
                -1,
                6,
                -1,
                0,
                blue);
        }

        private void PlaceSimple(
            string id,
            int x,
            int y,
            int z,
            int rotation,
            Color color)
        {
            BrickRecord record =
                MakeRecord(
                    id,
                    x,
                    y,
                    z,
                    rotation,
                    color);

            PlaceRecord(
                record,
                false);
        }
    }
}
