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

        public BrickSpec Spec
        {
            get { return BrickCatalog.Get(BrickId); }
        }

        public int Width
        {
            get
            {
                BrickSpec s = Spec;
                return RotationStep % 2 == 0 ? s.width : s.depth;
            }
        }

        public int Depth
        {
            get
            {
                BrickSpec s = Spec;
                return RotationStep % 2 == 0 ? s.depth : s.width;
            }
        }

        public int HeightLayers
        {
            get { return Spec.heightLayers; }
        }

        public bool IsSurface
        {
            get { return Spec.isSurface; }
        }

        public bool GroundOnly
        {
            get { return Spec.groundOnly; }
        }

        public void Configure(
            string id,
            int gx,
            int gy,
            int gz,
            int rotationStep,
            Color color,
            bool preview,
            Mesh mesh)
        {
            BrickId = id;
            GridX = gx;
            GridY = gy;
            GridZ = gz;
            RotationStep = ((rotationStep % 4) + 4) % 4;
            PieceColor = color;
            IsPreview = preview;
            generatedMesh = mesh;
        }

        public void SetGridPosition(int gx, int gy, int gz)
        {
            GridX = gx;
            GridY = gy;
            GridZ = gz;
        }

        public void SetPreviewColor(Color color)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                BrickMaterialLibrary.SetSurface(
                    renderers[i],
                    color,
                    0.42f,
                    0.0f);
            }
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
