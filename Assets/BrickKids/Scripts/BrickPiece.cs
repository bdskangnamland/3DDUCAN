using UnityEngine;

namespace BrickKids3D
{
    public class BrickPiece : MonoBehaviour
    {
        public string BrickId { get; private set; }
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public int GridZ { get; private set; }
        public int RotationStep { get; private set; }
        public Color PieceColor { get; private set; }
        public bool IsPreview { get; private set; }

        public int Width
        {
            get
            {
                var s = BrickCatalog.Get(BrickId);
                return RotationStep % 2 == 0 ? s.width : s.depth;
            }
        }

        public int Depth
        {
            get
            {
                var s = BrickCatalog.Get(BrickId);
                return RotationStep % 2 == 0 ? s.depth : s.width;
            }
        }

        public void Configure(string id, int gx, int gy, int gz, int rotationStep, Color color, bool preview)
        {
            BrickId = id;
            GridX = gx;
            GridY = gy;
            GridZ = gz;
            RotationStep = ((rotationStep % 4) + 4) % 4;
            PieceColor = color;
            IsPreview = preview;
        }

        public void SetGridPosition(int gx, int gy, int gz)
        {
            GridX = gx;
            GridY = gy;
            GridZ = gz;
        }

        public void SetPreviewColor(Color color)
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                if (r.material != null) r.material.color = color;
            }
        }
    }
}
