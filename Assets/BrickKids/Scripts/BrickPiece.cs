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

        private Mesh generatedMesh;
        private Renderer cachedRenderer;

        public int Width
        {
            get
            {
                BrickSpec s = BrickCatalog.Get(BrickId);
                return RotationStep % 2 == 0 ? s.width : s.depth;
            }
        }

        public int Depth
        {
            get
            {
                BrickSpec s = BrickCatalog.Get(BrickId);
                return RotationStep % 2 == 0 ? s.depth : s.width;
            }
        }

        public void Configure(string id, int gx, int gy, int gz, int rotationStep, Color color, bool preview, Mesh mesh)
        {
            BrickId = id;
            GridX = gx;
            GridY = gy;
            GridZ = gz;
            RotationStep = ((rotationStep % 4) + 4) % 4;
            PieceColor = color;
            IsPreview = preview;
            generatedMesh = mesh;
            cachedRenderer = GetComponent<Renderer>();
        }

        public void SetGridPosition(int gx, int gy, int gz)
        {
            GridX = gx;
            GridY = gy;
            GridZ = gz;
        }

        public void SetPreviewColor(Color color)
        {
            if (cachedRenderer == null) cachedRenderer = GetComponent<Renderer>();
            BrickMaterialLibrary.SetColor(cachedRenderer, color);
        }

        private void OnDestroy()
        {
            if (generatedMesh != null)
            {
                Destroy(generatedMesh);
                generatedMesh = null;
            }
        }
    }
}
